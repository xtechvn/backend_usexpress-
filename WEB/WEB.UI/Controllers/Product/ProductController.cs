using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using Crawler;
using Crawler.CrawlCriterias;
using Entities.ViewModels;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.UI.Common;
using WEB.UI.Controllers.LandingPage;
using WEB.UI.Controllers.Product;
using WEB.UI.Controllers.Product.Base;
using WEB.UI.FilterAttribute;
using WEB.UI.Service;
using WEB.UI.ViewModels;

namespace WEB.UI.Controllers
{
    // [EnableCors("MyApi")]
    [Route("[controller]")]
    public class ProductController : BaseProductController
    {
        private readonly IConfiguration Configuration;
        public readonly IProductRepository ProductRepository;
        private readonly RedisConn redisService;

        public ProductController(IProductRepository _productRepository, IConfiguration _Configuration, RedisConn _redisService)
        {
            ProductRepository = _productRepository;
            Configuration = _Configuration;
            redisService = _redisService;

        }

        /// <summary>
        /// Tìm kiếm sản phẩm khi User gõ. Load suggestion. Lấy từ ES
        /// </summary>
        /// <param name="input_search"></param>
        /// <returns></returns>

        [HttpPost("search-suggest.json")]
        public async Task<IActionResult> SearchProduct(string input_search, int search_type = -1)
        {
            string INDEX_ES_PRODUCT = Configuration["DataBaseConfig:Elastic:index_product_search"];
            string ES_HOST = Configuration["DataBaseConfig:Elastic:Host"];
            try
            {
                IESRepository<object> _ESRepository = new ESRepository<object>(ES_HOST);

                var result_product = _ESRepository.searchProduct(input_search, INDEX_ES_PRODUCT, ES_PRODUCT_SEARCH_TOP);

                return Json(new { result_search = (result_product != null && result_product.Count() > 0) ? true : false, products = await this.RenderViewToStringAsync("Components/search/suggestion", result_product) });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "SearchProduct error: " + ex.ToString());
                return Json(new { result_search = false });
            }
        }


