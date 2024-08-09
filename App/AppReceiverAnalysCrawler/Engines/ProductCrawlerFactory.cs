using AppReceiverAnalysCrawler.Behaviors;
using AppReceiverAnalysCrawler.Common;
using AppReceiverAnalysCrawler.Interfaces;

using AppReceiverAnalysCrawler.Redis;
using Caching.Elasticsearch;
using Crawler.ScraperLib.Amazon;
using CsQuery.Utility;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.CompilerServices;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Utilities;
using Utilities.Contants;

namespace AppReceiverAnalysCrawler.Engines
{
    // setup FACTORY. Contain with service
    public class ProductCrawlerFactory : IProductCrawlerFactory
    {
        private string bot_index = ConfigurationManager.AppSettings["computer_index"];
        private string elastic_config = ConfigurationManager.AppSettings["elastic_config"];
        public static string KEY_CONNECT_API_USEXPRESS = ConfigurationManager.AppSettings["KEY_CONNECT_API_USEXPRESS"];
        public static string URL_API_USEXPRESS = ConfigurationManager.AppSettings["API_USEXPRESS"];
        private readonly IAmazonCrawlerService _product_amazon_crawler;

        public ProductCrawlerFactory(IAmazonCrawlerService product_amazon_crawler)
        {
            _product_amazon_crawler = product_amazon_crawler;
        }
        public void DoSomeRealWork(string page_type, string product_code, int label_Id, string url, ChromeDriver browers, int group_product_id, int BOT_TYPE)
        {
            var product_detail_result = new ProductViewModel();
            var sw = new Stopwatch();            
            try
            {
                sw.Start();
                // delay cho pageload xong. apply crawl offline. background
                if (BOT_TYPE == BotType.CRAWL_SCHEDULER)
                {
                    browers.Manage().Timeouts().PageLoad.Add(System.TimeSpan.FromSeconds(2));
                }

                string page_source = string.Empty;
                var config = LoadConfiguration();

                // Phân luồng Redis và ES
                // Kiểm tra có trên ES chưa. Có rồi thì ko push

                var serviceProvider = LoadServices(config, 1);
                //var redis_worker = serviceProvider.GetService<RedisService>();

                string cache_name = CacheHelper.cacheKeyProductDetail(product_code, label_Id);

                switch (page_type)
                {
                    case TaskQueueName.product_detail_amazon_crawl_queue:

                        #region Tiến trình 1
                        browers.Url = url;
                        // browers.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);                                                  
                        product_detail_result = _product_amazon_crawler.crawlerProductAmazon(browers, url, product_code, group_product_id);
                        #endregion

                        break;
                }

                if (product_detail_result == null)
                {
                    // writelog tele
                    // push vao queue. Neu qua 3 lan ban len tele
                    // LogHelper.InsertLogTelegram("DoSomeRealWork: product_code: " + product_code + " crawl failed");
                }
                else
                {
                    if (product_detail_result.page_not_found)
                    {
                        // LogHelper.InsertLogTelegram("DoSomeRealWork: product_code: " + product_code + " crawl failed, url=" + url);
                        // set cache san pham detail crawl duoc. 
                        //redis_worker.SetCacheRedis(cache_name, JsonConvert.SerializeObject(product_detail_result), DateTime.Now.AddHours(1));
                    }
                    else
                    {
                        //if (BOT_TYPE == BotType.CRAWL_REALTIME)
                        //{
                        //    // set cache san pham detail crawl duoc để hiển thị ra trước. 
                        //    product_detail_result.group_product_id = group_product_id;
                        //    redis_worker.SetCacheRedis(cache_name, JsonConvert.SerializeObject(product_detail_result), DateTime.Now.AddHours(1));
                        //}

                        #region Tiến trình 2: thực hiện crawl tiếp phần cân nặng.                        
                        var js = (IJavaScriptExecutor)browers;
                        js.ExecuteScript("window.scrollTo(0,700)");

                        if (!product_detail_result.is_crawl_weight)
                        {
                            Thread.Sleep((Convert.ToInt32(config["total_delay_crawl_part_2"])));
                            page_source = browers.PageSource;

                            product_detail_result = _product_amazon_crawler.crawlProductMoreAmazon(browers, product_detail_result, page_source);
                            if (product_detail_result != null)
                            {
                                product_detail_result.regex_step += 1; // ghi nhận kết quả crawl xong tiến trình 2
                                if (BOT_TYPE == BotType.CRAWL_REALTIME)
                                {
                                    // Update lại giá theo cân nặng lấy được
                                    //redis_worker.SetCacheRedis(cache_name, JsonConvert.SerializeObject(product_detail_result), DateTime.Now.AddHours(1));
                                }
                            }
                        }
                        #endregion

                        #region Tiến trình 3: thực hiện crawl tiếp phần giá sản phẩm trong listing seller
                        //if (product_detail_result.is_has_seller)
                        //{
                        //    //Thread.Sleep(500); olp-new
                        //    var seller_list = _product_amazon_crawler.getPriceBySellers(browers, page_source);
                        //    if (seller_list != null)
                        //    {
                        //        product_detail_result.seller_list = seller_list;
                        //    }

                        //}
                        #endregion
                    }
                }

                #region Push ElasticSearch    
                //if (BOT_TYPE == BotType.CRAWL_SCHEDULER && !product_detail_result.page_not_found)
                //{
                //    IESRepository<object> _ESRepository = new ESRepository<object>(elastic_config);
                //    // Build id identity:
                //    var product_response = _ESRepository.DeleteProductByCode("product", product_code);
                //    if (product_response) // delete thành công thì mới dc add new
                //    {
                //        long unixTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                //        product_detail_result.id = unixTime;

                //        var result_push_es = _ESRepository.UpSert(product_detail_result, "product");
                //        if (result_push_es > 0)
                //        {
                //            // Xóa khỏi Cache để nạp lại từ ES                         
                //            Console.WriteLine(" [" + product_detail_result.id + "] Push Data ES Success");
                //        }
                //        else
                //        {
                //            LogHelper.InsertLogTelegram("CRAWL_SCHEDULER [" + product_code + "] Sync Data Error");
                //        }
                //    }
                //    else
                //    {
                //        LogHelper.InsertLogTelegram("[" + bot_index + ".CRAWL_SCHEDULER] DoSomeRealWork: product_code: " + product_code + ". Xóa trong ES thất bại");
                //    }
                //}
                //else
                //{
                //    IESRepository<object> _ESRepository = new ESRepository<object>(elastic_config);
                //    // Kiểm tra có trong ES ko. Nếu có đồng bộ lại vào ES
                //    var detail = _ESRepository.getProductDetailByCode("product", product_code, (int)LabelType.amazon);
                //    if (detail != null)
                //    {
                //        var product_response = _ESRepository.DeleteProductByCode("product", product_code);
                //        if (product_response) // delete thành công thì mới dc add new
                //        {
                //            var result_push_es = _ESRepository.UpSert(product_detail_result, "product");

                //            if (result_push_es > 0)
                //            {
                //                Console.WriteLine(" [" + product_detail_result.id + "] Sync Data ES Success");
                //            }
                //            else
                //            {                                
                //                LogHelper.InsertLogTelegram(" [" + product_code + "] Sync Data Error");
                //            }
                //        }
                //        else
                //        {
                //            LogHelper.InsertLogTelegram("[" + bot_index + "] DoSomeRealWork: product_code: " + product_code + ". Xóa trong ES thất bại");
                //        }
                //    }
                //}
                IESRepository<object> _ESRepository = new ESRepository<object>(elastic_config);
                var product_response = _ESRepository.DeleteProductByCode("product", product_code);
                long unixTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                product_detail_result.id = unixTime;
                var result_push_es = _ESRepository.UpSert(product_detail_result, "product");
                Console.WriteLine(" [" + product_detail_result.id + "] Sync Data ES: "+ result_push_es);

                #endregion

                sw.Stop();
                LogHelper.InsertLogTelegram("[" + bot_index + "] DoSomeRealWork: product_code: " + product_code + " crawl : " + (sw.ElapsedMilliseconds/1000) + " second");
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("[" + bot_index + "] DoSomeRealWork: product_code: " + product_code + " crawl failed, url=" + url + ", catch = " + ex.ToString());
            }
        }

