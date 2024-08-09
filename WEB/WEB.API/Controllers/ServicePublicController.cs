using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.ServicePublic;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.API.Common;
using WEB.API.Service.Log;
using WEB.API.Service.Queue;
using WEB.API.Service.Survery;
using static Utilities.Contants.Constants;

namespace WEB.API.Controllers
{

    /// <summary>
    /// Controller này sẽ public ra các api cho bên thứ 3 sử dụng
    /// Yêu cầu tất cả phải CACHE lại
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ServicePublicController : BaseController
    {
        private readonly IConfiguration _Configuration;
        private readonly IProductRepository _ProductRepository;
        private readonly ICommonRepository _CommonRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly RedisConn _RedisService;
        private readonly IOrderItemRepository _orderItemRepository;

        public ServicePublicController(IProductRepository productRepository, IConfiguration Configuration,
             ICommonRepository CommonRepository,
            IOrderRepository orderRepository, RedisConn redisService, IOrderItemRepository orderItemRepository)
        {
            _ProductRepository = productRepository;
            _Configuration = Configuration;
            _CommonRepository = CommonRepository;
            _RedisService = redisService;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
        }

        /// <summary>
        /// Báo giá thủ công
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        // [ResponseCache(Duration = 300, VaryByQueryKeys = new string[] { "token" })]
        //  [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client)]
        [EnableCors("MyApi")]
        [HttpPost("tracking-shippingfee.json")]
        public async Task<ActionResult> trackingShippingfee(string token)
        {
            var st = new Stopwatch();
            st.Start();
            try
            {
                // LogHelper.InsertLogTelegram("trackingShippingfee-->token==> error:  " + token);

                JArray objParr = null;

                #region TEST
                //int _label_id = (int)LabelType.amazon;
                //float _pound = 2;
                //string _unit = "pounds";
                //int _industry_special = -1;
                //float _price = 22;

                //string j_param = "{'price':" + _price + ", 'label_id':'" + _label_id + "','pound':'" + _pound + "','unit':'" + _unit + "','industry_special':" + _industry_special + "}";
                //  token = CommonHelper.Encode(j_param, _Configuration["KEY_TOKEN_API"]);
                #endregion

                if (!(CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"])))
                {
                    return Ok(new { status = ResponseType.FAILED.ToString(), msg = "token connect Faild" });
                }

                double price = Convert.ToDouble(objParr[0]["price"]);
                int label_id = Convert.ToInt32(objParr[0]["label_id"]);
                double pound = Convert.ToDouble(objParr[0]["pound"]);
                string unit = objParr[0]["unit"].ToString();
                int industry_special = -1;

                var product_buyer = new ProductBuyerViewModel
                {
                    LabelId = label_id,
                    Price = price,
                    Pound = Utilities.CommonHelper.convertToPound(pound, unit),
                    IndustrySpecialType = industry_special
                };

                var _shipping_fee = await _ProductRepository.getShippingFee(label_id, product_buyer);

                st.Stop();
                return Ok(new { status = ResponseType.SUCCESS.ToString(), shipping_fee = _shipping_fee, msg = "success" });
            }
            catch (Exception ex)
            {
                st.Stop();
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = ex.ToString() });
            }
        }

        //  [ResponseCache(VaryByHeader = "User-Agent", Duration = 60)]
        [HttpGet("rate.json")]
        public ActionResult getRateCurrent()
        {
            double rate_default = Convert.ToDouble(_Configuration["rate:rate_default"]);
            double percent_sell = Convert.ToDouble(_Configuration["rate:percent_sell"]);
            try
            {
                var lib = new Service.Lib.Common(_Configuration, _RedisService);
                var rate = lib.getRateCache();
                return Content(rate.ToString());
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getRateCurrent==> error:  " + ex.Message);
                return Content((rate_default + (rate_default * percent_sell) / 100).ToString());
            }
        }


        [HttpPost("province.json")]
        public async Task<ActionResult> GetProvinceList()
        {
            try
            {
                string cache_name = CacheType.PROVINCE;
                var j_province = await _RedisService.GetAsync(cache_name, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                if (j_province != null)
                {
                    var province = JsonConvert.DeserializeObject<List<Province>>(j_province);
                    return Ok(new { status = ResponseType.SUCCESS, data_list = province });
                }
                else
                {
                    var province = await _CommonRepository.GetProvinceList();
                    if (province.Count() > 0)
                    {
                        _RedisService.Set(cache_name, JsonConvert.SerializeObject(province), Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                    }
                    return Ok(new { status = province.Count() > 0 ? ResponseType.SUCCESS : ResponseType.ERROR, data_list = province });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[api==>>] GetProvinceList==> error:  " + ex.Message);
                return Ok(new { status = ResponseType.EMPTY, msg = ex.ToString() });
            }
        }

        [HttpPost("district.json")]
        public async Task<ActionResult> getDistrictListByProvinceId(string token)
        {
            try
            {
                string province_id = token;
                string cache_name = CacheType.DISTRICT + province_id;
                var j_data = await _RedisService.GetAsync(cache_name, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                if (j_data != null)
                {
                    var obj = JsonConvert.DeserializeObject<List<District>>(j_data);
                    return Ok(new { status = ResponseType.SUCCESS, data_list = obj });
                }
                else
                {
                    var district = await _CommonRepository.GetDistrictListByProvinceId(province_id);
                    if (district.Count() > 0)
                    {
                        _RedisService.Set(cache_name, JsonConvert.SerializeObject(district), Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                    }
                    return Ok(new { status = district.Count() > 0 ? ResponseType.SUCCESS : ResponseType.ERROR, data_list = district });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[api==>>] getDistrictListByProvinceId==> error:  " + ex.Message);
                return Ok(new { status = ResponseType.EMPTY, msg = ex.ToString() });
            }
        }

        [HttpPost("ward.json")]
        public async Task<ActionResult> GetWardListByDistrictId(string token)
        {
            try
            {
                string district_id = token;
                string cache_name = CacheType.WARD + district_id;
                var j_data = await _RedisService.GetAsync(cache_name, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                if (j_data != null)
                {
                    var obj = JsonConvert.DeserializeObject<List<Ward>>(j_data);
                    return Ok(new { status = ResponseType.SUCCESS, data_list = obj });
                }
                else
                {
                    var ward = await _CommonRepository.GetWardListByDistrictId(district_id);
                    if (ward.Count() > 0)
                    {
                        _RedisService.Set(cache_name, JsonConvert.SerializeObject(ward), Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                    }
                    return Ok(new { status = ward.Count() > 0 ? ResponseType.SUCCESS : ResponseType.ERROR, data_list = ward });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[api==>>] GetWardListByDistrictId==> error:  " + ex.Message);
                return Ok(new { status = ResponseType.EMPTY, msg = ex.ToString() });
            }
        }

        [HttpPost("add-answer-survery.json")]
        public async Task<ActionResult> AnswerSurvery(string token)
        {
            try
            {
                //var answerSurveryViewModel = new AnswerSurveryViewModel()
                //{
                //    FuntionId = "1",
                //    Answer = "ABCDEFGHIJK11",
                //    Email = "thangnv11@gmail.com",
                //    CreateOn = DateTime.Now
                //};
                //string j_param = "{'answer_survery':'" + Newtonsoft.Json.JsonConvert.SerializeObject(answerSurveryViewModel) + "'}";
                //  token = CommonHelper.Encode(j_param, _Configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    var answerSurveryModel = Newtonsoft.Json.JsonConvert.DeserializeObject<AnswerSurveryViewModel>
                        (objParr[0]["answer_survery"].ToString());
                    var answerSurvery = new AnswerSurvery(_Configuration);
                    var answer_survery_result = await answerSurvery.addNew(answerSurveryModel);
                    if (answer_survery_result != null)
                    {
                        return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = answer_survery_result });
                    }
                    else
                    {
                        return Ok(new { status = ResponseType.FAILED.ToString(), msg = "add answer survery fail. API/ServicePublicController", _token = token });
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("add-answer-survery.json - API/ServicePublicController: token valid !!! token =" + token);
                    return Ok(new { status = ResponseType.EXISTS.ToString(), _token = token, msg = "token valid !!!" });
                }

            }
            catch (Exception ex)
            {
                //Utilities.LogHelper.InsertLogTelegram("add-answer-survery.json - API/ServicePublicController " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = ex.ToString(), _token = token });
            }
        }

        //[HttpPost("get-all-function-survery.json")]
        //public async Task<ActionResult> GetFunctionSurvery()
        //{
        //    try
        //    {
        //        var functionSurvery = new FunctionSurvery(_Configuration);
        //        var listFunctionSurvery = functionSurvery.GetAllFunctionSurvery().Result;
        //        return Ok(new { status = ResponseType.SUCCESS.ToString(), listFunctionSurvery = JsonConvert.SerializeObject(listFunctionSurvery), msg = "Get Function Survery Success" });
        //    }
        //    catch (Exception ex)
        //    {
        //        Utilities.LogHelper.InsertLogTelegram("GetFunctionSurvery - API/ServicePublicController " + ex.Message);
        //        return Ok(new { status = ResponseType.ERROR.ToString(), listFunctionSurvery = new List<FunctionSurveryViewModel>(), msg = ex.ToString() });
        //    }
        //}

        /// <summary>
        /// Hàm này để lưu lại trạng thái của bên đối tác Kerry push về. Cập nhật cho tình trạng đơn hàng
        /// Project Api Kerry trên con 161 sẽ push về đây.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("update-order-finish")]
        public async Task<ActionResult> UpdateOrderStatusFromKerry(string token)
        {
            int _status = (int)ResponseType.FAILED;
            string _token = null;
            string _msg = "Failed";
            int statusService = -1;
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, ReadFile.LoadConfig().Kerry_Order_API_Key))
                {
                    string transfer_msg = "";
                    string order_code = objParr[0]["order_code"].ToString();
                    switch (objParr[0]["statusService"].ToString())
                    {
                        case "PUP":
                            {
                                statusService = (int)OrderStatus.CLIENT_TRANSPORT_ORDER;
                                transfer_msg = "đang được giao tới khách hàng bởi Kerry Express.";
                            }
                            break;
                        case "POD":
                            {
                                statusService = (int)OrderStatus.SUCCEED_ORDER;
                                transfer_msg = "đã được giao tới khách hàng thành công bởi Kerry Express.";

                            }
                            break;
                        default:
                            {
                            }
                            break;
                    };
                    if (order_code == null | order_code.Trim() == "" || statusService < 0)
                    {
                        _msg = "Invalid Data";
                        _token = token;
                    }
                    else
                    {
                        var result = await _orderRepository.UpdateOrderStatus(order_code, statusService);
                        if (result == null)
                        {
                            _status = (int)ResponseType.SUCCESS;
                            _msg = "Success";
                        }
                        else
                        {
                            _status = (int)ResponseType.FAILED;
                            _msg = result;

                        }
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("update-order-from-kerry.json - API/ServicePublicController: token invalid !!! token =" + token);
                    _msg = "Invalid Data";
                    _token = token;
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("update-order-from-kerry.json - API/ServicePublicController : " + ex.ToString() + "\nToken=" + token);
                _status = (int)ResponseType.ERROR;
                _msg = ex.ToString();
                _token = token;
            }
            return Ok(new { status = _status, msg = _msg, _token = _token });

        }
        /// <summary>
        /// Lấy cân nặng của đơn để post đơn mới sang Kerry.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [EnableCors("MyApi")]
        [HttpPost("get-order-weight")]
        public async Task<IActionResult> GetOrderWeight(string token)
        {
            int _status = (int)ResponseType.FAILED;
            string _msg = "Failed";
            double total_weight = 0;
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, ReadFile.LoadConfig().Kerry_Order_API_Key))
                {
                    string order_code = objParr[0]["order_code"].ToString();
                    if (order_code == null || order_code.Trim() == "")
                    {
                        _msg = "Invalid Data";
                    }
                    else
                    {
                        var order_id = await _orderRepository.FindOrderIdByOrderNo(order_code);
                        if (order_id < 0)
                        {
                            _msg = "Order Not Found.";
                        }
                        else
                        {
                            total_weight = await _orderItemRepository.GetAllItemWeightByOrderID(order_id);
                            if (total_weight > 0.5)
                            {
                                total_weight = Math.Round(total_weight, 1);
                                _status = (int)ResponseType.SUCCESS;
                                _msg = "SUCCESS";
                            }
                            else
                            {
                                total_weight = 0.5;
                                _status = (int)ResponseType.SUCCESS;
                                _msg = "SUCCESS";
                            }
                        }
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("get-order-weight - API/ServicePublicController: token invalid !!! token =" + token);
                    _msg = "Invalid Data";
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("get-order-weight - API/ServicePublicController : " + ex.ToString() + "\nToken=" + token);
                _status = (int)ResponseType.ERROR;
                _msg = ex.ToString();
            }
            return Ok(new { status = _status, msg = _msg, total_weight = total_weight });

        }
        [HttpPost("monitor-logging")]
        public async Task<ActionResult> TelegramLogging(string token)
        {
            int _status = (int)ResponseType.FAILED;
            string _token = null;
            string _msg = "Failed";
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, ReadFile.LoadConfig().Kerry_Order_API_Key))
                {
                    string msg = objParr[0]["log"].ToString();
                    string function_name = objParr[0]["name"].ToString();
                    Utilities.LogHelper.InsertLogTelegram(function_name + " : " + msg);
                    _status = (int)ResponseType.SUCCESS;
                    _msg = "Successful";
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("monitor-logging - API/ServicePublicController: token invalid !!! token =" + token);
                    _msg = "Invalid Data";
                    _token = token;
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("monitor-logging - API/ServicePublicController : " + ex.ToString() + "\nToken=" + token);
                _status = (int)ResponseType.ERROR;
                _msg = ex.ToString();
                _token = token;
            }
            return Ok(new { status = _status, msg = _msg, _token = _token });
        }

        //public async Task<ActionResult> testDeleteProductInEs(string asin)
        //{
        //    string ES_HOST = _Configuration["DataBaseConfig:Elastic:Host"];
        //    var ESRepository = new ESRepository<object>(ES_HOST);
        //    var rs = ESRepository.DeleteProductByCode("product", asin);
        //    return Ok(new { status = rs, asin = asin });
        //}
        [EnableCors("MyApi")]
        [HttpPost("get-joma-detail")]
        public async Task<IActionResult> GetJomaShopProductDetail(string url, string key)
        {
            int _status = (int)ResponseType.FAILED;
            string _msg = "Failed";
            ProductViewModel detail = null;
            var time = new Stopwatch();
            time.Restart();
            try
            {
                if (url == null || !url.Trim().Contains("jomashop.com") || !url.Trim().Contains("http"))
                {
                    _msg = "URL khong chinh xac";
                }
                else if (key != "od5XHJ1tIJQlSET9aTAulNO5ES1XTirfI2epe1QV")
                {
                    _msg = "Du lieu gui len khong chinh xac";

                }
                else
                {
                    var obj = new
                    {
                        label_id = 7,
                        url = url
                    };
                    var work_queue = new WorkQueueClient();
                    var queue_setting = new QueueSettingViewModel
                    {
                        host = _Configuration["Queue:Host"],
                        v_host = _Configuration["Queue:V_Host"],
                        port = Convert.ToInt32(_Configuration["Queue:Port"]),
                        username = _Configuration["Queue:Username"],
                        password = _Configuration["Queue:Password"]
                    };
                    var cache_name = CacheHelper.getProductDetailCacheKeyFromURL(url, (int)LabelType.jomashop);
                    var response_queue = work_queue.InsertQueueSimple(queue_setting, JsonConvert.SerializeObject(obj), TaskQueueName.joma_detail);
                    if (response_queue)
                    {
                        _msg = "Cannot find Cache [" + cache_name + "] or Cannot Crawl after 7000 ms";
                        for (int i = 0; i < 35; i++)
                        {
                            var a = _RedisService.Get(cache_name, 4);
                            if (a != null)
                            {
                                detail = JsonConvert.DeserializeObject<ProductViewModel>(a);
                                if (detail != null && detail.product_code != null && detail.product_code.Trim() != "")
                                {
                                    _status = (int)ResponseType.SUCCESS;
                                    _msg = "Success. Product code: " + detail.product_code.Trim() + ". Cache name: " + cache_name;
                                    time.Stop();
                                    _msg += ". Excutime: " + time.ElapsedMilliseconds + " ms";
                                    break;
                                }

                            }
                            await Task.Delay(200);
                        }
                    }
                    else
                    {
                        _msg = "Cannot Push Queue";

                    }

                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("get-joma-detail - API/ServicePublicController : " + ex.ToString() + "\nURL=" + url);
                _status = (int)ResponseType.ERROR;
                _msg = "Error: " + ex.ToString();
            }
            time.Restart();
            time.Stop();
            LogHelper.InsertLogTelegram("API: /ServicePublic/get-joma-detail with " + url + ": " + _msg);
            return Ok(new { status = _status, msg = _msg, data = detail });
        }

        [EnableCors("MyApi")]
        [HttpPost("extension/get-xpath")]
        public async Task<IActionResult> GetExtensionXpath(string key)
        {
            int _status = (int)ResponseType.FAILED;
            string _msg = "Failed";
            dynamic data = null;
            try
            {
                if (key != "od5XHJ1tIJQlSET9aTAulNO5ES1XTirfI2epe1QV")
                {
                    _msg = "Du lieu gui len khong chinh xac";
                    return Ok(new { status = _status, msg = _msg });
                }
                var j_data = await _RedisService.GetAsync(CacheType.EXTENSION_XPATH_V2, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                try
                {
                    data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(j_data);
                    _status = (int)ResponseType.SUCCESS;
                    _msg = "Success";
                }
                catch
                {
                    _msg = "Du lieu duoc luu khong chinh xac";

                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("/extension/get-xpath - API/ServicePublicController : " + ex.ToString());
                _status = (int)ResponseType.ERROR;
                _msg = ex.ToString();
            }
            return Ok(new { status = _status, msg = _msg, data = data });


        }
        [EnableCors("MyApi")]
        [HttpPost("extension/update-xpath")]
        public async Task<IActionResult> UpdateExtensionXpath(string key, string field, string xpath, int label_id)
        {
            int _status = (int)ResponseType.FAILED;
            string _msg = "Failed";
            try
            {
                if (key != "od5XHJ1tIJQlSET9aTAulNO5ES1XTirfI2epe1QV" || field == null || field.Trim() == "" || xpath == null || xpath.Trim() == "" || label_id < 1)
                {
                    _msg = "Du lieu gui len khong chinh xac";
                    return Ok(new { status = _status, msg = _msg });
                }
                //-- Gen New List:
                Dictionary<string, Dictionary<string, List<string>>> data = new Dictionary<string, Dictionary<string, List<string>>>();

                //-- Get from Redis:
                string cache_name = CacheType.EXTENSION_XPATH_V2;
                var j_data = await _RedisService.GetAsync(cache_name, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                bool is_not_found = false;
                if (j_data == null || j_data.Trim() == "")
                {
                    is_not_found = true;
                }
                else
                {
                    try
                    {
                        data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(j_data);
                    }
                    catch
                    {
                        is_not_found = true;
                    }
                }

                if (is_not_found)
                {
                    var xpath_label = new Dictionary<string, List<string>>();
                    xpath_label.Add(field.ToLower(), xpath.Split("||").ToList());
                    switch (label_id)
                    {
                        case (int)LabelType.amazon:
                            {
                                data.Add("amazon", xpath_label);
                            }
                            break;
                        case (int)LabelType.bestbuy:
                            {
                                data.Add("bestbuy", xpath_label);
                            }
                            break;
                        case (int)LabelType.costco:
                            {
                                data.Add("costco", xpath_label);
                            }
                            break;
                        case (int)LabelType.hautelook:
                            {
                                data.Add("hautelook", xpath_label);
                            }
                            break;
                        case (int)LabelType.jomashop:
                            {
                                data.Add("jomashop", xpath_label);
                            }
                            break;
                        case (int)LabelType.nordstromrack:
                            {
                                data.Add("nordstromrack", xpath_label);
                            }
                            break;
                        case (int)LabelType.sephora:
                            {
                                data.Add("sephora", xpath_label);
                            }
                            break;
                        case (int)LabelType.victoria_secret:
                            {
                                data.Add("victoria_secret", xpath_label);
                            }
                            break;
                        default:
                            {
                                data.Add("amazon", xpath_label);
                            }
                            break;
                    }
                    _RedisService.Set(cache_name, JsonConvert.SerializeObject(data), Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                    _status = (int)ResponseType.SUCCESS;
                    _msg = "Success";
                    return Ok(new { status = _status, msg = _msg, data = xpath });
                }
                //-- If existsting:
                string label_name = "amazon";
                switch (label_id)
                {
                    case (int)LabelType.bestbuy:
                        {
                            label_name = "amazon";
                        }
                        break;
                    case (int)LabelType.costco:
                        {
                            label_name = "costco";
                        }
                        break;
                    case (int)LabelType.hautelook:
                        {
                            label_name = "hautelook";
                        }
                        break;
                    case (int)LabelType.jomashop:
                        {
                            label_name = "jomashop";
                        }
                        break;
                    case (int)LabelType.nordstromrack:
                        {
                            label_name = "nordstromrack";
                        }
                        break;
                    case (int)LabelType.sephora:
                        {
                            label_name = "sephora";
                        }
                        break;
                    case (int)LabelType.victoria_secret:
                        {
                            label_name = "victoria_secret";
                        }
                        break;
                    default:
                        break;
                }

                if (data.ContainsKey(label_name))
                {
                    if (data[label_name].ContainsKey(field.ToLower()))
                    {
                        data[label_name][field.ToLower()] = xpath.Split("||").ToList();
                    }
                    else
                    {
                        data[label_name].Add(field.ToLower(), xpath.Split("||").ToList());
                    }
                }
                else
                {
                    data.Add(label_name, new Dictionary<string, List<string>>());
                    data[label_name].Add(field.ToLower(), xpath.Split("||").ToList());
                }

                _RedisService.Set(cache_name, JsonConvert.SerializeObject(data), Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                _status = (int)ResponseType.SUCCESS;
                _msg = "Success";
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("/extension/update-xpath - API/ServicePublicController : " + ex.ToString());
                _status = (int)ResponseType.ERROR;
                _msg = ex.ToString();
            }
            return Ok(new { status = _status, msg = _msg });


        }
        [EnableCors("MyApi")]
        [HttpPost("extension/get-fee.json")]
        public async Task<IActionResult> ExtensionGetShippingFee(string token)
        {
            try
            {
                JArray objParr = null;

                if (!(CommonHelper.GetParamWithKey(token, out objParr, "kq1jnJAJShgdRPYjiMyi")))
                {
                    return Ok(new { status = ResponseType.FAILED.ToString(), msg = "token connect Faild" });
                }

                double price = Convert.ToDouble(objParr[0]["price"]);
                int label_id = Convert.ToInt32(objParr[0]["label_id"]);
                double pound = Convert.ToDouble(objParr[0]["pound"]);
                double shipping_fee = Convert.ToDouble(objParr[0]["shipping_fee"]);
                string unit = objParr[0]["unit"].ToString();
                string product_code = objParr[0]["product_code"].ToString();
                string product_name = objParr[0]["product_name"].ToString();

                string cache_name = CacheHelper.cacheKeyProductDetail(product_code, label_id);
                int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_product_amazon"]);

                //Tỷ giá trong ngày
                var lib = new Service.Lib.Common(_Configuration, _RedisService);
                double rate = Convert.ToDouble(lib.crawlRateVCB());

                //4. Tính phí mua hộ
                var product_buyer = new ProductBuyerViewModel
                {
                    LabelId = label_id,
                    Price = price + shipping_fee,
                    Pound = Utilities.CommonHelper.convertToPound(pound, unit),
                    IndustrySpecialType = 0,
                    RateCurrent = rate,
                    ShippingUSFee = shipping_fee,
                    Unit = 1
                };

                var _shipping_fee = await _ProductRepository.getShippingFee(label_id, product_buyer);
                if (!_shipping_fee.ContainsKey("TOTAL_SHIPPING_FEE") || _shipping_fee["TOTAL_SHIPPING_FEE"] <= 0)
                {
                    _shipping_fee["TOTAL_SHIPPING_FEE"] = _shipping_fee["FIRST_POUND_FEE"] + _shipping_fee["NEXT_POUND_FEE"] + _shipping_fee["LUXURY_FEE"];
                }
                if (!_shipping_fee.ContainsKey("PRICE_LAST") || _shipping_fee["PRICE_LAST"] <= 0)
                {
                    _shipping_fee["PRICE_LAST"] = _shipping_fee["TOTAL_SHIPPING_FEE"] + price + shipping_fee;
                }


                //5. Gán phí mua hộ
                var product_fee = new ProductFeeViewModel
                {
                    label_name = LabelType.amazon.ToString(),
                    price = price + shipping_fee,
                    amount_vnd = price == 0 ? 0 : Convert.ToDouble(_shipping_fee[FeeBuyType.PRICE_LAST.ToString()]) * rate,
                    list_product_fee = _shipping_fee,
                    shiping_fee = shipping_fee,
                    total_fee = _shipping_fee["TOTAL_SHIPPING_FEE"]
                };

                // tinh ra gia ve tay
                var amount_vnd = product_fee != null ? product_fee.amount_vnd : 0;
                string url_usexpress_home = _Configuration["enpoint_us:url_usexpress_home"];
                string url_usexpress_detail = url_usexpress_home + CommonHelper.genLinkDetailProduct(LabelType.amazon.ToString(), product_code, CommonHelper.RemoveSpecialCharacters(product_name)) + "?product_source=3";
                switch (label_id)
                {
                    case (int)LabelType.bestbuy:
                        {
                            url_usexpress_detail = url_usexpress_home + CommonHelper.genLinkDetailProductOtherLabel(LabelType.bestbuy.ToString(), product_code, true);
                            if (product_fee == null) product_fee = new ProductFeeViewModel();
                            product_fee.label_name = LabelType.bestbuy.ToString();
                        }
                        break;
                    case (int)LabelType.costco:
                        {
                            url_usexpress_detail = url_usexpress_home + CommonHelper.genLinkDetailProductOtherLabel(LabelType.costco.ToString(), product_code, true);
                            if (product_fee == null) product_fee = new ProductFeeViewModel();
                            product_fee.label_name = LabelType.costco.ToString();
                        }
                        break;
                    case (int)LabelType.hautelook:
                        {
                            url_usexpress_detail = url_usexpress_home + CommonHelper.genLinkDetailProductOtherLabel(LabelType.hautelook.ToString(), product_code, true);
                            if (product_fee == null) product_fee = new ProductFeeViewModel();
                            product_fee.label_name = LabelType.hautelook.ToString();
                        }
                        break;
                    case (int)LabelType.nordstromrack:
                        {
                            url_usexpress_detail = url_usexpress_home + CommonHelper.genLinkDetailProductOtherLabel(LabelType.nordstromrack.ToString(), product_code, true);
                            if (product_fee == null) product_fee = new ProductFeeViewModel();
                            product_fee.label_name = LabelType.nordstromrack.ToString();
                        }
                        break;
                    case (int)LabelType.sephora:
                        {
                            url_usexpress_detail = url_usexpress_home + CommonHelper.genLinkDetailProductOtherLabel(LabelType.sephora.ToString(), product_code, true);
                            if (product_fee == null) product_fee = new ProductFeeViewModel();
                            product_fee.label_name = LabelType.sephora.ToString();
                        }
                        break;
                    case (int)LabelType.victoria_secret:
                        {
                            url_usexpress_detail = url_usexpress_home + CommonHelper.genLinkDetailProductOtherLabel(LabelType.victoria_secret.ToString(), product_code, true);
                            if (product_fee == null) product_fee = new ProductFeeViewModel();
                            product_fee.label_name = LabelType.victoria_secret.ToString();
                        }
                        break;
                    case (int)LabelType.jomashop:
                        {
                            url_usexpress_detail = url_usexpress_home + CommonHelper.genLinkDetailProductOtherLabel(LabelType.jomashop.ToString(), product_code, true);
                            if (product_fee == null) product_fee = new ProductFeeViewModel();
                            product_fee.label_name = LabelType.jomashop.ToString();
                        }
                        break;
                }
                //-- Trả extension:
                var extension_output = new
                {
                    product_code = product_code,
                    product_price = price + shipping_fee,
                    first_pound_fee = _shipping_fee["FIRST_POUND_FEE"],
                    next_pound_fee = _shipping_fee["NEXT_POUND_FEE"],
                    luxury_fee = _shipping_fee["LUXURY_FEE"],
                    discount_first_fee = 0,
                    total_shipping_fee = _shipping_fee["TOTAL_SHIPPING_FEE"],
                    price_last = _shipping_fee["PRICE_LAST"],
                    price_last_vnd = Math.Round(amount_vnd, 0),
                    url = url_usexpress_detail,
                };
                return Ok(new { status = ResponseType.SUCCESS.ToString(), msg = "Crawl-Success: ", data = extension_output });
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("[API] get-shipping-fee/by-pagesource.json - API/ProductController " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = ex.ToString() });
            }
        }
        [EnableCors("MyApi")]
        [HttpPost("app/update-config")]
        public async Task<IActionResult> UpdateConfig(string key, string field, string value, int app_id)
        {

            int _status = (int)ResponseType.FAILED;
            string _msg = "Failed";
            try
            {
                if (key != "drmAGfcoa8QHxGGtmtxMwVaXGAV8GpifxyWMmINhCmbrJxnBr7" || field == null || field.Trim() == "" || value == null || value.Trim() == "" || app_id < 1)
                {
                    _msg = "Du lieu gui len khong chinh xac";
                    return Ok(new { status = _status, msg = _msg });
                }
                //-- Gen New List:
                Dictionary<string, Dictionary<string, string>> data = new Dictionary<string, Dictionary<string, string>>();

                //-- Get from Redis:
                string cache_name = CacheType.APP_CONFIG;
                var j_data = await _RedisService.GetAsync(cache_name, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                bool is_not_found = false;
                if (j_data == null || j_data.Trim() == "")
                {
                    is_not_found = true;
                }
                else
                {
                    try
                    {
                        data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(j_data);
                    }
                    catch
                    {
                        is_not_found = true;
                    }
                }

                if (is_not_found)
                {
                    var xpath_config = new Dictionary<string, Dictionary<string, string>>();
                    var xpath_config_app_byid = new Dictionary<string, string>();
                    switch (app_id)
                    {
                        case 1:
                            {
                                xpath_config_app_byid.Add(field, value);
                                xpath_config.Add("App_Auto_Mapping_Push_Queue", xpath_config_app_byid);
                            }
                            break;
                       
                        default:
                            {
                                xpath_config_app_byid.Add(field, value);
                                xpath_config.Add("Global", xpath_config_app_byid);
                            }
                            break;
                    }

                    _RedisService.Set(cache_name, JsonConvert.SerializeObject(data), Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                    _status = (int)ResponseType.SUCCESS;
                    _msg = "Success";
                    return Ok(new { status = _status, msg = _msg, data = xpath_config });
                }
                //-- If existsting:
                var app_name = "Global";
                switch (app_id)
                {
                    case 1:
                        {
                            app_name = "App_Auto_Mapping_Push_Queue";
                        }
                        break;
                   
                    default:
                        break;
                }

                if (data.ContainsKey(app_name))
                {
                    if (data[app_name].ContainsKey(field.ToLower()))
                    {
                        data[app_name][field.ToLower()] = value;
                    }
                    else
                    {
                        data[app_name].Add(field.ToLower(),value);
                    }
                }
                else
                {
                    data.Add(app_name, new Dictionary<string,string>());
                    data[app_name].Add(field.ToLower(),value);
                }

                _RedisService.Set(cache_name, JsonConvert.SerializeObject(data), Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                _status = (int)ResponseType.SUCCESS;
                _msg = "Success";
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("/extension/update-xpath - API/ServicePublicController : " + ex.ToString());
                _status = (int)ResponseType.ERROR;
                _msg = ex.ToString();
            }
            return Ok(new { status = _status, msg = _msg });
        }
        [EnableCors("MyApi")]
        [HttpPost("app/update-config")]
        public async Task<IActionResult> GetExtensionXpath(string key,int app_id)
        {
            int _status = (int)ResponseType.FAILED;
            string _msg = "Failed";
            dynamic data = null;
            try
            {
                if (key != "drmAGfcoa8QHxGGtmtxMwVaXGAV8GpifxyWMmINhCmbrJxnBr7")
                {
                    _msg = "Du lieu gui len khong chinh xac";
                    return Ok(new { status = _status, msg = _msg });
                }
                var j_data = await _RedisService.GetAsync(CacheType.APP_CONFIG, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                try
                {
                    var config= JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(j_data);
                    switch (app_id)
                    {
                        case 1:
                            {
                                data = config["App_Auto_Mapping_Push_Queue"];
                            }
                            break;

                        default:
                            {
                                data = config["Global"];
                            }
                            break;
                    }
                    _status = (int)ResponseType.SUCCESS;
                    _msg = "Success";
                }
                catch
                {
                    _msg = "Du lieu duoc luu khong chinh xac";

                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("/extension/get-xpath - API/ServicePublicController : " + ex.ToString());
                _status = (int)ResponseType.ERROR;
                _msg = ex.ToString();
            }
            return Ok(new { status = _status, msg = _msg, data = data });


        }
    }

}