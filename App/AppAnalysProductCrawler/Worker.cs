using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppAnalysProductCrawler.Models;
using Caching.RedisWorker;
using Entities.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;

namespace AppAnalysProductCrawler
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        public static string KEY_TOKEN_API = ConfigurationManager.AppSettings["KEY_TOKEN_API"];
        public static string API_CRAWL_DETAIL_PRODUCT = ConfigurationManager.AppSettings["API_CRAWL_DETAIL_PRODUCT"];
        public static string API_CMS_URL = ConfigurationManager.AppSettings["API_CMS_URL"];
        public static string QUEUE_BLACK_FRIDAY = ConfigurationManager.AppSettings["QUEUE_BLACK_FRIDAY"];
        public static string data_feed = ConfigurationManager.AppSettings["data_feed"];
        public static string QUEUE_HOST = ConfigurationManager.AppSettings["QUEUE_HOST"];
        public static string QUEUE_V_HOST = ConfigurationManager.AppSettings["QUEUE_V_HOST"];
        public static string QUEUE_USERNAME = ConfigurationManager.AppSettings["QUEUE_USERNAME"];
        public static string QUEUE_PASSWORD = ConfigurationManager.AppSettings["QUEUE_PASSWORD"];
        public static string QUEUE_PORT = ConfigurationManager.AppSettings["QUEUE_PORT"];
        public static int db_folder = int.Parse(ConfigurationManager.AppSettings["db_folder"]);
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    QueueConsumer();
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(2000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                var err = ex.Message;
            }
        }
        public void QueueConsumer()
        {
            Console.WriteLine("Chay job AppAnalysProductCrawler");
            var factory = new ConnectionFactory()
            {
                HostName = QUEUE_HOST,
                UserName = QUEUE_USERNAME,
                Password = QUEUE_PASSWORD,
                VirtualHost = QUEUE_V_HOST,
                Port = Protocols.DefaultProtocol.DefaultPort
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: data_feed,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (sender, ea) =>
              {
                  var body = ea.Body.ToArray();
                  var message = Encoding.UTF8.GetString(body);

                  #region Get data from Queue & Analys
                  var productDetailModel = JsonConvert.DeserializeObject<ProductDetailModel>(message);
                  Thread.Sleep(8000);
                  await ProcessAppAnalysProductCrawler(productDetailModel);
                  #endregion

                  Console.WriteLine("[" + DateTime.Now.ToString() + "] - [x] Process Success - Data Received {0}", message);
              };
                channel.BasicConsume(queue: data_feed,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        async Task ProcessAppAnalysProductCrawler(ProductDetailModel productDetailModel)
        {
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).
                        AddJsonFile("appsettings.json").Build();

            var _RedisService = new RedisConn(config);
            _RedisService.Connect();
            var redisConfig = config.GetSection("Redis").Get<IPConfig>();
            string StrRedisConfig = $"{redisConfig.Host}:{redisConfig.Port},connectRetry=5";

            //IRedisRepository _RedisRepository = new RedisRepository(StrRedisConfig);
            var listProduct = new List<ProductViewModel>();
            try
            {
                var j_product = _RedisService.Get(productDetailModel.cache_name, db_folder).Result;
                if (j_product != null)
                {
                    //Thread.Sleep(2000);
                    listProduct = JsonConvert.DeserializeObject<List<ProductViewModel>>(j_product);
                }
            }
            catch (Exception)
            {
                //_RedisService.Set(productDetailModel.cache_name, JsonConvert.SerializeObject(listProduct), db_folder);
            }

            var existsProductCode = listProduct.FirstOrDefault(n => n.product_code == productDetailModel.product_code);

            if (existsProductCode != null)
            {
                listProduct.Remove(existsProductCode);
                var productViewModel = await CrawlData(productDetailModel.product_code, productDetailModel.url, productDetailModel.shop_id, productDetailModel.label_id);
                if (productViewModel != null && !string.IsNullOrEmpty(productViewModel.product_code))
                    listProduct.Add(productViewModel);
                Console.WriteLine("Da cap nhat cache cho san pham:  " + productViewModel.product_code);
            }
            else
            {
                var productViewModel = await CrawlData(productDetailModel.product_code, productDetailModel.url, productDetailModel.shop_id, productDetailModel.label_id);
                if (productViewModel != null && !string.IsNullOrEmpty(productViewModel.product_code))
                {
                    listProduct.Add(productViewModel);
                    Console.WriteLine("Da set cache cho san pham:  " + productViewModel.product_code + " i=" + listProduct.Count);
                }
            }


            if (listProduct.Count > 0)
                _RedisService.Set(productDetailModel.cache_name, JsonConvert.SerializeObject(listProduct), db_folder);
        }

        /// <summary>
        /// hàm crawl data
        /// </summary>
        /// <param name="ASIN"></param>
        /// <param name="link"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static async Task<ProductViewModel> CrawlData(string ASIN, string link, string shop_id, int groupId)
        {
            try
            {
                //Thread.Sleep(2000);
                HttpClient httpClient = new HttpClient();
                var apiPrefix = API_CMS_URL + API_CRAWL_DETAIL_PRODUCT;
                string j_param = "{'product_code':'" + ASIN + "','url':'" + link + "','shop_id':'app_crawl_page'}";
                string token = CommonHelper.Encode(j_param, KEY_TOKEN_API);
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("token", token),
                });
                var result = await httpClient.PostAsync(apiPrefix, content);
                var rs = result.Content.ReadAsStringAsync().Result;
                var product_detail = JsonConvert.DeserializeObject<ProductViewModel>
                    (result.Content.ReadAsStringAsync().Result);
                return product_detail;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