        /// <summary>
        /// Tìm kiếm khi User bấm nút SEARCH
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> search(string keywords)
        {
            try
            {
                string token_tele = Configuration["telegram_log_error_fe:Token"];
                string group_id_tele = Configuration["telegram_log_error_fe:GroupId"];
                string KEY_TOKEN_API = Configuration["KEY_TOKEN_API"];
                int time_refresh_price_product = Convert.ToInt32(Configuration["Crawl:time_refresh_price_product"]);
                bool isAjax = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

                if (String.IsNullOrEmpty(keywords))
                {
                    return RedirectToAction("SearchNotFound");
                }


                // Detect By Link 
                if (Utilities.CommonHelper.isCheckLink(keywords))
                {
                    string full_path_crawl_by_queue = Configuration["url_api_usexpress_new"] + "api/product/detail.json";
                    string asin = string.Empty;

                    var detect_label = Utilities.CommonHelper.getLabelTypeByLink(keywords);
                    switch (detect_label)
                    {
                        case (int)LabelType.amazon:
                            #region search by amazon

                            if (Utilities.CommonHelper.CheckAsinByLink(keywords, out asin))
                            {

                                string page_type = TaskQueueName.product_detail_amazon_crawl_queue;
                                int db_index = Convert.ToInt32(Configuration["Redis:Database:db_product_amazon"]);

                                string url_detail = keywords;// (keywords.IndexOf("?") >= 0 ? keywords + "&psc=1" : keywords + "?psc=1");
                                var connect_api_us = new RequestData(full_path_crawl_by_queue, token_tele, group_id_tele, asin, page_type, KEY_TOKEN_API, url_detail, (int)LabelType.amazon);
                                string cache_key = CacheHelper.cacheKeyProductDetail(asin, (int)LabelType.amazon);
                                var product_detail = new ProductViewModel();


                             


                                // check cache có không
                                var j_product_detail = await redisService.GetAsync(cache_key, db_index);
                                if (!string.IsNullOrEmpty(j_product_detail))
                                {
                                    product_detail = JsonConvert.DeserializeObject<ProductViewModel>(j_product_detail);
                                 
                                    return Json(new { status = (int)ResponseType.SUCCESS, url_redirect = product_detail.link_product });
                                }
                                else
                                {
                                    // Kiểm tra có trong ES không
                                    string INDEX_ES_PRODUCT = Configuration["DataBaseConfig:Elastic:index_product_search"];
                                    string ES_HOST = Configuration["DataBaseConfig:Elastic:Host"];
                                    var ESRepository = new ESRepository<object>(ES_HOST);
                                    var result_product = ESRepository.getProductDetailByCode(INDEX_ES_PRODUCT, asin, (int)LabelType.amazon);

                                    product_detail = result_product; // gán data từ es về
                                    if (product_detail != null)
                                    {
                                        // set lại REDIS
                                        redisService.Set(cache_key, JsonConvert.SerializeObject(product_detail), DateTime.Now.AddHours(time_refresh_price_product), db_index);
                                  
                                        return Json(new { status = (int)ResponseType.SUCCESS, url_redirect = product_detail.link_product });
                                    }
                                }

                                #region push queue de crawler
                                var response_crawl = await connect_api_us.CrawlDetailProduct();
                                if (response_crawl) // true la push Queue thanh cong
                                {
                                    string link_product_wating = "/product/" +LabelType.amazon.ToString() + "/waiting/" + asin + ".html";
                                    return Json(new { status = (int)ResponseType.SUCCESS, url_redirect = link_product_wating });

                                    //var product_service = new ProductService(Configuration, redisService);
                                    //product_detail = await product_service.getProductResultJob(cache_key);
                                    //if (product_detail != null && product_detail.page_not_found == false)
                                    //{
                                    //    return Json(new { status = (int)ResponseType.SUCCESS, url_redirect = product_detail.link_product });
                                    //}
                                    //else
                                    //{
                                    //    if (isAjax)
                                    //    {
                                    //        return Ok(new { status = ResponseType.EMPTY, url_redirect = "/", msg = "Điều hướng sang page ko tìm thấy sp" });
                                    //    }
                                    //    else
                                    //    {
                                    //        return Content("Điều hướng sang orderlink");
                                    //    }
                                    //}
                                }
                                else
                                {
                                    LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "Crawl not found" + " response_crawl =" + response_crawl);

                                }
                                #endregion

                            }
                            #endregion
                            break;
                        case (int)LabelType.jomashop:
                            break;
                        default:
                            // Nhan hang nay khong ho tro                          
                            break;
                    }
                }
                else // Search by Keyword
                {
                    #region Search By USEXPRESS
                    var sw = new Stopwatch();
                    sw.Start();
                    string KEY_TOKEN_API_2 = Configuration["KEY_TOKEN_API_2"];
                    string page_type = TaskQueueName.keyword_crawl_queue;
                    int db_index = Convert.ToInt32(Configuration["Redis:Database:db_product_search"]);

                    keywords = CommonHelper.RemoveSpecialCharacters(keywords);

                    string cache_key = CacheHelper.cacheKeySearchByKeyWord(keywords, (int)LabelType.amazon);

                    string full_path_crawl_by_queue = Configuration["url_api_usexpress_new"] + "api/QueueService/data-push.json";

                    var connect_api_us = new RequestData(full_path_crawl_by_queue, token_tele, group_id_tele, string.Empty, page_type, KEY_TOKEN_API_2, string.Empty, (int)LabelType.amazon);
                    var response_crawl = await connect_api_us.CrawlSearchProduct(keywords, cache_key);
                    if (response_crawl) // true la push Queue thanh cong
                    {
                        var product_service = new ProductService(Configuration, redisService);

                        var result = await product_service.getSearchResultJob(cache_key);
                        sw.Stop();
                        if (result != null)
                        {
                            if (isAjax)
                            {
                                return Ok(new { status = ResponseType.SUCCESS, url_redirect = "/product/search/keywords=" + keywords + ".html", msg = "Tim thay" + result.Count() + " sp. Thong time la:" + sw.ElapsedMilliseconds / 1000 + " s" });
                            }
                            else
                            {
                                return Content("Tim thay" + result.Count() + " sp. Thong time la:" + sw.ElapsedMilliseconds / 1000 + " s");
                            }
                        }
                        else
                        {
                            if (isAjax)
                            {
                                return Ok(new { status = ResponseType.EMPTY, url_redirect = "/", msg = "Điều hướng sang page ko tìm thấy sp" });
                            }
                            else
                            {
                                return Content("Rất tiếc chúng tôi không tìm sản phẩm theo từ khóa này. Vui lòng qua trang gốc để lấy link"); // Redirect sang trang Page not found
                            }
                        }
                    }
                    else
                    {
                        sw.Stop();
                        LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "push queue error: " + " response_crawl_queue =" + response_crawl);
                    }

                    #endregion

                }

