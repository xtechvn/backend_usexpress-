using AppReceiverAnalysCrawler.Redis;
using AppReceiverAnalysCrawler_Jomashop.Common;
using AppReceiverAnalysCrawler_Jomashop.Cores;
using AppReceiverAnalysCrawler_Jomashop.Interfaces;
using AppReceiverAnalysCrawler_Jomashop.Models;
using Caching.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Utilities;
using Utilities.Contants;

namespace AppReceiverAnalysCrawler_Jomashop
{
    class Program
    {
        private static string startupPath = Environment.CurrentDirectory;
        public static ChromeDriver _chromeDriver;
        private static IConfiguration _configuration;
        private static IJomaCrawler _jomaCrawler;
        private static RedisService _redisService;
        private static Stopwatch time_measurement = new Stopwatch();
        private static IESRepository<object> _ESRepository;

        static void Main(string[] args)
        {
            try
            {
                // Set up Dependency Injection
                var serviceProvider = new ServiceCollection()
                                               .AddSingleton<IJomaCrawler, JomaCrawlerV2>()
                                            //   .AddSingleton<IJomaCrawler, JomaCrawler>()
                                            .BuildServiceProvider();

                _jomaCrawler = serviceProvider.GetService<IJomaCrawler>();
                _configuration = LoadConfiguration();
                var serviceProvider2 = LoadServices(_configuration);
                _redisService = serviceProvider2.GetService<RedisService>();
                //-- ES Service:
                _ESRepository = new ESRepository<object>(_configuration["ElasticSearch"]);
                // Setting SE Chrome Driver:
                var chrome_option = new ChromeOptions();
                chrome_option.AddArgument("--start-maximized"); // set full man hinh
                // chrome_option.AddArgument("--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");
                if (_configuration["Setting:Crawl_Image_Enable"].Trim() == "0")
                {
                    chrome_option.AddArgument("blink-settings=imagesEnabled=false");
                }
                if (_configuration["Setting:JS_Enabled"].Trim() == "0")
                {
                    chrome_option.AddArgument("--disable-javascript");
                }
                chrome_option.AddArgument("--log-level=3"); //Start Silently.
                chrome_option.AddArgument("--disable-remote-fonts");
                chrome_option.AddArgument("--disable-extensions");
                //chrome_option.AddArgument("--disable-remote-fonts");
                _chromeDriver = new ChromeDriver(startupPath, chrome_option);
                _chromeDriver.Url = "https://www.jomashop.com/";
                try
                {
                    _chromeDriver.Navigate().GoToUrl("https://www.jomashop.com/automatic-watches.html");
                    Thread.Sleep(1000);
                    var xpath = "//li[contains(@class,\"productItem\")]//a[contains(@class,\"productName-link\")]";
                    if (LocalHelper.IsElementPresentByXpath(_chromeDriver, xpath))
                    {
                        var elements = _chromeDriver.FindElements(By.XPath(xpath));
                        var url_preload = elements[0].GetAttribute("href");
                        if (!url_preload.StartsWith("https://www.jomashop.com/"))
                        {
                            _chromeDriver.Navigate().GoToUrl("https://www.jomashop.com/" + url_preload);
                        }
                        else
                        {
                            _chromeDriver.Navigate().GoToUrl(url_preload);
                        }
                        Thread.Sleep(3000);
                    }
                }
                catch (Exception)
                {

                }
                var factory = new ConnectionFactory()
                {
                    HostName = _configuration["RabbitMQ:HostName"],
                    UserName = _configuration["RabbitMQ:UserName"],
                    Password = _configuration["RabbitMQ:Password"],
                    VirtualHost = _configuration["RabbitMQ:VirtualHost"],
                    Port = Convert.ToInt32(_configuration["RabbitMQ:Port"])
                };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: _configuration["RabbitMQ:QueueName_request"], // chuyển đổi cơ chế đọc data từ Queue
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    Console.WriteLine("Waiting from: " + _configuration["RabbitMQ:HostName"] + "/" + _configuration["RabbitMQ:Port"] + " - " + _configuration["RabbitMQ:QueueName_request"]);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (sender, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var request = JsonConvert.DeserializeObject<QueueMessage>(message);
                        //Đo thời gian:
                        time_measurement.Restart();

                        #region Crawl V1 - Disabled
                        //-- Crawl V1:
                        /*
                        var product_detail = _jomaCrawler.CrawlDetail(_chromeDriver, _configuration, request).Result;
                        var msg = product_detail.product_infomation_HTML;
                        product_detail.product_infomation_HTML = null;
                        Console.WriteLine("Time: " + time_measurement.ElapsedMilliseconds + " ms.");
                        Console.WriteLine(msg);
                        //-- Product Detail is found:
                        if (product_detail != null && product_detail.page_not_found == false && product_detail.product_code != null && product_detail.product_code.Trim() != "")
                        {

                            Console.WriteLine("Crawl Detail Success: \nProduct_Code = " + product_detail.product_code + " . Price: " + product_detail.amount);
                            //-- Push ES:
                            // Build id identity:
                            var product_response = _ESRepository.DeleteProductByCode("product", product_detail.product_code);
                            if (product_response)
                            {
                                long unixTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                                product_detail.id = unixTime;
                                var result_push_es = _ESRepository.UpSert(product_detail, "product");
                                if (result_push_es > 0)
                                {
                                    Console.Write("Push ES Success. ");
                                }
                                else
                                {
                                    Console.Write("Push ES Failed. ");
                                }
                            }
                            //-- Push Redis:
                            var cache_name = JomaHelper.GetCacheNameFromURL(request.url);
                            _redisService.SetCacheRedis(cache_name, JsonConvert.SerializeObject(product_detail), DateTime.Now.AddHours(2));
                            Console.WriteLine("Push Redis Success.");
                            //-- Show Result:
                            time_measurement.Stop();
                            Console.WriteLine("Excute Time: " + time_measurement.ElapsedMilliseconds + " ms. Completed at: " + DateTime.Now.ToString());
                            LogHelper.InsertLogTelegram("AppReceiverAnalysCrawler_Jomashop - CrawlDetail - Success with: " + message + ".\nProduct_Code = " + product_detail.product_code + " . Price: " + product_detail.amount + ". " + msg + " . Time: " + DateTime.Now.ToString() + "\nExcute Time: " + time_measurement.ElapsedMilliseconds + " ms.", _configuration["Setting:ID"].Trim());
                        }
                        else
                        {
                            Console.WriteLine(msg);
                            Console.WriteLine("URL is unvailable or cannot get detail: " + message);
                            LogHelper.InsertLogTelegram("AppReceiverAnalysCrawler_Jomashop - Main - URL is unvailable or cannot get detail: " + message + ". MSG: " + msg, _configuration["Setting:ID"].Trim());
                        }
                        */
                        #endregion

                        #region Crawl V2 - Enable
                        var crawl =  _jomaCrawler.CrawlDetailV2(_chromeDriver, _configuration, request).Result;
                        if(crawl.status != (int)MethodOutputStatusCode.Success)
                        {
                            Console.WriteLine("URL is unvailable or cannot get detail: " + crawl.message);
                            LogHelper.InsertLogTelegram("AppReceiverAnalysCrawler_Jomashop - Main - URL is unvailable or cannot get detail: " + message + ". MSG: " + crawl.message, _configuration["Setting:ID"].Trim());
                        }
                        else if (crawl.product != null && crawl.product.page_not_found == false && crawl.product.product_code != null && crawl.product.product_code.Trim() != "")
                        {
                            Console.WriteLine("Crawl Detail Success: \nProduct_Code = " + crawl.product.product_code + " . Price: " + crawl.product.amount);
                            //-- Push Redis:
                            var cache_name = CacheHelper.getProductDetailCacheKeyFromURL(request.url,crawl.product.label_id);
                            _redisService.SetCacheRedis(cache_name, JsonConvert.SerializeObject(crawl.product), DateTime.Now.AddHours(2));
                            Console.WriteLine("Push Redis Success.");

                            //-- Push ES:
                            // Build id identity:
                            if (_configuration["Setting:PushES"].Trim()=="1")
                            {
                                var product_response = _ESRepository.DeleteProductByCode("product", crawl.product.product_code);
                                if (product_response)
                                {
                                    long unixTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                                    crawl.product.id = unixTime;
                                    var result_push_es = _ESRepository.UpSert(crawl.product, "product");
                                    if (result_push_es > 0)
                                    {
                                        Console.Write("Push ES Success. ");
                                    }
                                    else
                                    {
                                        Console.Write("Push ES Failed. ");
                                    }
                                }

                            }
                            //-- Show Result:
                            time_measurement.Stop();
                            Console.WriteLine("Excute Time: " + time_measurement.ElapsedMilliseconds + " ms. Completed at: " + DateTime.Now.ToString());
                        }
                        #endregion
                    };
                    channel.BasicConsume(queue: _configuration["RabbitMQ:QueueName_request"],
                                         autoAck: true,
                                         consumer: consumer);

                    Console.WriteLine("Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                //LogHelper.InsertLogTelegram("AppReceiverAnalysCrawler: " + ex.ToString());
                Console.WriteLine("execute Queue error: " + ex.ToString());
                Console.ReadLine();
            }
        }
        #region connection Redis
        static IServiceProvider LoadServices(IConfiguration configuration)
          => new ServiceCollection()
                  .AddRedis(builder =>
                     builder
                         .SetConfigurationOptions(configuration["Redis:HostURL"])
                  //.SetKeyPrefix("usexpress_")

                  .SetDBIndex(Convert.ToInt32(configuration["Redis:db_joma"]))
                  )
                  .AddTransient<RedisService>()
                  .BuildServiceProvider();

        static IConfiguration LoadConfiguration()
            => new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")

                    .Build();
        #endregion
    }
}