        public void SyncElasticsearch(string product_manual_key_id, string group_id)
        {
            try
            {
                var config = LoadConfiguration();
                var serviceProvider = LoadServices(config, 0); // group product
                var serviceProviderProductDetail = LoadServices(config, 1); // product detail

                var redis_worker = serviceProvider.GetService<RedisService>();
                var redis_worker_product_detail = serviceProviderProductDetail.GetService<RedisService>();

                IESRepository<object> _ESRepository = new ESRepository<object>(elastic_config);

                // get data product from MONGO API
                var param_push = new Dictionary<string, string>
                {
                    { "id",product_manual_key_id}
                };

                string token = CommonHelper.Encode(JsonConvert.SerializeObject(param_push), "5fDmJ8Ze");
                var data = new RequestData(token, URL_API_USEXPRESS + "/api/AppData/get-product-from-mongo.json");
                var response_api = data.CreateHttpRequest();

                var json_data = JArray.Parse("[" + response_api + "]");
                int status = Convert.ToInt32(json_data[0]["status"].ToString());
                // SYNC ES
                if (status == (int)ResponseType.SUCCESS)
                {
                    string data_product_manual = json_data[0]["data"].ToString();
                    var product_detail = JsonConvert.DeserializeObject<ProductViewModel>(data_product_manual);

                    var detail = _ESRepository.getProductDetailByCode("product", product_detail.product_code, product_detail.label_id);
                    if (detail != null)
                    {
                        var product_response = _ESRepository.DeleteProductByKey("product", product_detail.product_code, product_detail.label_id);
                        if (!product_response.Result)
                        {
                            Console.WriteLine(" [" + product_detail.product_code + "] Delete Data product_code = " + product_detail.product_code + " Mongo To ES  Error");
                        }
                    }

                    // if (product_response) // delete thành công thì mới dc add new
                    //{
                    long unixTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                    product_detail.id = unixTime;

                    var result_push_es = _ESRepository.UpSert(product_detail, "product");
                    if (result_push_es > 0)
                    {
                        Console.WriteLine(" [" + product_detail.product_code + "] Sync Data Mongo To ES Success");
                        // REMOVE CACHE PRODUCT in DB (1)
                        redis_worker_product_detail.removeCache(CacheType.PRODUCT_DETAIL + product_detail.product_code + "_" + product_detail.label_id);

                        // REMOVE CACHE GROUP PRODUCT in DB (0)
                        var gr = group_id.Split(",");
                        for (int i = 0; i <= gr.Length - 1; i++)
                        {
                            redis_worker.removeCache(CacheType.GROUP_PRODUCT_MANUAL + gr[i]); // xóa cache mà chuyên mục chứa các mã sp
                            redis_worker.removeCache(CacheType.GROUP_PRODUCT + gr[i] + "_" + product_detail.label_id); // Xóa cache chứa chi tiết sản phẩm trong 1 chuyên mục               
                        }
                    }
                    else
                    {
                        Console.WriteLine(" [" + product_detail.product_code + "] Sync Data Mongo To ES  Error");
                    }

                    // }

                }
                // SET CACHE



            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("[" + bot_index + "] SyncElasticsearch: product_manual_key_id: " + product_manual_key_id + " group_id =" + group_id + ", catch = " + ex.ToString());
            }
        }


        #region connection Redis
        static IServiceProvider LoadServices(IConfiguration configuration, int db_index)
          => new ServiceCollection()
                  .AddRedis(builder =>
                     builder
                         .SetConfigurationOptions(configuration["ConnectionStrings:Redis"])
                  //.SetKeyPrefix("usexpress_")

                  //.SetDBIndex(Convert.ToInt32(configuration["ConnectionStrings:db_product_index"]))
                  .SetDBIndex(db_index)
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
