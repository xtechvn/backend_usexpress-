using Caching.Elasticsearch;
using Entities.ViewModels.Affiliate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.API.Model.Affiliate;
using WEB.API.Service.ClientAffiliates;
using static Utilities.Contants.Constants;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AffilliateController : BaseController
    {
        public IConfiguration _configuration;
        private Affiliate _affiliate;
        private readonly IOrderRepository _OrderRepository;
        private readonly IAffiliateGroupProductRepository _AffiliateGroupProductRepository;
        private readonly IGroupProductRepository _groupProductRepository;
        
        public AffilliateController(IConfiguration config, IOrderRepository orderRepository, IAffiliateGroupProductRepository AffiliateGroupProductRepository,
            IGroupProductRepository groupProductRepository)
        {
            _configuration = config;
            _affiliate = new Affiliate(config);
            _OrderRepository = orderRepository;
            _AffiliateGroupProductRepository = AffiliateGroupProductRepository;
            _groupProductRepository = groupProductRepository;
            
        }
        [HttpPost("addnew.json")]
        public async Task<ActionResult> Addnew(string token)
        {
            JArray objParr = null;
            try
            {
                AffiliateViewModel x = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    var aff_item = Newtonsoft.Json.JsonConvert.DeserializeObject<AffiliateViewModel>(objParr[0].ToString());
                    if (aff_item != null && (aff_item.id == null || aff_item.id == ""))
                    {
                        x = await _affiliate.Add(aff_item);
                    }
                    if (x != null && x.id != null)
                    {
                        return Ok(new
                        {
                            status = ResponseType.SUCCESS.ToString(),
                            data = x,
                            msg = "Successs!!!!"
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = ResponseType.FAILED.ToString(),
                            data = aff_item,
                            msg = "Failed !!!!"
                        });
                    }
                }
                else
                {

                    return Ok(new
                    {
                        status = ResponseType.FAILED.ToString(),
                        token = token,
                        msg = "token invalid !!!!"
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = ResponseType.ERROR.ToString(),
                    token = token,
                    msg = "Error on Excution"
                });
            }
        }

        [HttpPost("find.json")]
        public async Task<ActionResult> Find(string token)
        {
            JArray objParr = null;
            try
            {
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    double client_id = Convert.ToDouble(objParr[0]["client_id"].ToString());
                    var x = await _affiliate.IsAffiliateOrder(client_id);
                    return Ok(new
                    {
                        status = ResponseType.SUCCESS.ToString(),
                        data = x,
                        msg = x > -1 ? "Belong to Affilliate" : "DIRECT "
                    });
                }
                else
                {

                    return Ok(new
                    {
                        status = ResponseType.FAILED.ToString(),
                        token = token,
                        msg = "token invalid !!!!"
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = ResponseType.ERROR.ToString(),
                    token = token,
                    msg = "Error on Excution"
                });
            }
        }


        /// <summary>
        /// Create By: Minh
        /// Edit By: CuongLv
        /// Api này để trả ra datafeed theo từng đối tác 
        /// </summary>
        /// <param name="token">
        /// group_id : nhóm ngành hàng cần lấy
        /// aff_type : quy ước type của đối tác. 1: cho AT
        /// </param>
        /// <returns></returns>
        [HttpPost("get-data-feed.json")]
        public async Task<ActionResult> GetAffilliateDataFeed(string token)
        {
            string _msg = string.Empty;
            string data = null;
            int status = 0;
            try
            {

                //string j_param = "{'group_id':'5,2','aff_type':1}";
                //token = CommonHelper.Encode(j_param, EncryptApi);
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    string group_list = objParr[0]["group_id"].ToString();

                    int aff_type = Convert.ToInt32(objParr[0]["aff_type"]);

                    switch (aff_type)
                    {
                        case AffiliateType.accesstrade:
                            {
                                var result = new List<AccesstradeDataFeed>();
                                foreach (var group_id in group_list.Trim().Split(","))
                                {
                                    string ES_HOST = _configuration["DataBaseConfig:Elastic:Host"];
                                    var ESRepository = new ESRepository<object>(ES_HOST);
                                    var list_product = ESRepository.GetByGroupID(_configuration["DataBaseConfig:Elastic:index_product_search"], group_id);
                                    int cate_id = Convert.ToInt32(group_id);
                                    var group_product_name = "Data Feed";
                                    if (cate_id > 0) group_product_name = await _groupProductRepository.GetGroupProductNameAsync(cate_id);
                                    if (list_product != null && list_product.Count > 0)
                                    {
                                        foreach (var item in list_product)
                                        {
                                            if (item.price > 0 && item.amount > 0 && item.product_code != "")
                                                result.Add(new AccesstradeDataFeed
                                                {
                                                    sku = item.product_id.ToString(),
                                                    name = item.product_name.Replace(",", "-"),
                                                    id = item.product_code.Replace(",", "-").ToString(),
                                                    price = (int)item.amount_vnd,
                                                    retail_Price = (int)item.amount_vnd,
                                                    url = _configuration["enpoint_us:url_usexpress_home"] + item.link_product.Replace(",", ""),
                                                    image_url = item.image_thumb.Replace(",", ""),
                                                    category_id = item.group_product_id.ToString(),
                                                    category_name = group_product_name
                                                });
                                        }

                                    }
                                }
                                status = (int)ResponseType.SUCCESS;
                                _msg = "Successful";
                                return Ok(new { status = status, data = result, msg = _msg });
                            }
                        case AffiliateType.adpia:
                            {
                                var result = new List<AdpiaDataFeed>();
                                foreach (var group_id in group_list.Trim().Split(","))
                                {
                                    string ES_HOST = _configuration["DataBaseConfig:Elastic:Host"];
                                    var ESRepository = new ESRepository<object>(ES_HOST);
                                    var list_product = ESRepository.GetByGroupID(_configuration["DataBaseConfig:Elastic:index_product_search"], group_id);
                                    int cate_id = Convert.ToInt32(group_id);
                                    var group_product_name = "Data Feed";
                                    if (cate_id > 0) group_product_name = await _groupProductRepository.GetGroupProductNameAsync(cate_id);
                                    if (list_product != null && list_product.Count > 0)
                                    {
                                        foreach (var item in list_product)
                                        {
                                            if (item.price > 0 && item.amount > 0 && item.product_code != "" && result.Count < 100)
                                                result.Add(new AdpiaDataFeed
                                                {
                                                    product_name = item.product_name.Replace(",", "-"),
                                                    product_id = item.product_code.Replace(",", "-"),
                                                    category = group_product_name,
                                                    price = item.amount_vnd,
                                                    discount = item.discount > 0 ? (int)item.discount : 0,
                                                    link = _configuration["enpoint_us:url_usexpress_home"] + item.link_product.Replace(",", ""),
                                                    image = item.image_thumb.Replace(",", ""),
                                                    url = _configuration["enpoint_us:url_usexpress_home"] + item.link_product.Replace(",", "")
                                                });
                                        }

                                    }
                                }
                                status = (int)ResponseType.SUCCESS;
                                _msg = "Successful";
                                return Ok(new { status = status, data = result, msg = _msg });
                            }
                        default:
                            {
                                status = (int)ResponseType.FAILED;
                                _msg = "Đối tác chưa được hỗ trợ hoặc mã đối tác không đúng.";
                            }
                            break;
                    }
                }
                else
                {
                    status = (int)ResponseType.FAILED;
                    _msg = "Token khong hop le: token = " + token;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api/Affilliate/get-at-data-feed.json: " + ex);
                status = (int)ResponseType.ERROR;
                _msg = "Error On Excution";

            }
            return Ok(new { status = status, data = "", msg = _msg });
        }

        #region VINH: ACESSTRADE       
        [HttpPost("update-affiliate.json")]
        public async Task<ActionResult> UpdateAffiliate(string token)
        {
            JArray objParr = null;
            try
            {
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    long order_id = Convert.ToInt64(objParr[0]["order_id"]);
                    string utm_medium = objParr[0]["utm_medium"].ToString();
                    string utm_campaign = objParr[0]["utm_campaign"].ToString();
                    string utm_firsttime = objParr[0]["utm_firsttime"].ToString();
                    string utm_source = objParr[0]["utm_source"].ToString();
                    var order = await _OrderRepository.GetOrderDetail(order_id);
                    order.UtmMedium = utm_medium;
                    order.UtmCampaign = utm_campaign;
                    order.UtmFirstTime = utm_firsttime;
                    order.UtmSource = utm_source;
                    var rs = await _OrderRepository.Update(order);
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api: order/update-affiliate.json ==> error:  " + ex.Message);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                });
            }
        }
        /// <summary> 
        /// endpoind gửi lại đơn hàng đến accesstrade
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("at-create-order.json")]
        public async Task<ActionResult> PostOrderToAT(string token)
        {
            JArray objParr = null;
            try
            {
                //string j_param = "{'order_id':16626}";
                //token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);

                //Decode ra được dữ liệu
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    long order_id = Convert.ToInt64(objParr[0]["order_id"]);
                    var order_detail = await _OrderRepository.GetOrderDetail(order_id);
                    var order_items = await _OrderRepository.GetOrderItemList(order_id);
                    int at_status = -1; string order_rejected_reason = null;
                    switch (order_detail.OrderStatus)
                    {
                        case (int)OrderStatus.PAID_ORDER:
                        case (int)OrderStatus.SUCCEED_ORDER:
                            at_status = 1;
                            order_rejected_reason = "Đơn hàng thành công";
                            break;

                        case (int)OrderStatus.BUY_FAILED_ORDER:
                        case (int)OrderStatus.CANCEL_ORDER:
                            at_status = 2;
                            order_rejected_reason = ((OrderStatus)order_detail.OrderStatus).GetDisplayName();
                            break;
                        default:
                            at_status = 2;
                            order_rejected_reason = "Đơn hàng bị hủy";
                            break;
                    }

                    if (at_status > -1)
                    {
                        var body = new AccesstradeOrderPostback()
                        {
                            conversion_id = order_detail.OrderNo,
                            conversion_result_id = "30",
                            tracking_id = order_detail.TrackingId == null ? "P5KQc6CjdUWXUK8PVoO3JSZZ5c3VRuHmjoIEMfhZC4lcMjn1" : order_detail.TrackingId,
                            transaction_id = order_detail.OrderNo,
                            transaction_time = (DateTime)order_detail.CreatedOn != null ? (DateTime)order_detail.CreatedOn : DateTime.Now,
                            transaction_value = (float)order_detail.AmountVnd,
                            transaction_discount = (float)(order_detail.TotalDiscount2ndVnd + order_detail.TotalDiscountVoucherVnd),
                            items = new List<AccesstradeOrderItem>(),
                            status = at_status,
                        };
                        foreach (var orderitem in order_items)
                        {
                            if (order_items != null)
                            {
                                body.items.Add(new AccesstradeOrderItem
                                {
                                    id = orderitem.ProductId.ToString(),
                                    sku = orderitem.ProductCode,
                                    name = orderitem.ProductName,
                                    price = (float)orderitem.Price,
                                    quantity = orderitem.Quantity,
                                    category = orderitem.LabelId.ToString(), // edit
                                    category_id = orderitem.LabelId.ToString(), //edit
                                });
                            }
                        }
                        if (order_rejected_reason != null)
                        {
                            body.extra = new Dictionary<string, string>();
                            body.extra.Add("rejected_reason", order_rejected_reason);
                            if (body.items != null && body.items.Count > 0)
                                foreach (var postback_item in body.items)
                                {
                                    if (postback_item != null)
                                    {
                                        if (postback_item.extra == null) postback_item.extra = new Dictionary<string, string>();
                                        postback_item.extra.Add("rejected_reason", order_rejected_reason);
                                    }

                                }
                        }
                        /*
                    var body = new
                    {
                        conversion_id = order_detail.OrderNo,
                        conversion_result_id = "30",
                        tracking_id = order_detail.TrackingId==null? "wOPs7HHGmAh2msfuLK73odJs5gOtunJaVljDcBvXxISyLPvK" : order_detail.TrackingId,
                        transaction_id = order_detail.OrderNo,
                        transaction_time = order_detail.CreatedOn,
                        transaction_value = order_detail.AmountVnd,
                        transaction_discount = order_detail.TotalDiscount2ndVnd + order_detail.TotalDiscountVoucherVnd,
                        status= 0,
                        extra = new {
                            rejected_reason="cancel by customer"
                         },
                        items = order_items.Select(s => new
                        {
                            id = s.ProductId,
                            sku = s.ProductCode,
                            name = s.ProductName,
                            price = s.Price,
                            quantity = s.Quantity,
                            category = s.LabelId, // edit
                            category_id = s.LabelId, //edit
                            status= 0,
                            extra = new
                            {
                                rejected_reason = "cancel by customer"
                            },
                        })
                    };
                   */
                        HttpClient httpClient = new HttpClient();
                        httpClient.DefaultRequestHeaders.Add("Authorization", "Token " + _configuration["access_trade:key"]);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var content = JsonConvert.SerializeObject(body);
                        var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                        var responseInsert = await httpClient.PostAsync(_configuration["access_trade:api_url"], stringContent);
                        var result = responseInsert.Content.ReadAsStringAsync().Result;
                        var result_content = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
                        if (responseInsert.StatusCode == System.Net.HttpStatusCode.OK && result_content["success"] != null && (bool)result_content["success"] == true)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = "Push đơn hàng sang Accesstrade thành công. Response = " + result
                            });
                        }
                        else
                        {
                            LogHelper.InsertLogTelegram("api: at-create-order.json ==> Push Order Failed, Result:  " + result + "\n With Order: " + content);
                            return Ok(new
                            {
                                status = (int)ResponseType.FAILED,
                                msg = "Push đơn hàng sang Accesstrade thất bại, Response = " + result
                            });
                        }
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Trạng thái đơn hàng không chính xác, vui lòng kiểm tra lại đơn hàng. Status = " + at_status
                        });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Token Invalid"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api: at-create-order.json ==> error:  " + ex.Message);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Lỗi trong quá trình xử lý, vui lòng liên hệ bộ phận kỹ thuật"
                });
            }
        }

        /// <summary>
        /// endpoind cập nhật trạng thái đơn hàng đến accesstrade
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("at-update-status.json")]
        public async Task<ActionResult> PutOrderStatusToAT(string token)
        {
            JArray objParr = null;
            try
            {
                //string j_param = "{'order_id':16626,'order_status':13}";
                //token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);

                //Decode ra được dữ liệu
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    long order_id = Convert.ToInt64(objParr[0]["order_id"]);
                    long order_status = Convert.ToInt32(objParr[0]["order_status"]);
                    var order_detail = await _OrderRepository.GetOrderDetail(order_id);
                    var order_items = await _OrderRepository.GetOrderItemList(order_id);

                    var at_status = 0;
                    string order_rejected_reason = string.Empty;
                    switch (order_status)
                    {
                        case (int)OrderStatus.PAID_ORDER:
                        case (int)OrderStatus.SUCCEED_ORDER:
                            at_status = 1;
                            order_rejected_reason = "Đơn hàng thành công";
                            break;

                        case (int)OrderStatus.BUY_FAILED_ORDER:
                        case (int)OrderStatus.CANCEL_ORDER:
                            at_status = 2;
                            order_rejected_reason = ((OrderStatus)order_status).GetDisplayName();
                            break;
                        default:
                            at_status = 2;
                            order_rejected_reason = "Đơn hàng bị hủy";
                            break;
                    }
                    var body = new AccesstradeOrderPostback()
                    {
                        conversion_id = order_detail.OrderNo,
                        transaction_id = order_detail.OrderNo,
                        status = at_status,
                        extra = new Dictionary<string, string>(),
                        items = new List<AccesstradeOrderItem>()
                    };

                    foreach (var orderitem in order_items)
                    {
                        if (order_items != null)
                        {
                            body.items.Add(new AccesstradeOrderItem
                            {
                                id = orderitem.ProductId.ToString(),
                                status = at_status
                            });
                        }
                    }
                    if (at_status == 2)
                    {
                        body.extra.Add("rejected_reason", order_rejected_reason);
                        body.extra = new Dictionary<string, string>();
                        body.extra.Add("rejected_reason", order_rejected_reason);
                        if (body.items != null && body.items.Count > 0)
                            foreach (var postback_item in body.items)
                            {
                                if (postback_item != null)
                                {
                                    if (postback_item.extra == null) postback_item.extra = new Dictionary<string, string>();
                                    postback_item.extra.Add("rejected_reason", order_rejected_reason);
                                }

                            }
                    }
                    /*
                    var body = new
                    {
                        transaction_id = order_detail.OrderNo,
                        status = at_status,
                        rejected_reason = order_rejected_reason,
                        items = order_items.Select(s => new
                        {
                            id = s.ProductCode,
                            status = at_status,
                            extra = new
                            {
                                rejected_reason = 
                            }
                        })
                    };
                    */
                    using (HttpClient httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("Authorization", "Token " + _configuration["access_trade:key"]);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        string content = JsonConvert.SerializeObject(body);
                        var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                        var responseInsert = await httpClient.PutAsync(_configuration["access_trade:api_url"], stringContent);
                        var result = responseInsert.Content.ReadAsStringAsync().Result;
                        var result_content = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
                        if (responseInsert.StatusCode == System.Net.HttpStatusCode.OK && result_content["success"] != null && (bool)result_content["success"] == true)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = "Cập nhật trạng thái đơn hàng sang Accesstrade thành công. Response = " + result
                            });
                        }
                        else
                        {
                            LogHelper.InsertLogTelegram("api:at-update-status.json ==> Update Order To AT failed. Result:  " + result + "\n With Order: " + content);
                            return Ok(new
                            {
                                status = (int)ResponseType.FAILED,
                                msg = "Cập nhật trạng thái đơn hàng sang Accesstrade thất bại, Response = " + result
                            });
                        }
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Token Invalid"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api:at-update-status.json ==> error:  " + ex.Message);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Lỗi trong quá trình xử lý, vui lòng liên hệ bộ phận kỹ thuật"
                });
            }
        }
        #endregion

        /// <summary>
        /// Create BY: Cuonglv
        /// Lấy ra danh sách nhưng nhóm hàng được set để đẩy qua các đối tác
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-all-affiliate-group-product.json")]
        public async Task<ActionResult> GetAllAffiliateGroupProduct(string token)
        {
            try
            {
                JArray objParr = null;
                //string j_param = "{'id':-1}";
                //token = CommonHelper.Encode(j_param, EncryptApi);

                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    var aff = await _AffiliateGroupProductRepository.GetAllAffiliateGroupProduct();
                    if (aff.Count > 0)
                    {
                        return Ok(new { status = (int)ResponseType.SUCCESS, data = aff });
                    }
                    else
                    {
                        return Ok(new { status = (int)ResponseType.EMPTY });
                    }
                }
                else
                {
                    return Ok(new { status = (int)ResponseType.ERROR, msg = "token k hop le" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api/Affilliate/get-all-affiliate-group-product.json " + ex);
                return Ok(new { status = (int)ResponseType.ERROR, msg = ex.ToString() });

            }

        }
        /// <summary> 
        /// endpoind gửi lại đơn hàng đến adpia
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("adpia-create-order.json")]
        public async Task<ActionResult> PostOrderToAdpia(string token)
        {
            JArray objParr = null;
            try
            {
                //string j_param = "{'order_id':16626}";
                //token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);

                //Decode ra được dữ liệu
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    long order_id = Convert.ToInt64(objParr[0]["order_id"]);
                    var order_detail = await _OrderRepository.GetOrderDetail(order_id);
                    var order_items = await _OrderRepository.GetOrderItemList(order_id);
                    int at_status = -1; string order_rejected_reason = null;
                    switch (order_detail.OrderStatus)
                    {
                        case (int)OrderStatus.PAID_ORDER:
                        case (int)OrderStatus.SUCCEED_ORDER:
                            at_status = 1;
                            order_rejected_reason = "Đơn hàng thành công";
                            break;

                        case (int)OrderStatus.BUY_FAILED_ORDER:
                        case (int)OrderStatus.CANCEL_ORDER:
                            at_status = 2;
                            order_rejected_reason = ((OrderStatus)order_detail.OrderStatus).GetDisplayName();
                            break;
                        default:
                            at_status = 2;
                            order_rejected_reason = "Đơn hàng bị hủy";
                            break;
                    }

                    if (at_status > -1)
                    {
                        var body = new AdpiaOrderPostback()
                        {
                            order_code = order_detail.OrderNo,
                            order_time = (DateTime)order_detail.CreatedOn != null ? (DateTime)order_detail.CreatedOn : DateTime.Now,
                            order_value = Convert.ToInt32(order_detail.AmountVnd),
                            track_id = order_detail.TrackingId == null ? "QTEwMDAwMDEyMzoxMjMuMzIxLjIxMi4xMjoxMTEz" : order_detail.TrackingId,
                            items = new List<AdpiaOrderItem>(),
                        };
                        foreach (var orderitem in order_items)
                        {
                            if (order_items != null)
                            {
                                body.items.Add(new AdpiaOrderItem
                                {
                                    pcd = orderitem.ProductCode,
                                    pnm = orderitem.ProductName,
                                    price = (float)orderitem.Price,
                                    cnt = orderitem.Quantity,
                                    ccd = orderitem.LabelId.ToString(), // edit
                                });
                            }
                        }
                        string adpia_account = _configuration["adpia:account"];
                        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(adpia_account);
                        HttpClient httpClient = new HttpClient();
                        httpClient.DefaultRequestHeaders.Add("Authorization", System.Convert.ToBase64String(plainTextBytes));
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var content = JsonConvert.SerializeObject(body);
                        var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                        var responseInsert = await httpClient.PostAsync(_configuration["adpia:api_url"], stringContent);
                        var result = responseInsert.Content.ReadAsStringAsync().Result;
                        var result_content = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
                        if (responseInsert.StatusCode == System.Net.HttpStatusCode.OK && result_content["status"] != null && result_content["status"].ToString() == "200")
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS,
                                msg = "Push đơn hàng sang Adpia thành công. Response = " + result
                            });
                        }
                        else
                        {
                            LogHelper.InsertLogTelegram("api: Adpia-create-order.json ==> Push Order Failed, Result:  " + result + "\n With Order: " + content);
                            return Ok(new
                            {
                                status = (int)ResponseType.FAILED,
                                msg = "Push đơn hàng sang Adpia thất bại, Response = " + result
                            });
                        }
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Trạng thái đơn hàng không chính xác, vui lòng kiểm tra lại đơn hàng. Status = " + at_status
                        });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Token Invalid"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api: Adpia-create-order.json ==> error:  " + ex.Message);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Lỗi trong quá trình xử lý, vui lòng liên hệ bộ phận kỹ thuật"
                });
            }
        }

        /// <summary>
        /// endpoind cập nhật trạng thái đơn hàng đến adpia
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("adpia-update-status.json")]
        public async Task<ActionResult> PutOrderStatusToAdpia(string token)
        {
            JArray objParr = null;
            try
            {
                //string j_param = "{'order_id':16626,'order_status':13}";
                //token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);

                //Decode ra được dữ liệu
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    long order_id = Convert.ToInt64(objParr[0]["order_id"]);
                    long order_status = Convert.ToInt32(objParr[0]["order_status"]);
                    var order_detail = await _OrderRepository.GetOrderDetail(order_id);
                    var order_items = await _OrderRepository.GetOrderItemList(order_id);

                    var at_status = 0;
                    string order_rejected_reason = string.Empty;
                    switch (order_status)
                    {
                        case (int)OrderStatus.PAID_ORDER:
                        case (int)OrderStatus.SUCCEED_ORDER:
                            at_status = 1;
                            order_rejected_reason = "Đơn hàng thành công";
                            break;

                        case (int)OrderStatus.BUY_FAILED_ORDER:
                        case (int)OrderStatus.CANCEL_ORDER:
                            at_status = 2;
                            order_rejected_reason = ((OrderStatus)order_status).GetDisplayName();
                            break;
                        default:
                            at_status = 2;
                            order_rejected_reason = "Đơn hàng bị hủy";
                            break;
                    }
                    var body = new AdpiaOrderPostback()
                    {
                        order_code = order_detail.OrderNo,
                        order_value = Convert.ToInt32(order_detail.AmountVnd),
                        track_id = order_detail.TrackingId == null ? "QTEwMDAwMDEyMzoxMjMuMzIxLjIxMi4xMjoxMTEz" : order_detail.TrackingId,
                        items = new List<AdpiaOrderItem>(),
                    };
                    foreach (var orderitem in order_items)
                    {
                        if (order_items != null)
                        {
                            body.items.Add(new AdpiaOrderItem
                            {
                                pcd = orderitem.ProductCode,
                                pnm = orderitem.ProductName,
                                price = (float)orderitem.Price,
                                cnt = orderitem.Quantity,
                                ccd = orderitem.LabelId.ToString(), // edit
                            });
                        }
                    }
                    if (at_status == 1)
                    {
                        body.order_status = "approved";
                        if (body.items != null && body.items.Count > 0)
                            foreach (var postback_item in body.items)
                            {
                                if (postback_item != null)
                                {
                                    postback_item.status = "approved";
                                }

                            }
                    }
                    else if (at_status == 2)
                    {
                        body.order_status = "rejected";
                        body.reject_reason = order_rejected_reason;
                        if (body.items != null && body.items.Count > 0)
                            foreach (var postback_item in body.items)
                            {
                                if (postback_item != null)
                                {
                                    postback_item.status = "rejected";
                                    postback_item.reject_reason = order_rejected_reason;
                                }

                            }
                    }
                    else return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Trạng thái đơn hàng chưa xác định được thành công hay thất bại, chưa cần thực thi việc POST"
                    });

                    string adpia_account = _configuration["adpia:account"];
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(adpia_account);
                    HttpClient httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("Authorization", System.Convert.ToBase64String(plainTextBytes));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = JsonConvert.SerializeObject(body);
                    var stringContent = new StringContent(content, Encoding.UTF8, "application/json");
                    var responseInsert = await httpClient.PostAsync(_configuration["adpia:api_url"], stringContent);
                    var result = responseInsert.Content.ReadAsStringAsync().Result;
                    var result_content = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
                    if (responseInsert.StatusCode == System.Net.HttpStatusCode.OK && result_content["status"] != null && result_content["status"].ToString() == "200")
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Cập nhật trạng thái đơn hàng sang Adpia thành công. Response = " + result
                        });
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("api: Adpia-update-status.json ==> Update Order To AT failed. Result:  " + result + "\n With Order: " + content);
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Cập nhật trạng thái đơn hàng sang Adpia thất bại, Response = " + result
                        });
                    }

                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Token Invalid"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api:Adpia-update-status.json ==> error:  " + ex.Message);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Lỗi trong quá trình xử lý, vui lòng liên hệ bộ phận kỹ thuật"
                });
            }
        }

        [HttpPost("get-aff-by-client.json")]
        public async Task<IActionResult> GetAffLinkByClientID(string token)
        {
            JArray objParr = null;
            int status = (int)ResponseType.FAILED;
            string msg = "Dữ liệu gửi lên không chính xác";
            dynamic data=null;
            try
            {
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    long client_id = Convert.ToInt64(objParr[0]["client_id"]);
                    var client_affiliate = new ClientAffiliates(_configuration);
                    var listData = client_affiliate.GetAffliateURLByClient(client_id);
                    if (listData != null)
                    {
                        status = (int)ResponseType.SUCCESS;
                        msg = "Lấy danh sách Affiliate thành công";
                        data = listData;
                    }
                    else
                    {
                        msg = "Không có dữ liệu trả về";
                        data = listData;
                    }
                }
                else
                {

                }
                
            } catch(Exception ex)
            {
                LogHelper.InsertLogTelegram("api: get-aff-by-client.json ==> Get Aff Link of token:  " + token + " Error: \n " + ex.ToString());
                status = (int)ResponseType.FAILED;
                msg = "Lấy danh sách Affiliate Link thất bại . Vui lòng liên hệ bộ phận CSKH.";
            }
            return Ok(new
            {
                status = status,
                msg = msg,
                data = data
            });
        }
        [HttpPost("get-aff-url-by-id.json")]
        public async Task<IActionResult> GetAffLinkByID(string token)
        {
            JArray objParr = null;
            int status = (int)ResponseType.FAILED;
            string msg = "Dữ liệu gửi lên không chính xác";
            dynamic data = null;
            try
            {
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    int aff_id = Convert.ToInt32(objParr[0]["aff_id"]);
                    var client_affiliate = new ClientAffiliates(_configuration);
                    var listData = await client_affiliate.GetAffliateURLByIDAsync(aff_id);
                    if (listData != null)
                    {
                        status = (int)ResponseType.SUCCESS;
                        msg = "Lấy danh sách Affiliate thành công";
                        data = listData;
                    }
                    else
                    {
                        msg = "Không có dữ liệu trả về";
                        data = listData;
                    }
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api: get-aff-by-client.json ==> Get Aff Link of token:  " + token + " Error: \n " + ex.ToString());
                status = (int)ResponseType.FAILED;
                msg = "Lấy danh sách Affiliate Link thất bại . Vui lòng liên hệ bộ phận CSKH.";
            }
            return Ok(new
            {
                status = status,
                msg = msg,
                data = data
            });
        }
        [HttpPost("set-aff-by-client.json")]
        public async Task<IActionResult> PushAffLinkByClientID(string token)
        {
            JArray objParr = null;
            int status = (int)ResponseType.FAILED;
            string msg = "Dữ liệu gửi lên không chính xác";
            try
            {
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    long client_id = Convert.ToInt64(objParr[0]["client_id"].ToString());
                    DateTime create_date = DateTime.Parse(objParr[0]["create_date"].ToString());
                    string link_aff = objParr[0]["link_aff"].ToString();
                    MyAffiliateLinkViewModel client_aff = new MyAffiliateLinkViewModel()
                    {
                        client_id = client_id,
                        create_date = create_date,
                        link_aff = link_aff
                    };
                    var client_affiliate = new ClientAffiliates(_configuration);
                    if (client_aff == null || client_aff.client_id < 0 || client_aff.link_aff == null || client_aff.link_aff.Trim() == "")
                    {
                        
                    }
                    else
                    {
                        client_aff.link_aff = client_aff.link_aff.Trim();
                        var duplicate_item = await client_affiliate.CheckAffiliateURLExists(client_aff);
                        if (duplicate_item == null)
                        {
                            client_aff.GenID();
                            client_aff.create_date = DateTime.Now;
                            client_aff.update_time = DateTime.Now;
                            var data = await client_affiliate.PushAffliateURLAsync(client_aff);
                            if (data != null)
                            {
                                status = (int)ResponseType.SUCCESS;
                                msg = "Thêm Affiliate URL thành công";
                            }
                            else
                            {
                                status = (int)ResponseType.FAILED;
                                msg = "Thêm Affliate URL thất bại";
                            }
                        }
                        else if(duplicate_item._id.Trim()== "Error on Excution")
                        {
                            status = (int)ResponseType.ERROR;
                            msg = "Lỗi trong quá trình xử lý, vui lòng liên hệ với bộ phận CSKH";
                        }
                        else 
                        {
                            client_aff._id = duplicate_item._id;
                            client_aff.create_date = duplicate_item.create_date;
                            client_aff.update_time = DateTime.Now;
                            var data = await client_affiliate.UpdateAffliateURLAsync(client_aff);
                            status = (int)ResponseType.SUCCESS;
                            msg = "Affliate URL đã tồn tại, cập nhật thông tin thành công";
                        }
                       
                    }
                }
                else
                {

                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api: set-aff-by-client.json ==> Push Aff URL to DB:  " + token + " Error: \n " + ex.ToString());
                status = (int)ResponseType.FAILED;
                msg = "Thêm Affiliate URL thất bại . Vui lòng liên hệ bộ phận CSKH.";
            }
            return Ok(new
            {
                status = status,
                msg = msg,
            });
        }
        [HttpPost("get-aff-order.json")]
        public async Task<ActionResult> GetAffiliateOrders(string token)
        {
            JArray objParr = null;
            int status = (int)ResponseType.FAILED;
            string msg = "Dữ liệu gửi lên không chính xác";
            dynamic data = null;
            try
            {      
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    DateTime time_start = DateTime.Parse(objParr[0]["time_start"].ToString());
                    DateTime time_end = DateTime.Parse(objParr[0]["time_end"].ToString());
                    var utm_sources = JsonConvert.DeserializeObject<List<string>>(objParr[0]["utm_sources"].ToString());
                    data = await _OrderRepository.GetAffiliateOrderItems(time_start, time_end, utm_sources);
                    status = (int)ResponseType.SUCCESS;
                    msg = "Success";
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api: get-aff-order.json with " + token + " Error: \n " + ex.ToString());
                status = (int)ResponseType.ERROR;
                msg = "Error On Excution";
            }
            return Ok(new
            {
                status = status,
                msg = msg,
                data= data
            });

        }
    }
}
