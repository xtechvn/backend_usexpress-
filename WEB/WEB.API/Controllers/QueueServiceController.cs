using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Utilities;
using Utilities.Contants;
using WEB.API.Common;
using WEB.API.Service.Queue;
using WEB.API.ViewModels;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueueServiceController : BaseController
    {
        private readonly IConfiguration _Configuration;
        public QueueServiceController(IConfiguration Configuration)
        {
            _Configuration = Configuration;
        }


        /// <summary>
        ///Push data to queue
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("data-push.json")]
        public ActionResult pushDataToQueue(string token)
        {
            var st = new Stopwatch();
            st.Start();
            try
            {
                JArray objParr = null;
                bool response_queue = false;

                #region TEST
                //var j_param = new Dictionary<string, string>
                //{
                //    {"data_push","https://www.amazon.com/EZVIZ-C3W-ezGuard-1080p-Activated/dp/B079D8CTWJ"},
                //    {"type",TaskQueueName.product_detail_amazon_crawl_queue},
                //};
                //var j_param = new Dictionary<string, string>
                //{
                //    {"data_push","tinhte085@gmail.com"},
                //    {"type",TaskQueueName.client_old_convert_queue},
                //};
                // token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), "5fDmJ8Ze");

                //var j_param = new Dictionary<string, string>
                //{
                //    {"data_push","UAM-0G29304"},
                //    {"type",TaskQueueName.order_old_convert_queue},
                //};
                //token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), "5fDmJ8Ze");
                //var j_param = new Dictionary<string, string>
                //{
                //    {"data_push","18393"},
                //    {"type",TaskQueueName.client_new_convert_queue},
                //};


                //var product_detail = new Dictionary<string, string>
                //{
                //    {"label_id","1"},
                //    {"cache_name","data_feed_amazon"},
                //    {"product_code","B097777"},
                //    {"link","www.amazon.com"},
                //};

                //var j_param = new Dictionary<string, string>
                //{
                //    {"data_push",JsonConvert.SerializeObject(product_detail)},
                //    {"type",TaskQueueName.data_feed},
                //};

                //var obj_param_group_product = new Dictionary<string, string>
                //{
                //    {"product_manual_key_id","61d6a9b7723a46d59bdb5d6d"},
                //    {"group_id","260,267"}
                //};
                //var j_param = new Dictionary<string, string>
                //{
                //    {"data_push",(JsonConvert.SerializeObject(obj_param_group_product))},
                //    {"type",TaskQueueName.product_detail_manual_queue},
                //};
                //token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), "5fDmJ8Ze");
                #endregion END TEST

                if (CommonHelper.GetParamWithKey(token, out objParr, EncryptApi))
                {
                    var data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(objParr.ToString());
                    if (data.Count == 0)
                    {
                        return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.SUCCESS.ToString(), " Data empty !!!", token, st.ElapsedMilliseconds + " ms")));
                    }

                    string _queue_type = (data[0].FirstOrDefault(x => x.Key == "type").Value.ToString()); // queue name
                    string _data_push = (data[0].FirstOrDefault(x => x.Key == "data_push").Value.ToString()); // data push to queue

                    //detect type queue
                    switch (_queue_type)
                    {
                        case TaskQueueName.order_old_convert_queue:
                            var order_param = new Dictionary<string, string>
                            {
                                {"order_no",_data_push}
                            };
                            _data_push = JsonConvert.SerializeObject(order_param);
                            break;
                        case TaskQueueName.client_old_convert_queue:
                            var client_param = new Dictionary<string, string>
                            {
                                {"email",_data_push}
                            };
                            _data_push = JsonConvert.SerializeObject(client_param);
                            break;
                        case TaskQueueName.product_detail_amazon_crawl_queue:
                            var product_detail_param = new Dictionary<string, string>
                            {
                                {"asin",_data_push}
                            };
                            _data_push = JsonConvert.SerializeObject(product_detail_param);
                            break;
                        case TaskQueueName.client_new_convert_queue:
                        case TaskQueueName.order_new_convert_queue:
                        case TaskQueueName.data_feed:
                        case TaskQueueName.group_product_mapping: // data mapping group product
                        case TaskQueueName.group_product_mapping_detail:
                        case TaskQueueName.keyword_crawl_queue:
                        case TaskQueueName.joma_detail:
                        case TaskQueueName.product_detail_manual_queue:
                            //_data_push: là value
                            break;

                        default:
                            return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.SUCCESS.ToString(), "Task Queue Name Not Support!", token, st.ElapsedMilliseconds + " ms")));
                    }

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
                    response_queue = work_queue.InsertQueueSimple(queue_setting, _data_push, _queue_type);
                    if (response_queue)
                    {
                        st.Stop();
                        //LogHelper.InsertLogTelegram(_data_push + " publish queue success !" + "==> token = " + token);
                        return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.SUCCESS.ToString(), "Push Queue Success !!!", token, st.ElapsedMilliseconds + " ms")));
                    }
                    else
                    {
                        st.Stop();
                        LogHelper.InsertLogTelegram(" publish queue ERROR !" + "==> token = " + token);
                        return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.ERROR.ToString(), "Push Queue ERROR !!!", token, st.ElapsedMilliseconds + " ms")));
                    }
                }
                else
                {
                    st.Stop();
                    LogHelper.InsertLogTelegram( "Push Queue Faild: Token invalid !" + "==> token = " + token);
                    return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.EXISTS.ToString(), "Push Queue Faild: Token invalid !!!", token, st.ElapsedMilliseconds + " ms")));
                }
            }
            catch (Exception ex)
            {
                st.Stop();
                LogHelper.InsertLogTelegram(ControllerContext.ActionDescriptor.ControllerName + "/" + ControllerContext.ActionDescriptor.ActionName + "==> error:  " + ex.Message + "==> token =" + token);
                return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.EXISTS.ToString(), "Token invalid !!!", token, st.ElapsedMilliseconds + " ms")));
            }
        }
       

        [HttpPost("push-product-more-queue.json")]
        public ActionResult pushProductToQueue(string product_id, string label_type, string link_detail_product, string task_queue)
        {
            var st = new Stopwatch();
            st.Start();
            try
            {
                var j_param = new Dictionary<string, string>
                {
                    {"product_id",product_id},
                   {"label_type",label_type},
                    {"link_detail_product",link_detail_product}
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
                var response_queue = work_queue.InsertQueueSimple(queue_setting, JsonConvert.SerializeObject(j_param), task_queue);
                if (response_queue)
                {
                    st.Stop();
                    return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.SUCCESS.ToString(), "Push Queue Success !!! data = " + JsonConvert.SerializeObject(j_param), link_detail_product, st.ElapsedMilliseconds + " ms")));
                }
                else
                {
                    st.Stop();
                    return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.ERROR.ToString(), "Push Queue ERROR !!!", JsonConvert.SerializeObject(j_param), st.ElapsedMilliseconds + " ms")));
                }

            }

            catch (Exception ex)
            {

                st.Stop();
                LogHelper.InsertLogTelegram(ControllerContext.ActionDescriptor.ControllerName + "/" + ControllerContext.ActionDescriptor.ActionName + "==> error:  " + ex.Message + "==> link_detail_product =" + link_detail_product);
                return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.EXISTS.ToString(), "Queue invalid !!!", "", st.ElapsedMilliseconds + " ms")));
            }
        }
        /// <summary>
        /// Api này sẽ push các sản phẩm product để đi crawler offline
        /// </summary>
        /// <param name="product_id"></param>
        /// <param name="label_type"></param>
        /// /// <param name="link_detail_product"></param>
        /// AppReceiverAnalysCrawler sẽ đọc param từ Api này
        /// <returns></returns>
        [HttpPost("product-push.json")]
        public ActionResult pushProductToQueue(string token)
        {

            var st = new Stopwatch();
            st.Start();
            try
            {
                JArray objParr = null;
                string task_queue = TaskQueueName.product_crawl_queue;
                //var j_param = new Dictionary<string, string>
                //{
                //    {"product_id","B072M34RQC"},
                //    {"label_type",( (int)LabelType.amazon).ToString()},
                //    {"link_detail_product","https://www.amazon.com/HP-23-8-inch-Adjustment-Speakers-VH240a/dp/B072M34RQC/ref=lp_16225007011_1_1?s=computers-intl-ship&ie=UTF8&qid=1594577588&sr=1-1"}
                //};
                //token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), QUEUE_KEY_API);

                //string task_queue = TaskQueueName.product_detail_more_crawl_queue;
                //var j_param = new Dictionary<string, string>
                //{
                //    {"product_id","111506"},
                //   {"label_type",( (int)LabelType.jomashop).ToString()},
                //    {"link_detail_product","https://www.jomashop.com/montblanc-111506.html"}
                //};
                //token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), QUEUE_KEY_API);

                if (CommonHelper.GetParamWithKey(token, out objParr, QUEUE_KEY_API))
                {
                    var work_queue = new WorkQueueClient();
                    var queue_setting = new QueueSettingViewModel
                    {
                        host = _Configuration["Queue:Host"],
                        v_host = _Configuration["Queue:V_Host"],
                        port = Convert.ToInt32(_Configuration["Queue:Port"]),
                        username = _Configuration["Queue:Username"],
                        password = _Configuration["Queue:Password"]
                    };
                    var response_queue = work_queue.InsertQueueSimple(queue_setting, objParr.First.ToString(), task_queue);
                    if (response_queue)
                    {
                        st.Stop();
                        return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.SUCCESS.ToString(), "Push Queue Success !!! data = " + objParr.First.ToString(), token, st.ElapsedMilliseconds + " ms")));
                    }
                    else
                    {
                        st.Stop();
                        return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.ERROR.ToString(), "Push Queue ERROR !!!", token, st.ElapsedMilliseconds + " ms")));
                    }
                }
                else
                {
                    st.Stop();
                    return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.EXISTS.ToString(), "Push Queue Faild: Token invalid !!!", token, st.ElapsedMilliseconds + " ms")));
                }
            }
            catch (Exception ex)
            {
                st.Stop();
                LogHelper.InsertLogTelegram(ControllerContext.ActionDescriptor.ControllerName + "/" + ControllerContext.ActionDescriptor.ActionName + "==> error:  " + ex.Message + "==> token =" + token);
                return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.EXISTS.ToString(), "Token invalid !!!", token, st.ElapsedMilliseconds + " ms")));
            }
        }

        /// <summary>
        /// Api này dùng crawl sản phẩm theo chuẩn RPC QUEUE
        /// Nhược điểm: thời gian phản hồi chậm hơn so với việc dùng Redis
        /// </summary>
        /// <param name="link_crawl"></param>
        /// <returns></returns>
        [HttpPost("product-scrapy-se.json")]
        public async Task<ActionResult> CrawlData(string token)
        {
            var st = new Stopwatch();
            st.Start();
            try
            {
                string response_server = string.Empty;
                JArray objParr = null;
                //var j_param = new Dictionary<string, string>
                //{
                //    {"link_crawl","https://www.amazon.com/Silicone-Sonic-Facial-Cleansing-Brush/dp/B07KY7CD9S?ref_=Oct_DLandingS_D_85bf973f_63&smid=A3V9AGKCJIX5OG"},
                //    {"type",TaskQueueName.client_old_convert_queue},
                //};
                //  token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), "5fDmJ8Ze");
                if (CommonHelper.GetParamWithKey(token, out objParr, EncryptApi))
                {
                    var data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(objParr.ToString());
                    string link_crawl = (data[0].FirstOrDefault(x => x.Key == "link_crawl").Value.ToString()); // link_crawl

                    var queue_param = new QueueRPcViewModel(link_crawl, ResponseType.PROCESSING.ToString());
                    var message = JsonConvert.SerializeObject(queue_param);

                    var rpcClient = new RpcClient();
                    var response = await rpcClient.CallAsync(message);
                    // rpcClient.Close();

                    st.Stop();
                    return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.SUCCESS.ToString(), "response_server = " + response, response_server, st.ElapsedMilliseconds + " ms")));
                }
                return Content(JsonConvert.SerializeObject(ResponseApi(ResponseType.ERROR.ToString(), "Push Queue ERROR !!!", token, "")));
            }
            catch (Exception)
            {
                throw;
            }
        }




    }
}