                return Json(new { result_search = false });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "SearchProduct error: " + ex.ToString() + " keywords =" + keywords);
                return Json(new { result_search = false });
            }
        }

        /// <summary>
        /// label_name:tên nhãn
        ///(check redis -> ES ->Re Crawl- > Redis -> push queue: đồng bộ lên ES))
        /// title: tiêu đề sp
        /// product_id: id sp
        /// </summary>
        /// <param name="product_id"></param>
        /// <returns></returns>        
        [HttpGet("{label_name}/{title}-{product_code}.html")]
        public async Task<ActionResult> getProductDetail(string product_code, string label_name)
        {
            bool isAjax = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            string full_path_crawl_by_queue = Configuration["url_api_usexpress_new"] + "api/product/detail.json"; // push vao queue
            int label_id = LabelNameType.GetLabelId(label_name.Trim());
            string cache_key = CacheHelper.cacheKeyProductDetail(product_code.ToString(), label_id);
            int db_index = Convert.ToInt32(Configuration["Redis:Database:db_product_amazon"]);
            int time_refresh_price_product = Convert.ToInt32(Configuration["Crawl:time_refresh_price_product"]);
            bool is_reCrawl_price = false;
            try
            {
                var product_detail = new ProductViewModel();

                var j_product_detail = await redisService.GetAsync(cache_key, db_index);
                if (!string.IsNullOrEmpty(j_product_detail) && j_product_detail != "null")
                {
                    // Đọc từ Redis 
                    product_detail = JsonConvert.DeserializeObject<ProductViewModel>(j_product_detail);
                }
                else
                {
                    // Kiểm tra có trong ES không
                    string INDEX_ES_PRODUCT = Configuration["DataBaseConfig:Elastic:index_product_search"];
                    string ES_HOST = Configuration["DataBaseConfig:Elastic:Host"];
                    var ESRepository = new ESRepository<object>(ES_HOST);
                    var result_product = ESRepository.getProductDetailByCode(INDEX_ES_PRODUCT, product_code, label_id);

                    product_detail = result_product; // gán data từ es về
                    if (product_detail != null)
                    {
                        // set lại REDIS
                        redisService.Set(cache_key, JsonConvert.SerializeObject(product_detail), DateTime.Now.AddHours(time_refresh_price_product), db_index);
                    }
                }

                if (product_detail != null)
                {
                    if (product_detail.product_type == ProductType.AUTO)
                    {
                        // Kiểm tra ngày crawl gần nhất so với ngày hiện tại vượt quá 2 tiếng sẽ crawl lại giá
                        TimeSpan total_hours = DateTime.Now - product_detail.update_last;
                        if (total_hours.TotalHours > time_refresh_price_product)
                        {
                            is_reCrawl_price = true; //crawl lại giá
                        }
                    }
                    if (product_detail.product_type == ProductType.CRAWL_EXTENSION)
                    {
                        is_reCrawl_price = true; //crawl lại giá
                    }
                }
                else
                {
                    is_reCrawl_price = true; //crawl lại giá + sp detail
                }

                #region Crawl mới lại
                if (is_reCrawl_price)
                {
                    switch (label_name)
                    {
                        case LabelNameType.amazon:
                            string KEY_TOKEN_API = Configuration["KEY_TOKEN_API"];
                            string page_type = TaskQueueName.product_detail_amazon_crawl_queue;
                            string token_tele = Configuration["telegram_log_error_fe:Token"];
                            string group_id_tele = Configuration["telegram_log_error_fe:GroupId"];
                            string url_crawl_detail = "https://www.amazon.com/dp/" + product_code;
                            var connect_api_us = new RequestData(full_path_crawl_by_queue, token_tele, group_id_tele, product_code, page_type, KEY_TOKEN_API, url_crawl_detail, (int)LabelType.amazon);

                            var response_queue = await connect_api_us.CrawlDetailProduct();

                            if (response_queue) // true la push queue thanh cong
                            {
                                if (product_detail == null)
                                {
                                    // Thực hiện get data từ app crawl
                                    // var product_service = new ProductService(Configuration, redisService);
                                    // product_detail = await product_service.getProductResultJob(cache_key);
                                    ViewBag.product_code = product_code;
                                    return View("DetailWaiting");
                                }
                            }
                            else
                            {
                                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "api service queue error" + "- product_code" + product_code);
                            }
                            break;
                    }
                }
                #endregion


                if (!isAjax)
                {
                    if (product_detail != null && product_detail.page_not_found == false)
                    {
                        return View("Detail", product_detail);
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "Crawl not found for Asin =" + product_code);
                        return Redirect("/Error/1");
                    }
                }
                else
                {
                    if (product_detail == null)
                    {
                        LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "Lỗi Crawl khi chọn biến thể từ sản phẩm có asin =" + product_code);
                        return Ok(new { status = ResponseType.EMPTY });
                    }
                    else
                    {
                        return Ok(new { status = product_detail.product_name != string.Empty ? ResponseType.SUCCESS : ResponseType.EMPTY });
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "getProductDetail [asin = " + product_code + "] error: " + ex.ToString());
                if (!isAjax)
                {
                    return Redirect("/Error/1");
                }
                else
                {
                    return Redirect("/Error/1");
                }

            }
        }

        [HttpGet("{label_name}/waiting/{product_code}.html")]
        public async Task<ActionResult> getProductDetailWaiting(string product_code, string label_name)
        {
            ViewBag.product_code = product_code;
            return View("DetailWaiting");
        }

        /// <summary>
        /// [PART 2]
        /// Lấy tiếp thông tin sản phẩm sau khi job crawl xong tiến trình 2
        /// </summary>
        /// <param name="product_code"></param>
        /// <param name="label_name"></param>
        /// <returns></returns>
        [HttpPost("get-detail-product-price.json")]
        //  [ValidateAntiForgeryToken]
        public async Task<IActionResult> getDetailProductPrice(string product_code, int label_type)
        {
            var start_crawl = new Stopwatch();
            start_crawl.Start();
            int status_process = (int)ResponseType.PROCESSING;

            try
            {
                double _price_old = 0;
                double _discount = 0;
                bool response_redis = true;
                string cache_key = CacheHelper.cacheKeyProductDetail(product_code.ToString(), label_type);
                long total_crawl_startime = start_crawl.ElapsedMilliseconds;
                var product_detail = new ProductViewModel();
                int crawl_timeout = Convert.ToInt32(Configuration["timeout_crawl_page_detail_price"]); //ms
                int waiting_crawl = Convert.ToInt32(Configuration["waiting_crawl"]); //Thời gian chờ Job crawl xong. Sau khoảng thời gian này mới bắt đầu vào Redis để lấy dữ lieuj về
                int db_index = Convert.ToInt32(Configuration["Redis:Database:db_product_amazon"]);
                Thread.Sleep(waiting_crawl - 100);//Call tín hiệu trước 100ms khi job bắt đầu thực thi.
                while (response_redis)
                {
                    var j_product_detail = await redisService.GetAsync(cache_key, db_index);
                    if (!string.IsNullOrEmpty(j_product_detail))
                    {
                        product_detail = JsonConvert.DeserializeObject<ProductViewModel>(j_product_detail);
                        if (product_detail.regex_step > 1)
                        {
                            status_process = product_detail.price == 0 ? (int)ResponseType.EMPTY : (int)ResponseType.SUCCESS;
                            start_crawl.Stop();
                            response_redis = false;
                        }
                        else
                        {
                            // Thời gian đọc Cache vượt quá crawl_timeout mà không có data thì return
                            long total_time_current = start_crawl.ElapsedMilliseconds;

                            if (total_time_current - total_crawl_startime >= crawl_timeout)
                            {
                                response_redis = false;
                                status_process = (int)ResponseType.EMPTY;
                                start_crawl.Stop();
                            }
                        }
                    }
                    else
                    {
                        response_redis = false;
                        status_process = (int)ResponseType.EMPTY;
                        start_crawl.Stop();
                    }
                }

                string _render_product_list_fee = string.Empty;
                if (status_process == (int)ResponseType.SUCCESS && product_detail.price > 0)
                {                   
                    _render_product_list_fee = product_detail.list_product_fee == null ? string.Empty : await this.RenderViewToStringAsync("Components/product/fee", product_detail.list_product_fee);
                }

                return Ok(new
                {
                    status = status_process,
                    amount_last = Math.Round(product_detail.amount_vnd).ToString("N0"),
                    price_old = product_detail.price_vnd.ToString("N0"),
                    discount = product_detail.discount,
                    render_product_list_fee = _render_product_list_fee
                });

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "getPriceProduct error: " + ex.ToString() + " product_code =" + product_code);
                return Ok(new
                {
                    status = ResponseType.ERROR,
                    msg = ex.ToString()
                });
            }
        }

        [HttpPost("get-seller.json")]
        public async Task<IActionResult> getSellerList(string product_code, int label_type)
        {
            var start_crawl = new Stopwatch();
            start_crawl.Start();
            int status_process = (int)ResponseType.PROCESSING;
            try
            {
                bool response_redis = true;

                if (string.IsNullOrEmpty(product_code))
                {
                    return Ok(new
                    {
                        status = ResponseType.ERROR
                    });
                }

                string cache_key = CacheHelper.cacheKeyProductDetail(product_code.ToString(), label_type);
                long total_crawl_startime = start_crawl.ElapsedMilliseconds;
                var product_detail = new ProductViewModel();
                int crawl_timeout = Convert.ToInt32(Configuration["timeout_crawl_page_detail_seller"]); //ms thời gian timeout nếu client chờ quá thời gian này sẽ tự hủy
                int waiting_crawl = Convert.ToInt32(Configuration["waiting_crawl"]); //Thời gian chờ Job crawl xong. Sau khoảng thời gian này mới bắt đầu vào Redis để lấy dữ lieuj về
                int db_index = Convert.ToInt32(Configuration["Redis:Database:db_product_amazon"]);

                Thread.Sleep(waiting_crawl - 100);//Call tín hiệu trước 100ms khi job bắt đầu thực thi.
                while (response_redis)
                {
                    var j_product_detail = await redisService.GetAsync(cache_key, db_index);
                    if (!string.IsNullOrEmpty(j_product_detail))
                    {
                        product_detail = JsonConvert.DeserializeObject<ProductViewModel>(j_product_detail);
                        if (product_detail.regex_step > 1)
                        {
                            status_process = product_detail.price == 0 ? (int)ResponseType.EMPTY : (int)ResponseType.SUCCESS;
                            start_crawl.Stop();
                            response_redis = false;
                        }
                        else
                        {
                            // Thời gian đọc Cache vượt quá crawl_timeout mà không có data thì return
                            long total_time_current = start_crawl.ElapsedMilliseconds;

                            if (total_time_current - total_crawl_startime >= crawl_timeout)
                            {
                                response_redis = false;
                                status_process = (int)ResponseType.EMPTY;
                                start_crawl.Stop();
                                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "getSellerList error: ko lay duoc ds seller cho san pham co -- product_code =" + product_code);
                            }
                        }
                    }
                    else
                    {
                        response_redis = false;
                        status_process = (int)ResponseType.EMPTY;
                        start_crawl.Stop();
                    }
                }

                string _render_seller_list = string.Empty;
                if (status_process == (int)ResponseType.SUCCESS)
                {
                    _render_seller_list = product_detail.seller_list == null ? string.Empty : await this.RenderViewToStringAsync("Components/seller/listing", product_detail.seller_list);
                }

                return Ok(new
                {
                    status = status_process,
                    amount_last = Math.Round(product_detail.amount_vnd).ToString("N0"),
                    render_product_list_fee = _render_seller_list
                });

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "getSellerList error: " + ex.ToString() + " product_code =" + product_code);
                return Ok(new
                {
                    status = ResponseType.ERROR
                });
            }
        }

        // Box sản phẩm nổi bật
        [HttpPost("get-product-tab.json")]
        public async Task<IActionResult> getProductTopByFolderId(int folder_id, string partial_view)
        {
            try
            {
                // Lấy ra ds san pham trong chuyên mục đầu tiên
                var product = new ProductGroup(Configuration, redisService);
                var data_feed = await product.getProductListByCacheName(folder_id, 2, 0, 8);

                return Json(new { status = (data_feed != null && data_feed.Count() > 0) ? (int)ResponseType.SUCCESS : (int)ResponseType.EMPTY, products = await this.RenderViewToStringAsync("/Views/Shared/Components/product/blog/" + partial_view + ".cshtml", data_feed) });

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "getProductTopByFolderId error: " + ex.ToString() + " folder_id =" + folder_id);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR
                });
            }
        }

        [HttpGet("search/keywords={keyword}.html")]
        [HttpGet("search/keywords={keyword}/p-{page_index}.html")]
        public async Task<IActionResult> SearchMain(string keyword, int page_index = 1)
        {
            //keyword: Cần phải Endcode - Decode lại 
            bool isAjax = HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            try
            {
                var product_service = new ProductService(Configuration, redisService);
                var product_lst_search = await product_service.mainSearch(keyword);
                if (!isAjax)
                {
                    if (product_lst_search != null)
                    {
                        var model = new SearchEntitiesViewModel
                        {
                            total_item = product_lst_search.Count(),
                            total_item_store = product_lst_search.Count(), // cập nhật sau ở bot crawl
                            page_index = page_index,
                            obj_lst_product_result = product_lst_search,
                            keyword = keyword
                        };
                        return View("Search", model);
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "Crawl not found for keyword =" + keyword);
                        return Content("Rất tiếc. Chúng tôi không tìm thấy sản phẩm nào thuộc từ khóa này");
                    }
                }
                else
                {
                    if (product_lst_search == null)
                    {
                        LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "Lỗi Crawl khi chọn biến thể từ sản phẩm có keyword =" + keyword);
                        return Ok(new { status = ResponseType.EMPTY });
                    }
                    else
                    {
                        return Ok(new { status = product_lst_search != null ? ResponseType.SUCCESS : ResponseType.EMPTY });
                    }
                }


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "Search error: " + ex.ToString());
                if (!isAjax)
                {
                    return Content("Rất tiếc. Chúng tôi không tìm thấy sản phẩm nào thuộc từ khóa này");
                }
                else
                {
                    return Ok(new { status = ResponseType.EMPTY });
                }
            }
        }

        // load lazy product
        [HttpPost("get-product-home")]
        public async Task<IActionResult> loadProductComponent(int _campaign_id, int _skip, int _take, string _component_name, string _box_name)
        {
            try
            {
                return ViewComponent(_component_name, new { campaign_id = _campaign_id, view = "/Views/Shared/Components/product/blog/" + _box_name + ".cshtml", skip = _skip, take = _take, redis_db_index = 2 });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "[FE] box-flashSale error: " + ex.ToString() + " campaign_id =" + _campaign_id);
                return Content("");
            }
        }

        [HttpPost("render-product-history.json")]
        public async Task<IActionResult> renderProductHistory(string j_data)
        {
            try
            {
                var model = JsonConvert.DeserializeObject<List<ProductViewModel>>(j_data);
                return Json(new { status = (int)ResponseType.SUCCESS, data = await this.RenderViewToStringAsync("/Views/Shared/PartialView/Product/ListingProductHistory.cshtml", model) });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "[FE] renderProductHistory error: " + ex.ToString() + " j_data =" + j_data);
                return Content("");
            }
        }

        [HttpPost("render-product-related.json")]
        public async Task<IActionResult> renderProductRelated(string product_code)
        {


            string full_path_crawl_by_queue = Configuration["url_api_usexpress_new"] + "api/product/detail.json"; // push vao queue
            string KEY_TOKEN_API = Configuration["KEY_TOKEN_API"];
            string page_type = TaskQueueName.product_detail_amazon_crawl_queue;
            string token_tele = Configuration["telegram_log_error_fe:Token"];
            string group_id_tele = Configuration["telegram_log_error_fe:GroupId"];

            try
            {

                string cache_key = CacheHelper.cacheKeyProductDetail(product_code, (int)LabelType.amazon);
                var j_product_detail = await redisService.GetAsync(cache_key, Convert.ToInt32(Configuration["Redis:Database:db_product_amazon"]));
                if (j_product_detail != null)
                {
                    var model = JsonConvert.DeserializeObject<ProductViewModel>(j_product_detail);
                    var product_list = model.product_related;
                    if (product_list != null)
                    {
                        product_list = product_list.OrderByDescending(x => x.rate).Skip(0).Take(5).ToList();

                        #region crawl product related

                        foreach (var item in product_list)
                        {
                            string cache_key_related = CacheHelper.cacheKeyProductDetail(item.product_code, (int)LabelType.amazon);
                            var j_product_detail_related = await redisService.GetAsync(cache_key_related, Convert.ToInt32(Configuration["Redis:Database:db_product_amazon"]));
                            if (j_product_detail_related == null)
                            {
                                string url_crawl_detail = "https://www.amazon.com/dp/" + item.product_code;
                                var connect_api_us = new RequestData(full_path_crawl_by_queue, token_tele, group_id_tele, item.product_code, page_type, KEY_TOKEN_API, url_crawl_detail, (int)LabelType.amazon);
                                var response_queue = await connect_api_us.CrawlDetailProduct();
                            }
                        }
                        #endregion

                        return Json(new { status = (int)ResponseType.SUCCESS, list_product_code = string.Join(",", product_list.Select(x => x.product_code)), data = await this.RenderViewToStringAsync("/Views/Shared/PartialView/Product/ProductRelated.cshtml", product_list) });
                    }
                    else
                    {
                        return Json(new { status = (int)ResponseType.EMPTY });
                    }
                }
                else
                {
                    return Json(new { status = (int)ResponseType.EMPTY, msg = "cache_empty" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "[FE] renderProductHistory error: " + ex.ToString() + " product_code =" + product_code);
                return Json(new { status = (int)ResponseType.ERROR });
            }
        }

        [HttpPost("render-product-price.json")]
        public async Task<IActionResult> renderProductPrice(string product_code, int label_id)
        {
            try
            {
                string cache_key = CacheHelper.cacheKeyProductDetail(product_code, label_id);
                var j_product_detail = await redisService.GetAsync(cache_key, Convert.ToInt32(Configuration["Redis:Database:db_product_amazon"]));
                if (j_product_detail != null)
                {
                    var model = JsonConvert.DeserializeObject<ProductViewModel>(j_product_detail);

                    return Json(new { status = (int)ResponseType.SUCCESS, amount_vnd = Math.Round(model.amount_vnd).ToString("N0"), link_product = model.link_product, amount_vnd_raw = model.amount_vnd });
                }
                else
                {
                    return Json(new { status = (int)ResponseType.EMPTY, msg = "cache_empty" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "[FE] renderProductPrice error: " + ex.ToString() + " product_code =" + product_code);
                return Json(new { status = (int)ResponseType.ERROR });
            }
        }

    }
}