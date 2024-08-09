using Caching.RedisWorker;
using Entities.ViewModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.UI.Common;
using WEB.UI.ViewModels;

namespace WEB.UI.Controllers.Product
{
    public partial class ProductService
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public ProductService(IConfiguration _configuration, RedisConn _redisService)
        {
            configuration = _configuration;
            redisService = _redisService;
        }

        /// <summary>
        /// Đọc dữ liệu sản phẩm khi Job crawl xong
        /// </summary>
        /// <param name="cache_key">Key cache sản phẩm sau khi Job crawl xong set cache</param>
        /// <returns></returns>
        public async Task<ProductViewModel> getProductResultJob(string cache_key)
        {
            var start_crawl = new Stopwatch();
            start_crawl.Start();
            try
            {
                var product_detail = new ProductViewModel();
                bool response_queue = true;
                long total_crawl_startime = start_crawl.ElapsedMilliseconds;
                int crawl_timeout = Convert.ToInt32(configuration["timeout_crawl_page_detail"]); //ms
                int db_index = Convert.ToInt32(configuration["Redis:Database:db_product_amazon"]);
                while (response_queue)
                {
                    string j_product_detail = await redisService.GetAsync(cache_key, db_index);
                    if (!string.IsNullOrEmpty(j_product_detail))
                    {
                        product_detail = JsonConvert.DeserializeObject<ProductViewModel>(j_product_detail);
                        start_crawl.Stop();
                        return product_detail;
                    }
                    else
                    {
                        // Thời gian đọc Cache vượt quá crawl_timeout mà không có data thì return
                        long total_time_current = start_crawl.ElapsedMilliseconds;

                        if (total_time_current - total_crawl_startime > crawl_timeout)
                        {

                            response_queue = false;
                            start_crawl.Stop();
                        }
                    }
                }

                if (product_detail == null)
                {
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[product_detail = null]Crawl execute time out !!! Please check app crawl for cache_key =" + cache_key);
                }

                return null;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getListingOrder " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Đọc dữ liệu sản phẩm khi Job crawl xong
        /// </summary>
        /// <param name="cache_key">Key cache sản phẩm sau khi Job crawl xong set cache</param>
        /// <returns></returns>
        public async Task<List<ProductListViewModel>> getSearchResultJob(string cache_key)
        {
            var start_crawl = new Stopwatch();
            start_crawl.Start();
            try
            {
                var product_detail = new List<ProductListViewModel>();
                bool response_queue = true;
                long total_crawl_startime = start_crawl.ElapsedMilliseconds;
                int crawl_timeout = Convert.ToInt32(configuration["timeout_crawl_page_search"]); //ms
                int db_index = Convert.ToInt32(configuration["Redis:Database:db_product_search"]);
                while (response_queue)
                {
                    string j_product_detail = await redisService.GetAsync(cache_key, db_index);
                    if (!string.IsNullOrEmpty(j_product_detail))
                    {
                        product_detail = JsonConvert.DeserializeObject<List<ProductListViewModel>>(j_product_detail);
                        start_crawl.Stop();
                        return product_detail;
                    }
                    else
                    {
                        // Thời gian đọc Cache vượt quá crawl_timeout mà không có data thì return
                        long total_time_current = start_crawl.ElapsedMilliseconds;

                        if (total_time_current - total_crawl_startime > crawl_timeout)
                        {
                            response_queue = false;
                            start_crawl.Stop();
                        }
                    }
                }

                if (product_detail == null)
                {
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[product_detail = null]Crawl execute time out !!! Please check app crawl for cache_key =" + cache_key);
                }

                return null;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getSearchResultJob[cache_key = " + cache_key + "] " + ex.Message);
                return null;
            }
        }

        public async Task<List<ProductListViewModel>> mainSearch(string keyword)
        {
            string token_tele = configuration["telegram_log_error_fe:Token"];
            string group_id_tele = configuration["telegram_log_error_fe:GroupId"];
            try
            {
                string full_path_crawl_by_queue = configuration["url_api_usexpress_new"] + "api/QueueService/data-push.json";
                string cache_key = CacheHelper.cacheKeySearchByKeyWord(keyword, (int)LabelType.amazon); // mac dinh search uu tien amz
                int db_index = Convert.ToInt32(configuration["Redis:Database:db_product_search"]);
                string KEY_TOKEN_API_2 = configuration["KEY_TOKEN_API_2"];
                string page_type = TaskQueueName.keyword_crawl_queue;


                var product_lst_search = new List<ProductListViewModel>();

                var j_product_detail = await redisService.GetAsync(cache_key, db_index);
                if (!string.IsNullOrEmpty(j_product_detail))
                {
                    product_lst_search = JsonConvert.DeserializeObject<List<ProductListViewModel>>(j_product_detail);
                }
                else
                {
                    //push queue        
                    var connect_api_us = new RequestData(full_path_crawl_by_queue, token_tele, group_id_tele, string.Empty, page_type, KEY_TOKEN_API_2, string.Empty, (int)LabelType.amazon);
                    var response_queue = await connect_api_us.CrawlSearchProduct(keyword, cache_key);

                    if (response_queue) // true la push queue thanh cong
                    {
                        var product_service = new ProductService(configuration, redisService);

                        product_lst_search = await product_service.getSearchResultJob(cache_key);
                    }
                    else
                    {
                        return null;
                    }
                }
                return product_lst_search;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(token_tele, group_id_tele, "fe-mainSearch push queue error" + "- ex" + ex.ToString());
                return null;
            }
        }



    }
}
