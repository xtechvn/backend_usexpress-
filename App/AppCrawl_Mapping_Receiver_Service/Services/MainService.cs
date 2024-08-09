using App_Crawl_Mapping_Receiver_Service_v2.LabelServices;
using App_Crawl_Mapping_Receiver_Service_v2.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace App_Crawl_Mapping_Receiver_Service_v2.Services
{
    public class MainService
    {
        static int queue_push_delay = 1000;
        private readonly ILogger<Worker> _logger;
        private IConfiguration _configuration;
        private ChromeDriver _browers;
        private IServiceProvider _ServiceProvider;
        public MainService(ILogger<Worker> logger)
        {
            _logger = logger;
            ConfigureServices();
            _browers = ChromeDriverInitilization();
        }
        public async Task ExcuteMainService()
        {
            try
            {
                _browers.Url = "https://www.amazon.com/";
                var is_success = await CheckAmazonLogin(_browers, _configuration);
                if (!is_success)
                {
                    Console.WriteLine("Automatic Login Failed. Please do Manual Login within 20 seconds from now ... ");
                    Thread.Sleep(20 * 1000);
                }
                queue_push_delay = Convert.ToInt32(_configuration["Delay:QueueDelay"]) < 1 ? 1000 : Convert.ToInt32(_configuration["Delay:QueueDelay"]);
                var queue_setting = new QueueSettingGroupMappingModel
                {
                    host = _configuration["Queue:HostName"],
                    v_host = _configuration["Queue:VirtualHost"],
                    port = Convert.ToInt32(_configuration["Queue:Port"])! <= 0 ? Convert.ToInt32(_configuration["Queue:Port"]) : 5672,
                    username = _configuration["Queue:UserName"],
                    password = _configuration["Queue:Password"],
                    queue_name = _configuration["Queue:QueueName"],
                    queue_name_detail = _configuration["Queue:QueueName_Detail"]
                };
                // Excute Queue
                var factory = new ConnectionFactory()
                {
                    HostName = queue_setting.host,
                    UserName = queue_setting.username,
                    Password = queue_setting.password,
                    VirtualHost = queue_setting.v_host,
                    Port = queue_setting.port
                };
                string QUEUE_NAME = queue_setting.queue_name;
                string queue_out_detail = queue_setting.queue_name_detail;
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: QUEUE_NAME,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    Console.WriteLine("Waiting from: " + queue_setting.host + " / " + QUEUE_NAME + "... ");
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += async (sender, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Console.Write("--- Received: ");
                        var queue_item = JsonConvert.DeserializeObject<SLQueueItem>(message);
                        await ExtractData(queue_item, message);


                    };
                    channel.BasicConsume(queue: QUEUE_NAME,
                                         autoAck: true,
                                         consumer: consumer);

                    Console.ReadLine();
                }
                Console.WriteLine("Excute Completed.");
            }
            catch (TimeoutException ex)
            {
                _logger.LogInformation("Excute Error - Timeout: " + ex.ToString());
                LogHelper.InsertLogTelegram("App_Crawl_Mapping_Receiver_Service_v2 - Timeout Error:" + ex.ToString());
            }
        }
        private async Task<List<SLProductItem>> ExtractData(SLQueueItem queue_item, string message)
        {
            string queue_out_detail = _configuration["Queue:QueueName_Detail"]; 
            List<SLProductItem> result_list = new List<SLProductItem>();
            int lv2_max_item = Convert.ToInt32(_configuration["Lv2MaxItemCrawl"]) < 2 ? 0 : Convert.ToInt32(_configuration["Lv2MaxItemCrawl"]);
            if (queue_item == null || !queue_item.linkdetail.Contains("http"))
            {
                _logger.LogInformation("Invaild Data: " + message + " \n");
                return new List<SLProductItem>();
            }
            _logger.LogInformation("Group Product ID: " + queue_item.groupProductid + ".  LabelID: " + queue_item.labelid + " - URL: " + queue_item.linkdetail + " - \n");
            switch (queue_item.labelid)
            {
                case (int)LabelType.amazon:
                    {
                        try
                        {
                            AmazonService amazonService = new AmazonService(_configuration);
                            /*
                            var result = amazonService.CrawlByRegex(queue_item, _browers);
                            if (result.list_product != null && result.list_product.Count >0)
                            {
                                result_list = result.list_product;
                                if (result.list_url.Count > 0)
                                {
                                    foreach (var url in result.list_url)
                                    {
                                        var rs_lv2 = amazonService.CrawlByRegex(url, _browers, true, 5);
                                        if (rs_lv2.list_product.Count > 0)
                                        {
                                            result_list.AddRange(rs_lv2.list_product);
                                        }
                                    }
                                }
                            }*/
                            //-- Get By Xpath:
                            if (result_list.Count <1) 
                            {
                                var rs = amazonService.CrawlByXpath(queue_item, _browers);
                                if (rs.list_product != null && rs.list_product.Count > 0)
                                {
                                    result_list = rs.list_product;
                                    _logger.LogInformation("LV1 Count = " + rs.list_product.Count);
                                    
                                    if (rs.list_url!=null && rs.list_url.Count > 0)
                                    {
                                        _logger.LogInformation("LV2 Count = " + rs.list_url.Count);
                                        foreach (var url in rs.list_url)
                                        {
                                            var rs_lv2 = amazonService.CrawlByXpath(url, _browers, true, lv2_max_item);
                                            if (rs_lv2.list_product!=null&& rs_lv2.list_product.Count > 0 && result_list.Count<60)
                                            {
                                                result_list.AddRange(rs_lv2.list_product);
                                            }
                                        }
                                    }
                                }
                            }
                            //-- Complteted, push to queue.
                            if (result_list.Count > 0)
                            {
                                //-- Push Result to Queue:
                                //  var excute_list = _filterService.FilterNonExistsProductCode(product_list, queue_item).Result;
                                _logger.LogInformation("Success. Count = " + result_list.Count);
                                string pcode_str = ""; int pcount = 0;
                                foreach (var product in result_list)
                                {
                                    var queue_push_result = PushDataToQueueAPI(_configuration["API:Queue"], JsonConvert.SerializeObject(product), queue_out_detail).Result;
                                    if (queue_push_result)
                                    {
                                        pcode_str += product.product_code + ", ";
                                        pcount++;
                                    }
                                    if (pcount > 60) break;
                                   // Thread.Sleep(queue_push_delay);
                                }
                                _logger.LogInformation("Push Queue Success - Item Count: " + pcount + " item(s) at : " + DateTime.Now + ". \n");

                            }
                        } catch(Exception ex)
                        {
                            _logger.LogInformation("App_Crawl_Mapping_Receiver_Service - ExtractData - Amazon - Error: "+ex.ToString());
                        }
                    }
                    break;
                default: break;
            }
            if (result_list.Count < 1)
            {
                _logger.LogInformation("App_Crawl_Mapping_Receiver_Service - Crawl: Error - No Result With Any Case. URL: " + queue_item.linkdetail + "  , Group-ID: " + queue_item.groupProductid + "  , Label: " + queue_item.labelid);
                LogHelper.InsertLogTelegram("App_Crawl_Mapping_Receiver_Service - Crawl: Error - No Result With Any Case. URL: " + queue_item.linkdetail + "  , Group-ID: " + queue_item.groupProductid + "  , Label: " + queue_item.labelid);
            }
            List<string> strings = result_list.Select(s => s.url).ToList();
            return result_list;
        }
        private void ConfigureServices()
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
            IConfiguration config = configBuilder.Build();
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddSingleton(config);
            _ServiceProvider = services.BuildServiceProvider();
            _configuration = _ServiceProvider.GetService<IConfiguration>();
        }
        private ChromeDriver ChromeDriverInitilization()
        {
            try
            {
                var chrome_option = new ChromeOptions();
                chrome_option.AddArgument("--start-maximized");
                // chrome_option.AddArgument("--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");
                chrome_option.AddArgument("--disable-remote-fonts");
                chrome_option.AddArgument("--disable-extensions");
                //chrome_option.AddArgument("--incognito");
                //string startupPath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\bin\Debug\netcoreapp3.1\", @"\");
                string startupPath = Directory.GetCurrentDirectory();
                var browers = new ChromeDriver(startupPath, chrome_option);
                return browers;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("App_Crawl_Mapping_Receiver_Service_v2 - ChromeDriverInitilization: " + ex.ToString());
                Console.WriteLine("App_Crawl_Mapping_Receiver_Service_v2 - ChromeDriver Initilization Error: " + ex.ToString());
                return null;
            }
        }
        public async Task<bool> CheckAmazonLogin(ChromeDriver chromeDriver, IConfiguration _configuration)
        {
            IWebElement element = null;
            if (CommonServices.IsElementPresent(By.Id("nav-signin-tooltip"), chromeDriver, out element)) // nếu chưa login
            {
                var aTag = element.FindElement(By.TagName("a"));
                var urlLogin = aTag.GetAttribute("href");
                chromeDriver.Url = urlLogin;
            }
            if (chromeDriver.Url.Contains("https://www.amazon.com/ap/signin"))
            {
                element = null;
                if (CommonServices.IsElementPresent(By.Id("ap-account-switcher-container"), chromeDriver, out element))
                {
                    element = null;
                    if (CommonServices.IsElementPresent(By.XPath("//div[contains(@class,'cvf-account-switcher-claim')]"), chromeDriver, out element))
                    {
                        chromeDriver.ExecuteScript("arguments[0].click();", element);
                        return true;
                    }
                }
                await Task.Delay(1000);
                element = null;
                if (CommonServices.IsElementPresent(By.Id("ap_email"), chromeDriver, out element))
                {
                    if (element.Displayed)
                    {
                        element.Clear();
                        element.SendKeys(_configuration["Login:Username"]);
                    }
                    await Task.Delay(1000);

                    IWebElement eleBtnContinue = null;
                    if (CommonServices.IsElementPresent(By.XPath("//input[@id='continue' and contains(@aria-labelledby,'continue-announce')]"), chromeDriver, out eleBtnContinue))
                    {
                        chromeDriver.ExecuteScript("arguments[0].click();", eleBtnContinue);
                    }
                }
                await Task.Delay(1000);

                element = null;
                if (CommonServices.IsElementPresent(By.Id("ap_password"), chromeDriver, out element))
                {
                    if (element.Displayed)
                    {
                        element.Clear();
                        element.SendKeys(_configuration["Login:Password"]);
                    }
                    await Task.Delay(1000);

                    element = null;
                    if (CommonServices.IsElementPresent(By.XPath("//input[@name='rememberMe']"), chromeDriver, out element))
                    {
                        if (element.GetAttribute("checked") == "false")
                        {
                            chromeDriver.ExecuteScript("arguments[0].click();", element);

                        }
                    }
                    await Task.Delay(3000);

                    element = null;
                    if (CommonServices.IsElementPresent(By.Id("signInSubmit"), chromeDriver, out element))
                    {
                        chromeDriver.ExecuteScript("arguments[0].click();", element);
                        if (chromeDriver.Url.Contains("/signin"))
                        {
                            return false;
                        }
                        Thread.Sleep(1000);
                        return true;
                    }
                }
            }
            return true;
        }
        public async Task<bool> PushDataToQueueAPI(string url, string message, string queue_name)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiPrefix = url;

                string j_param = "{'data_push':'" + message + "','type':'" + queue_name + "'}";
                string token = CommonHelper.Encode(j_param, _configuration["Key:Encrypt"]);
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("token", token),
                });
                var result = await httpClient.PostAsync(apiPrefix, content);
                var rs_content = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Content.ReadAsStringAsync().Result);
                if (rs_content["status"] == ResponseType.SUCCESS.ToString())
                    return true;
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}
