using AppReceiver_Keyword_Analyst.Redis;
using AppReceiver_Keyword_Analyst_New.Interfaces;
using AppReceiver_Keyword_Analyst_New.Model;
using AppReceiver_Keyword_Analyst_New.Models;
using AppReceiver_Keyword_Analyst_New.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Utilities;


namespace AppReceiver_Keyword_Analyst_New
{

    class Program
    {
        private static QueueSettingGroupMappingModel queue_setting;
        private static IServiceProvider Service_Provider;
        private static IConfiguration _configuration;
        private static IAnalyticsService analytics_service;
        private static RedisService redis_service;

        static string log_path = @Directory.GetCurrentDirectory() + @"\log\";
        private static ChromeDriver browers;
        static void Main(string[] args)
        {
            try
            {
                //-- Configure Service: 
                ConfigureServices();
                //-- ChromeDriver
                browers = ChromeDriverInitilization();
                browers.Url = _configuration["Default_URL"];
                analytics_service = Service_Provider.GetService<IAnalyticsService>();
                var config = LoadConfiguration();
                var serviceProvider = LoadServices(config);
                redis_service = serviceProvider.GetService<RedisService>();
                int manual_delay = Convert.ToInt32(_configuration["Manual_Setup_Delay"]) < 0 ? 10000 : Convert.ToInt32(_configuration["Manual_Setup_Delay"]);
                Console.WriteLine("Waiting for Manual Setup in "+ (int)manual_delay / 1000+"s...");
                //Thread.Sleep(manual_delay);
                Thread.Sleep(2000);
                #region Test Data
                /*
                var queue_item = new SLQueueItem()
                {
                    cache_name= "KEYWORD_iphone_1",
                    keyword= "iphone",
                    label_Id=1,
                    page_index=3
                };
                TestFunction(queue_item);
                */
                #endregion
                //-- Getting Message:
                
                var factory = new ConnectionFactory()
                {
                    HostName = queue_setting.host,
                    UserName = queue_setting.username,
                    Password = queue_setting.password,
                    VirtualHost = queue_setting.v_host,
                    Port = queue_setting.port
                };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queue_setting.queue_name,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    Console.WriteLine(" Waiting from "+queue_setting.host+":"+queue_setting.port+" / "+queue_setting.queue_name);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (sender, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        string page_source = string.Empty;
                        var queue_item = JsonConvert.DeserializeObject<SLQueueItem>(message);
                        if (queue_item != null&& queue_item.keyword!=null&& queue_item.keyword!=""&& queue_item.cache_name != null && queue_item.cache_name != ""&& queue_item.label_Id>0  )
                        {
                            Console.WriteLine("---------------------------------------------------------------------------------");
                            Console.Write("--> Received - Keyword: " + queue_item.keyword + ". LabelID: " + queue_item.label_Id + ". Cache_name: " + queue_item.cache_name + /*". page_index:" + queue_item.page_index+ */  " \n");
                           // CorrectInputParam(queue_item);
                            var data = analytics_service.CrawlData(browers, queue_item).Result;
                            if (data.data == null || data.data.Count < 1)
                            {
                                Console.Write("Crawl Failed. No Result Found with Keyword. \n");
                                LogHelper.InsertLogTelegram("AppReceiver_Keyword_Analyst - Exucute: Empty Result for keyword - " + queue_item.keyword);

                            }
                            else
                            {
                                Console.Write("Crawl Successful. Count = " + data.data.Count + ". ");
                                if (redis_service.GetCacheRedis(queue_item.cache_name) != null)
                                {
                                    Console.Write("Exists in Cache. Override? : ");
                                    bool do_override = Convert.ToInt32(_configuration["Config:Override_Cache_IfExists"]) == 1;
                                    if (do_override)
                                    {
                                        redis_service.SetCacheRedis(queue_item.cache_name, JsonConvert.SerializeObject(data), DateTime.Now.AddMinutes(30));
                                        Console.Write("Yes. Cached - " + queue_item.cache_name + ".  ");
                                    }
                                    else
                                    {
                                        Console.Write("No. Cached Exists - " + queue_item.cache_name + ". ");
                                    }
                                }
                                else
                                {
                                    redis_service.SetCacheRedis(queue_item.cache_name, JsonConvert.SerializeObject(data), DateTime.Now.AddMinutes(30));
                                    Console.Write("Cached - " + queue_item.cache_name + ".  ");

                                }
                
                        }
                        }
                        else
                        {
                            Console.WriteLine("Invalid Message: " + message);
                            LogHelper.InsertLogTelegram("AppReceiver_Keyword_Analyst - Invalid Message: " + message);
                        }
                
                    };
                    channel.BasicConsume(queue: queue_setting.queue_name,
                                         autoAck: true,
                                         consumer: consumer);

                    Console.ReadLine();
               
                }
               
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AppReceiver_Keyword_Analyst - Excute Queue: " + ex.ToString());
                Console.WriteLine("execute Queue error: " + ex.ToString());
                Console.ReadLine();
            }
        }
        public static void TestFunction(SLQueueItem queue_item)
        {
            Console.WriteLine("---------------------------------------------------------------------------------");
            Console.Write("--> Received - Keyword: " + queue_item.keyword + ". LabelID: " + queue_item.label_Id + ". Cache_name: " + queue_item.cache_name + ". page_index:"/* + queue_item.page_index */+ " \n");
            CorrectInputParam(queue_item);
            var data = analytics_service.CrawlData(browers, queue_item).Result;
            if (data.data == null || data.data.Count < 1)
            {
                Console.Write("Crawl Failed. No Result Found with Keyword. \n");
                LogHelper.InsertLogTelegram("AppReceiver_Keyword_Analyst - Exucute: Empty Result for keyword - " + queue_item.keyword);

            }
            else
            {
                string path_list = @"C:\Users\Mirrors\Desktop\SVN Personal 2\Generate_Token\Generate_Token\output1.json";
                Console.Write("Crawl Successful. Count = " + data.data.Count + ". ");
                if (redis_service.GetCacheRedis(queue_item.cache_name) != null)
                {
                    Console.Write("Exists in Cache. Override? : ");
                    bool do_override = Convert.ToInt32(_configuration["Config:Override_Cache_IfExists"]) == 1;
                    if (do_override)
                    {

                        File.WriteAllText(path_list, JsonConvert.SerializeObject(data));
                        redis_service.SetCacheRedis(queue_item.cache_name, JsonConvert.SerializeObject(data), DateTime.Now.AddMinutes(30));
                        Console.Write("Yes. Cached - " + queue_item.cache_name + ".  ");
                    }
                    else
                    {
                        Console.Write("No. Cached Exists - " + queue_item.cache_name + ". ");
                    }
                }
                else
                {
                    File.WriteAllText(path_list, JsonConvert.SerializeObject(data));
                    redis_service.SetCacheRedis(queue_item.cache_name, JsonConvert.SerializeObject(data), DateTime.Now.AddMinutes(30));
                    Console.Write("Cached - " + queue_item.cache_name + ".  ");

                }
            }
        }
        private static ChromeDriver ChromeDriverInitilization()
        {
            try
            {
                string startupPath = Directory.GetCurrentDirectory();
                // setting SE
                var chrome_option = new ChromeOptions();
                chrome_option.AddArgument("--start-maximized"); // set full man hinh                                                                
                chrome_option.AddArgument("--user-agent=Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");
                //chrome_option.AddArgument("blink-settings=imagesEnabled=false");
                // chrome_option.AddArgument("--headless");
                // chrome_option.AddArgument("--disable-javascript");
                chrome_option.AddArgument("--disable-remote-fonts");
                chrome_option.AddArgument("--disable-extensions");
                chrome_option.AddArgument("--disable-notifications");
                chrome_option.AddArgument("--user-data-dir="+startupPath.Replace("\\","/")+"/profile");
                //string startupPath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\bin\Debug\netcoreapp3.1\", @"\");
                var browers = new ChromeDriver(startupPath, chrome_option);
                return browers;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AppReceiver_Keyword_Analyst - ChromeDriverInitilization: " + ex.ToString());
                Console.WriteLine("ChromeDriver Initilization Error: " + ex.ToString());
                return null;
            }
        }
        private static void CorrectInputParam(SLQueueItem queue_item)
        {
            try
            {
                //-- Correct Keyword:
                if(queue_item.keyword==null|| queue_item.keyword == "")
                {
                    string cache = "KEYWORD_";
                    string regex = Regex.Replace(queue_item.keyword.Trim(), "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
                    cache += regex + "_";
                    //cache += queue_item.page_index + "_";
                    cache += queue_item.label_Id;
                    queue_item.cache_name = cache;
                }
                //-- Correct Page_index:
                //if (queue_item.page_index < 1) queue_item.page_index = 1;

            }
            catch (Exception)
            {

            }

        }
        private static void ConfigureServices()
        {
            try
            {
                // Build Service Provider:
                IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json");
                IConfiguration config = configBuilder.Build();
                var services = new ServiceCollection();
                //-- Add Service:
                services.AddOptions();
                services.AddSingleton(config);
                services.AddSingleton<IAnalyticsService, AnalyticsServiceRepository>();

                //--  Additional Service for another Label go here: 
                services.AddSingleton<IAMZCrawlService, AMZCrawlRepository>();

                //-- Aditional Config:
                Service_Provider = services.BuildServiceProvider();
                _configuration = Service_Provider.GetService<IConfiguration>();
                string select_queue = _configuration["Select_Queue"];
                queue_setting = new QueueSettingGroupMappingModel
                {
                    host = _configuration[select_queue + ":HostName"],
                    v_host = _configuration[select_queue + ":VirtualHost"],
                    port = Convert.ToInt32(_configuration[select_queue + ":Port"])! <= 0 ? Convert.ToInt32(_configuration["RabbitMQ:Port"]) : 5672,
                    username = _configuration[select_queue + ":UserName"],
                    password = _configuration[select_queue + ":Password"],
                    queue_name = _configuration[select_queue + ":QueueName"],
                };
            } catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AppReceiver_Keyword_Analyst - ConfigureServices: " + ex.ToString());
            }
        }
        #region connection Redis
        static IServiceProvider LoadServices(IConfiguration configuration)
          => new ServiceCollection()
                  .AddRedis(builder =>
                     builder
                         .SetConfigurationOptions(configuration["ConnectionStrings:Redis"])

                  .SetDBIndex(Convert.ToInt32(configuration["ConnectionStrings:db_product_index"]))
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
