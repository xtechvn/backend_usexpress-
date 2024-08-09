using AppReceiverAnalysCrawler.Behaviors;
using AppReceiverAnalysCrawler.Common;
using AppReceiverAnalysCrawler.Engines;
using AppReceiverAnalysCrawler.Engines.Amazon;
using AppReceiverAnalysCrawler.Interfaces;
using AppReceiverAnalysCrawler.Redis;
using Entities.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Repositories.IRepositories;
using Repositories.Repositories;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Utilities;
using Utilities.Contants;

namespace AppReceiverAnalysCrawler
{
    /// <summary>
    /// Create By: Cuonglv
    /// NEED: App sẽ get dữ liệu từ Queue và đi crawler sản phẩm theo nhãn hàng
    /// PATTERN: DEPENDENCY INJECTION THEO SOLID
    /// </summary>
    class Program
    {
        public static string QUEUE_HOST = ConfigurationManager.AppSettings["QUEUE_HOST"];
        public static string QUEUE_V_HOST = ConfigurationManager.AppSettings["QUEUE_V_HOST"];
        public static string QUEUE_USERNAME = ConfigurationManager.AppSettings["QUEUE_USERNAME"];
        public static string QUEUE_PASSWORD = ConfigurationManager.AppSettings["QUEUE_PASSWORD"];
        public static string QUEUE_PORT = ConfigurationManager.AppSettings["QUEUE_PORT"];
        public static string QUEUE_KEY_API = ConfigurationManager.AppSettings["QUEUE_KEY_API"];

        private static string DOMAIN_WEBSITE_CRAWLER = ConfigurationManager.AppSettings["DOMAIN_WEBSITE_CRAWLER"];
        private static string delay_start = ConfigurationManager.AppSettings["delay_start"];
        private static int BOT_TYPE = Convert.ToInt16(ConfigurationManager.AppSettings["BOT_TYPE"]); /*1: NHiệm vụ crawl chi tiết sản phẩm  |  2: Nhiệm vụ crawl chi tiết sản phẩm và push len ES |  3: push sản phẩm manual lên ES*/

        private static string task_queue_crawl_realtime = TaskQueueName.product_crawl_queue; // Thực hiện Crawl và đẩy sang cho client 
        private static string task_queue_crawl_offline = TaskQueueName.group_product_mapping_detail; // Thực hiện crawl và đẩy vào ES
        private static string task_queue_manual_offline = TaskQueueName.product_detail_manual_queue; //push sản phẩm manual lên ES

        private static string startupPath = Directory.GetCurrentDirectory();
        private static string bot_index = ConfigurationManager.AppSettings["computer_index"];
        public static int is_headless = Convert.ToInt16(ConfigurationManager.AppSettings["is_headless"]);
        static void Main(string[] args)
        {
            LogHelper.InsertLogTelegram("Job " + bot_index + " start...");
            try
            {
                string queue_name = string.Empty;
                //// setting SE
                var chrome_option = new ChromeOptions();
                chrome_option.AddArgument("--start-maximized");
                chrome_option.AddArgument("--disable-remote-fonts");
                chrome_option.AddArgument("--disable-extensions");

                // SE READY...
                var browers = new ChromeDriver(startupPath, chrome_option);
                
                    //#region Truy cập vào website Amazon và đăng ký vùng miền
                    browers.Url = DOMAIN_WEBSITE_CRAWLER;

                    Thread.Sleep(Convert.ToInt32(delay_start)); // Chờ trang load xong + đủ time login                
                    //                    //#endregion

                    var factory = new ConnectionFactory()
                    {
                        HostName = QUEUE_HOST,
                        UserName = QUEUE_USERNAME,
                        Password = QUEUE_PASSWORD,
                        VirtualHost = QUEUE_V_HOST,
                        Port = Protocols.DefaultProtocol.DefaultPort
                    };
                    switch (BOT_TYPE)
                    {
                        case BotType.CRAWL_REALTIME:
                            queue_name = task_queue_crawl_realtime;
                            break;
                        case BotType.CRAWL_SCHEDULER:
                            queue_name = task_queue_crawl_offline;
                            break;
                        case BotType.SYNC_PRODUCT_MANUAL:
                            queue_name = task_queue_manual_offline;
                            break;
                        default:
                            Console.ReadLine();
                            break;
                    }

                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: queue_name, // chuyển đổi cơ chế đọc data từ Queue
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);

                        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                        Console.WriteLine(" [*] Waiting for messages.");

                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (sender, ea) =>
                        {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);

                            Console.WriteLine(" [x] Received message: {0}", message);
                            #region set up Dependency Injection
                            var serviceProvider = new ServiceCollection()
                                                        .AddSingleton<IAmazonCrawlerService, AmazonCrawlerService>()
                                                        .AddSingleton<IProductCrawlerFactory, ProductCrawlerFactory>()
                                                        .BuildServiceProvider();
                            #endregion

                            if (BOT_TYPE == BotType.SYNC_PRODUCT_MANUAL)
                            {
                                #region Sync ES                                                                
                                var product = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                                string product_manual_key_id = product["product_manual_key_id"].ToString();
                                string group_id = product["group_id"].ToString();
                                var product_crawler = serviceProvider.GetService<IProductCrawlerFactory>();
                                product_crawler.SyncElasticsearch(product_manual_key_id, group_id);

                                #endregion
                            }
                            else
                            {
                                #region CRAWL DATA

                                string page_source = string.Empty;
                                var product = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                                if (product.Count > 0)
                                {

                                    // detect Store
                                    int label_Id = -1;
                                    string product_code = string.Empty;
                                    string url = string.Empty;
                                    string page_type = string.Empty;
                                    int group_product_id = -10; // Nhóm sản phẩm detail crawl realtime chưa phân loại.

                                    label_Id = Convert.ToInt32(product["label_id"]); // store
                                    product_code = product["product_code"]; // Mã sp
                                    url = product["url"]; // link sp detail

                                    if (BOT_TYPE == BotType.CRAWL_REALTIME)
                                    {
                                        // detect Store
                                        page_type = product["page_type"];
                                        group_product_id = -10; // Nhóm sản phẩm detail crawl realtime chưa phân loại.
                                    }
                                    else if (BOT_TYPE == BotType.CRAWL_SCHEDULER)
                                    {
                                        page_type = TaskQueueName.product_detail_amazon_crawl_queue;
                                        group_product_id = Convert.ToInt32(product["group_id"]);
                                    }



                                    //Do the actual work here
                                    var product_crawler = serviceProvider.GetService<IProductCrawlerFactory>();
                                    product_crawler.DoSomeRealWork(page_type, product_code, label_Id, url, browers, group_product_id, BOT_TYPE);
                                }
                                else
                                {
                                    // writelog tele
                                }
                                #endregion
                            }
                        };
                        channel.BasicConsume(queue: queue_name,
                                             autoAck: true,
                                             consumer: consumer);

                        Console.WriteLine(" Press [enter] to exit.");
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





    }
}
