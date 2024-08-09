using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using Crawler.ScraperLib.Amazon;
using Entities.ViewModels;
using Entities.ViewModels.Product;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using StackExchange.Redis.Extensions.Core.Abstractions;
using Telegram.Bot.Requests;
using Utilities;
using Utilities.Contants;
using WEB.API.Common;
using WEB.API.Model.Carts;
using WEB.API.Model.Product;
using WEB.API.Service.Queue;


namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : BaseController
    {
        private readonly IConfiguration _Configuration;
        private readonly IClientRepository _ClientRepository;
        private readonly IProductRepository _ProductRepository;
        private readonly RedisConn _redisService;

        public ProductController(IClientRepository clientRepository, IProductRepository productRepository,
            IConfiguration Configuration, RedisConn redisService)
        {
            _ProductRepository = productRepository;
            _Configuration = Configuration;
            _redisService = redisService;
            _ClientRepository = clientRepository;
        }

        [HttpPost("detail.json")]
        public async Task<ActionResult> getProductDetail(string token)
        {
            string msg = string.Empty;

            try
            {
                JArray objParr = null;
                bool response_queue = false;

                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    string product_code = objParr[0]["product_code"].ToString();
                    string page_type = objParr[0]["page_type"].ToString();
                    string url = objParr[0]["url"].ToString();
                    int label_id = Convert.ToInt32(objParr[0]["label_id"].ToString());
                    int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);

                    #region Setting Cache Redis
                    // string cache_name = CacheHelper.cacheKeyProductDetail(product_code, label_id);
                    //var product_detail = await _redisService.Get(cache_name, db_index);

                    //if (!string.IsNullOrEmpty(product_detail))
                    //{
                    //    return Ok(new { status = ResponseType.SUCCESS.ToString(), product_data = product_detail, msg = ResponseType.SUCCESS.ToString() });
                    //}
                    #endregion

                    #region Execute Push Queue
                    var work_queue = new WorkQueueClient();
                    var queue_setting = new QueueSettingViewModel
                    {
                        host = _Configuration["Queue:Host"],
                        v_host = _Configuration["Queue:V_Host"],
                        port = Convert.ToInt32(_Configuration["Queue:Port"]),
                        username = _Configuration["Queue:Username"],
                        password = _Configuration["Queue:Password"]
                    };
                    #endregion

                    var j_queue_param = new Dictionary<string, string>
                    {
                        {"product_code",product_code},
                        {"page_type", page_type},
                        {"url",url},
                        {"label_id", label_id.ToString()}
                    };
                    var data_product = JsonConvert.SerializeObject(j_queue_param);

                    response_queue = work_queue.InsertQueueSimple(queue_setting, data_product, TaskQueueName.product_crawl_queue);
                    if (response_queue)
                    {
                        return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = "response_queue = " + response_queue });
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("getProductDetail==> error with token: " + token + "-- product_code = " + product_code);
                        msg = "push queue error !!!";
                    }
                }

                var result_error = new Dictionary<string, string>
                        {
                            {"status",ResponseType.FAILED.ToString()},
                            {"msg", msg},
                            {"token",token}
                        };

                return Content(JsonConvert.SerializeObject(result_error));

            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("getProductDetail==> error:  " + ex.Message);
                var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.ERROR.ToString()},
                            {"msg", ex.ToString()},
                            {"token",token}
                        };

                return Content(JsonConvert.SerializeObject(result));
            }
        }

        /// <summary>
        /// Mở rộng ra cho bên ngoài dùng.
        /// Quy ước Key cho từng user
        /// </summary>
        /// <param name="product_code"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        [HttpPost("detail-public.json")]
        public async Task<ActionResult> PublicDetailAmz(string token)
        {
            int crawl_timeout = Convert.ToInt32(_Configuration["crawl_timeout_ms"]); //ms
            var st = new Stopwatch();
            st.Start();
            try
            {
                JArray objParr = null;
                bool response_queue = false;

                #region TEST
                string page_type = TaskQueueName.product_detail_amazon_crawl_queue; // crawl o page nao                
                int label_id = (int)LabelType.amazon;// tao cache
                string product_code = string.Empty;
                string url = string.Empty;
                string shop_id = string.Empty;

                product_code = "B003CEWPHC";
                url = "https://www.amazon.com/dp/B003CEWPHC";
                shop_id = "app_crawl_page";

                string j_param = "{'product_code':'" + product_code + "','url':'" + url + "','shop_id':'" + shop_id + "'}";
                //token = CommonHelper.Encode(j_param, _Configuration["KEY_TOKEN_API"]);

                #endregion

                if (!CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    return Content("Sai key");
                }
                else
                {
                    product_code = (objParr[0]["product_code"]).ToString();
                    url = (objParr[0]["url"]).ToString();
                    shop_id = (objParr[0]["shop_id"]).ToString();
                    if (product_code.Length <= 5 || url.IndexOf("amazon") == -1)
                    {
                        return Content("Tham so khong hop le");
                    }
                    else
                    {
                        switch (shop_id)
                        {
                            case "usexpress_old":
                            case "app_crawl_page":
                                break;
                            default:
                                return Content("Lien he admin de duoc cap api");

                        }
                    }
                }

                string cache_name = CacheHelper.cacheKeyProductDetail(product_code, label_id);
                var product_detail = await _redisService.GetAsync(cache_name, Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]));

                #region Execute Push Queue
                var work_queue = new WorkQueueClient();
                var queue_setting = new QueueSettingViewModel
                {
                    host = _Configuration["Queue:Host"],
                    v_host = _Configuration["Queue:V_Host"],
                    port = Convert.ToInt32(_Configuration["Queue:Port"]),
                    username = _Configuration["Queue:Username"],
                    password = _Configuration["Queue:Password"]
                };
                #endregion

                var j_queue_param = new Dictionary<string, string>
                    {
                        {"product_code",product_code},
                        {"page_type", page_type},
                        {"url",url},
                        {"label_id", label_id.ToString()}
                    };
                var data_product = JsonConvert.SerializeObject(j_queue_param);

                // co trong redis roi return luon                
                if (product_detail != null)
                {
                    var product_detail_revert = JsonConvert.DeserializeObject<ProductViewModel>(product_detail);
                    return Content(JsonConvert.SerializeObject(product_detail_revert));
                }

                // ko co redis push queue
                response_queue = work_queue.InsertQueueSimple(queue_setting, data_product, TaskQueueName.product_crawl_queue);
                if (response_queue)
                {
                    bool response_crawl = true;
                    var start_crawl = new Stopwatch();
                    start_crawl.Start();
                    long total_crawl_startime = start_crawl.ElapsedMilliseconds;
                    while (response_crawl)
                    {
                        int regex_step = 1;
                        product_detail = await _redisService.GetAsync(cache_name, Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"])); // Push queue để các Consummer xử lý đi Crawler
                        ProductViewModel product_detail_revert = new ProductViewModel();
                        if (product_detail != null)
                        {
                            product_detail_revert = JsonConvert.DeserializeObject<ProductViewModel>(product_detail);
                            regex_step = product_detail_revert.regex_step;
                        }
                        if (!string.IsNullOrEmpty(product_detail) && regex_step > 1)
                        {
                            // return luon neu < hon thoi gian timeout                            
                            return Content(JsonConvert.SerializeObject(product_detail_revert));
                        }
                        else
                        {
                            // Thời gian đọc Cache vượt quá crawl_timeout mà không có data thì return
                            long total_time_current = start_crawl.ElapsedMilliseconds;

                            if (total_time_current - total_crawl_startime > crawl_timeout)
                            {
                                response_crawl = false;
                            }
                        }
                    }

                    var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.EMPTY.ToString()},
                            {"msg", "Crawl execute FAILED !!! Please check app crawl for asin =" + product_code},
                            {"token",""}
                        };

                    return Content(JsonConvert.SerializeObject(result));
                }

                return Content(string.Empty);
            }
            catch (Exception ex)
            {
                st.Stop();
                LogHelper.InsertLogTelegram("getProductDetail testProductDetail==> error:  " + ex.Message);
                var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.ERROR.ToString()},
                            {"msg_response_error_api", ex.ToString()},
                            {"token",""}
                        };
                return Content(JsonConvert.SerializeObject(result));
            }
        }

        [HttpPost("add-product-favorite.json")]
        public async Task<IActionResult> addToFavorite(string token)
        {
            try
            {
                var productFavoriteViewModel = new ProductFavoriteViewModel()
                {
                    ClientId = 590,
                    LabelId = 1,
                    ProductCode = "B074JF7M9X",
                    IsFavorite = false,
                };
                //string j_param = "{'product_favorite':'" + Newtonsoft.Json.JsonConvert.SerializeObject(productFavoriteViewModel) + "'}";
                //token = CommonHelper.Encode(j_param, _Configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    var productFavoriteModel = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductFavoriteViewModel>(objParr[0]["product_favorite"].ToString());
                    var client = await _ClientRepository.getClientByClientMapId((int)productFavoriteViewModel.ClientId);
                    productFavoriteModel.ClientId = client != null ? client.ClientId : 0;
                    var productFavorite = new ProductFavorite(_Configuration);
                    var product_result = await productFavorite.addNew(productFavoriteModel);
                    if (product_result != null)
                    {
                        return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = product_result });
                    }
                    else
                    {
                        return Ok(new { status = ResponseType.FAILED.ToString(), msg = "Add to favorite fail. API/ProductController" });
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("add-product-favorite.json - API/ProductController: token valid !!! token =" + token);
                    return Ok(new { status = ResponseType.EXISTS.ToString(), _token = token, msg = "token valid !!!" });
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("add-product-favorite.json - API/ProductController " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = ex.ToString() });
            }
        }

        [HttpPost("add-product-not-found.json")]
        public async Task<IActionResult> addProductNotFound(string token)
        {
            try
            {
                var productNotFoundViewModel = new ProductNotFoundViewModel()
                {
                    LabelId = 1,
                    ProductCode = "B074JF798M",
                    CreateTime = DateTime.Now,
                    ExceptionMsg = "Sản phẩm không có thông tin",
                    Status = (int)Constants.Product_Not_Found_Status.Product_Exists,
                    Ip = "123.456.1.89",
                };
                string j_param = "{'product_not_found':'" + Newtonsoft.Json.JsonConvert.SerializeObject(productNotFoundViewModel) + "'}";
                //token = CommonHelper.Encode(j_param, _Configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    var productNotFoundModel = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductNotFoundViewModel>(objParr[0]["product_not_found"].ToString());
                    var productNotFound = new ProductNotFound(_Configuration);
                    var product_result = await productNotFound.addNew(productNotFoundModel);
                    if (product_result != null)
                    {
                        return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = product_result });
                    }
                    else
                    {
                        return Ok(new { status = ResponseType.FAILED.ToString(), msg = "Add to favorite fail. API/addProductNotFound" });
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("add-product-favorite.json - API/ProductController: token valid !!! token =" + token);
                    return Ok(new { status = ResponseType.EXISTS.ToString(), _token = token, msg = "token valid !!!" });
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("add-product-favorite.json - API/ProductController " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = ex.ToString() });
            }
        }

        [EnableCors("MyApi")]
        [HttpPost("get-shipping-fee/by-page-source.json")]
        public async Task<IActionResult> getShippingFeeByPageSource(string token)
        {
            try
            {
                JArray objParr = null;
                string product_code = string.Empty;
                bool is_crawl_weight = false;

                #region TEST
                //var j_param = new Dictionary<string, string>
                //{
                //    {"label_id", "1"},
                //    {"page_source","r-listing/B07G3QFG8N/ref=dp_olp_NEW?thresholdShippingMessage=true_mbc?ie=UTF8&amp;condition=NEW%3FthresholdShippingMessage%3Dtrue\"><span>New (5) from</span><span> </span><span class=\"a-size-base a-color-price\">$7.49</span>" },
                //    {"link","https://www.amazon.com/Stella-ChewyS-Freeze-Dried-Hearts-Treats/dp/B07G3QFG8N?ref_=Oct_DLandingS_D_963f4402_61&smid=ATVPDKIKX0DER"},
                //};
                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, _Configuration["KEY_TOKEN_API"]);
                #endregion


                if (!(CommonHelper.GetParamWithKey(token, out objParr, "kq1jnJAJShgdRPYjiMyi")))
                {
                    return Ok(new { status = ResponseType.FAILED.ToString(), msg = "token connect Faild" });
                }

                int label_id = Convert.ToInt32(objParr[0]["label_id"]);
                string page_source = objParr[0]["page_source"].ToString();
                string link_product_source = objParr[0]["link"].ToString();

                if (Utilities.CommonHelper.CheckAsinByLink(link_product_source, out product_code))
                {
                    string cache_name = CacheHelper.cacheKeyProductDetail(product_code, label_id);
                    int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);
                    double round_weight = 1;
                    string unit = "pounds";

                    //Tỷ giá trong ngày
                    var lib = new Service.Lib.Common(_Configuration, _redisService);
                    double rate = Convert.ToDouble(lib.crawlRateVCB());
                    var amz_detail = ParserAmz.RegexElementPage(page_source, product_code, link_product_source, rate);

                    if (amz_detail != null)
                    {
                        //1. Lấy ra giá
                        bool is_range_price = false;
                        if (amz_detail.price <= 0)
                        {
                            amz_detail.price = ParserAmz.getPriceForExtension(page_source, out is_range_price);
                        }

                        //2. Lấy cân nặng
                        string item_weight = ParserAmz.getItemWeight(page_source, product_code, out is_crawl_weight);

                        //3. Tách cân cặng                        
                        if (is_crawl_weight)
                        {
                            string[] weight_value = item_weight.Split(" ");
                            round_weight = Convert.ToDouble(weight_value[0]);
                            unit = weight_value[1].Trim();
                        }
                        //4. Tính phí mua hộ
                        var product_buyer = new ProductBuyerViewModel
                        {
                            LabelId = label_id,
                            Price = amz_detail.price,
                            Pound = Utilities.CommonHelper.convertToPound(round_weight, unit),
                            IndustrySpecialType = amz_detail.industry_special_type
                        };

                        var _shipping_fee = await _ProductRepository.getShippingFee(label_id, product_buyer);
                        if (!_shipping_fee.ContainsKey("PRICE_LAST") || _shipping_fee["PRICE_LAST"] <= 0)
                        {
                            _shipping_fee.Add("PRICE_LAST", _shipping_fee["TOTAL_SHIPPING_FEE"] + amz_detail.price + amz_detail.shiping_fee);
                        }
                        if (!_shipping_fee.ContainsKey("TOTAL_SHIPPING_FEE") || _shipping_fee["TOTAL_SHIPPING_FEE"]<=0)
                        {
                            _shipping_fee["TOTAL_SHIPPING_FEE"] = _shipping_fee["FIRST_POUND_FEE"] + _shipping_fee["NEXT_POUND_FEE"] + _shipping_fee["LUXURY_FEE"];
                        }

                        //5. Gán phí mua hộ
                        var product_fee = new ProductFeeViewModel
                        {
                            label_name = LabelType.amazon.ToString(),
                            price = amz_detail.price,
                            amount_vnd = amz_detail.price == 0 ? 0 : Convert.ToDouble(_shipping_fee[FeeBuyType.PRICE_LAST.ToString()]) * rate,
                            list_product_fee = _shipping_fee,
                            shiping_fee =amz_detail.shiping_fee,
                            total_fee = _shipping_fee["TOTAL_SHIPPING_FEE"]
                        };
                        amz_detail.list_product_fee = product_fee;
                        amz_detail.product_name = ParserAmz.GetProductName(page_source).Replace("\"", "").Replace("'", "");

                        // tinh ra gia ve tay
                        amz_detail.amount_vnd = amz_detail.list_product_fee != null ? amz_detail.list_product_fee.amount_vnd : 0;

                        //6. Thực hiện lưu cache để phục vụ cho trang Detail khi khách đặt mua hộ bên Addon
                        // Set is crawl = false. Để tiến hành crawl lại giá. Tránh trường hợp khách change element html trên mặt trang
                        amz_detail.is_crawl_weight = is_crawl_weight; // Có cân nặng hay không
                        amz_detail.regex_step = 2;//mã định đã full page
                        amz_detail.is_redirect_extension = true; // Nếu được gọi từ extension thì sẽ chỉ cần crawl lại giá. Ko cần crawl tiếp cân nặng
                        amz_detail.link_product = CommonHelper.genLinkDetailProduct(LabelType.amazon.ToString(), product_code, amz_detail.product_name);
                        //Set product là crawl từ ext về:
                        amz_detail.product_type= ProductType.CRAWL_EXTENSION;
                        //Set cache:
                        _redisService.Set(cache_name, JsonConvert.SerializeObject(amz_detail),DateTime.Now.AddMinutes(30), db_index);

                        string url_usexpress_home = _Configuration["enpoint_us:url_usexpress_home"];
                        string url_usexpress_detail = url_usexpress_home + CommonHelper.genLinkDetailProduct(LabelType.amazon.ToString(), product_code, CommonHelper.RemoveSpecialCharacters(amz_detail.product_name)) + "?product_source=3";
                         //-- Trả extension:
                         var extension_output = new
                        {
                            product_code=product_code,
                            product_price = amz_detail.price,
                            first_pound_fee = _shipping_fee["FIRST_POUND_FEE"],
                            next_pound_fee = _shipping_fee["NEXT_POUND_FEE"],
                            luxury_fee = _shipping_fee["LUXURY_FEE"],
                            discount_first_fee = 0,
                            total_shipping_fee = _shipping_fee["TOTAL_SHIPPING_FEE"],
                            price_last = _shipping_fee["PRICE_LAST"],
                            price_last_vnd = Math.Round(amz_detail.amount_vnd,0),
                            url = url_usexpress_detail,
                        };
                        return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = "Crawl-Success: ", data= extension_output });
                    }
                    else
                    {
                        Utilities.LogHelper.InsertLogTelegram("[API] get-shipping-fee/by-pagesource.json - API/ProductController: get detail error with asin =" + product_code + ", token =" + token);
                        return Ok(new { status = ResponseType.EMPTY.ToString(), msg = "Không lấy được sản phẩm.", asin = product_code }); ;
                    }
                }
                else
                {
                    return Ok(new { status = ResponseType.ERROR.ToString(), msg = "Không lấy được ProductCode từ link" });
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("[API] get-shipping-fee/by-pagesource.json - API/ProductController " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = ex.ToString() });
            }
        }

        [HttpPost("get-total-product-crawl-today.json")]
        public async Task<ActionResult> getTotalProductCrawlToday(string token)
        {
            string _msg = string.Empty;
            try
            {
                //Test
                //string j_param = "{'label_type':'-1'}";
                //token = CommonHelper.Encode(j_param, _Configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    int label_type = Convert.ToInt32(objParr[0]["label_type"]);
                    string ES_HOST = _Configuration["DataBaseConfig:Elastic:Host"];
                    var ESRepository = new ESRepository<object>(ES_HOST);
                    var total = ESRepository.getTotalProductCrawlToday(_Configuration["DataBaseConfig:Elastic:index_product_search"], label_type);

                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data = total
                    });
                }
                else
                {
                    _msg = "Token khong hop le: token = " + token;
                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = _msg
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(" api: get-total-product-crawl-today.json: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "[getTotalProductCrawlToday] = " + ex.ToString()
                });
            }
        }
        [HttpPost("public-detail-joma.json")]
        public async Task<ActionResult> PublicDetailJoma(string token)
        {
            string _msg = string.Empty;
            bool response_queue = false;
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    string url = objParr[0]["url"].ToString();
                    if (!url.Contains("https://www.jomashop.com/"))
                    {
                        _msg = "Token khong hop le: token = " + token;
                    }
                    else
                    {
                        // Execute Push Queue
                        var work_queue = new WorkQueueClient();
                        var queue_setting = new QueueSettingViewModel
                        {
                            host = _Configuration["Queue:Host"],
                            v_host = _Configuration["Queue:V_Host"],
                            port = Convert.ToInt32(_Configuration["Queue:Port"]),
                            username = _Configuration["Queue:Username"],
                            password = _Configuration["Queue:Password"]
                        };
                        var item = new
                        {
                            label_id = 7,
                            url = url,
                            url_type = 0
                        };
                        response_queue = work_queue.InsertQueueSimple(queue_setting, JsonConvert.SerializeObject(item), "data_feed");
                        if (response_queue)
                        {
                            var cache_name = CacheHelper.getProductDetailCacheKeyFromURL(url, (int)LabelType.jomashop);
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                cache_name = cache_name
                            });
                        }
                        else
                        {
                            LogHelper.InsertLogTelegram(" api: public-detail-joma.json, push queue failed: " + token);
                            return Ok(new
                            {
                                status = (int)ResponseType.FAILED,
                                cache_name = ""
                            });
                        }
                    }

                }
                else
                {
                    _msg = "Token khong hop le: token = " + token;
                }
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = _msg
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(" api: public-detail-joma.json: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "[PublicDetailJoma] = " + ex.ToString()
                });
            }
        }
        [HttpPost("get-fe-interested-product.json")]
        public async Task<IActionResult> GetInterestedProductFE(string token)
        {
            string _msg = "";
            int _status = (int)ResponseType.FAILED;
            dynamic data = null;
            string excute_time = "";
            long total_record = -1;
            try
            {
                List<ProductInterestedFEViewModel> data_feed;
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    int page_index = Convert.ToInt32(objParr[0]["page_index"]);
                    int page_size = 10; // Convert.ToInt32(objParr[0]["page_size"]);
                    var excute_db_time = new Stopwatch();
                    var data_table = _ProductRepository.GetInterestedProduct(page_index, page_size);
                    excute_time = excute_db_time.ElapsedMilliseconds + " ms";
                    string cache_name = ReadFile.LoadConfig().Interested_Product_Cache_Name;
                    int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_common"]);
                    string interested_json = _redisService.Get(cache_name, db_index);
                    if (interested_json != "" && interested_json != null)
                    {
                        data_feed = JsonConvert.DeserializeObject<List<ProductInterestedFEViewModel>>(interested_json);
                        if (data_feed != null && data_feed.Count > 0)
                        {
                            excute_db_time.Restart();
                            var data_table_2 = _ProductRepository.GetInterestedProductTotalRecord();
                            excute_db_time.Stop();
                            excute_time = excute_db_time.ElapsedMilliseconds + " ms";
                            total_record = Convert.ToInt32(data_table_2.Rows[0][0]);
                            _status = (int)ResponseType.SUCCESS;
                            _msg = "Success";
                            data = data_feed;
                            return Ok(new
                            {
                                status = _status,
                                msg = _msg,
                                data = data,
                                excute_time = excute_time,
                                total_record = total_record
                            });
                        }
                    }
                    data_feed = new List<ProductInterestedFEViewModel>();
                    var convertedList = (from row in data_table.AsEnumerable()
                                         select new ProductFEInterested()
                                         {
                                             ID = Convert.ToInt64(row["ID"]),
                                             ProductCode = Convert.ToString(row["ProductCode"]),
                                             LabelId = Convert.ToInt32(row["LabelId"])
                                         }).ToList();
                    IESRepository<object> _ESRepository = new ESRepository<object>(_Configuration["DataBaseConfig:Elastic:Host"]);
                    foreach (var p in convertedList)
                    {
                        var model = _ESRepository.getProductDetailByCode("product", p.ProductCode, p.LabelId);
                        if (model != null && model.page_not_found == false && model.product_code != "" && model.product_code != null)
                        {
                            data_feed.Add(new ProductInterestedFEViewModel()
                            {
                                amount_vnd = string.Format("{0:#,##0.00}", model.amount_vnd),
                                discount = model.discount,
                                image_thumb = model.image_thumb,
                                is_prime_eligible = model.is_prime_eligible,
                                label_name = model.label_name,
                                link_product = model.link_product,
                                product_bought_quantity = model.product_bought_quantity,
                                product_name = model.product_name,
                                seller_name = model.seller_name,
                                star = model.star,
                                brand_label_url = "#"
                            });
                        }
                    }
                    if (data_feed.Count > 0)
                    {
                        _redisService.Set(cache_name, JsonConvert.SerializeObject(data_feed), db_index);
                        excute_db_time.Restart();
                        var data_table_2 = _ProductRepository.GetInterestedProductTotalRecord();
                        excute_db_time.Stop();
                        excute_time = excute_db_time.ElapsedMilliseconds + " ms";
                        total_record = Convert.ToInt32(data_table_2.Rows[0][0]);
                        _status = (int)ResponseType.SUCCESS;
                        _msg = "Success";
                        data = data_feed;
                    }
                    else
                    {
                        _status = (int)ResponseType.FAILED;
                        _msg = "Empty Data";
                        data = null;
                        excute_time = excute_db_time.ElapsedMilliseconds + " ms";
                    }
                }
                else
                {
                    _msg = "Token khong hop le: token = " + token;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(" api: get-fe-interested-product.json: " + ex);
                _status = (int)ResponseType.ERROR;
                _msg = "Error On Excution";
                data = null;
            }
            return Ok(new
            {
                status = _status,
                msg = _msg,
                data = data,
                excute_time = excute_time,
                total_record = total_record
            });
        }
    }
}