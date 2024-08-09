using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Orders;
using Entities.ViewModels.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.API.Model.Order;
using WEB.API.ViewModels;
using static Utilities.Contants.Constants;
using static Utilities.Contants.OrderConstants;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IClientRepository _ClientRepository;
        private readonly IOrderItemRepository _OrderItemRepository;
        private readonly IOrderRepository _OrderRepository;
        private readonly IProductRepository _ProductRepository;
        private readonly IImageProductRepository _ImageProductRepository;
        private readonly IAllCodeRepository _AllCodeRepository;
        private readonly IPaymentRepository _PaymentRepository;
        public OrderController(IClientRepository clientRepository, IOrderItemRepository orderItemRepository,
            IOrderRepository orderRepository, IProductRepository productRepository,
            IImageProductRepository imageProductRepository, IAllCodeRepository allCodeRepository
            , IPaymentRepository paymentRepository, IConfiguration configuration)
        {
            _ClientRepository = clientRepository;
            _OrderItemRepository = orderItemRepository;
            _OrderRepository = orderRepository;
            _ProductRepository = productRepository;
            _ImageProductRepository = imageProductRepository;
            _AllCodeRepository = allCodeRepository;
            _PaymentRepository = paymentRepository;
            _configuration = configuration;
        }

        /// <summary>
        /// Api sẽ nhận dữ liệu từ FRONTEND OLD và CONSUMER "AppMappingOrder" sẽ push data từ db cũ về db mới
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("addNewOrder")]
        public async Task<ActionResult> addNewOrder(string token)
        {
            JArray objParr = null;
            long order_id = -1;
            try
            {
                #region TEST DATA
                //var order_model = new OrderViewModel
                //{
                //    ClientId = 79,
                //    UserId = -1,
                //    LabelId = (int)LabelType.amazon,
                //    OrderNo = "UAM-20072022",
                //    ClientName = "Đơn Test",
                //    Email = "tungvu@gmail.com",
                //    Phone = "0978793856",
                //    Address = "Phường Nhân Chính - Quận Thanh Xuân - Hà Nội",
                //    CreatedOn = DateTime.Now,
                //    RateCurrent = 23.5,
                //    PriceVnd = 100000,
                //    AmountVnd = 100000,
                //    TotalDiscount2ndVnd = 10200000,
                //    TotalShippingFeeVnd = 10020000,
                //    TotalDiscountVoucherVnd = 0,
                //    VoucherId = -1,
                //    Discount = 1,
                //    PriceUsd = 2,
                //    AmountUsd = 2,
                //    TotalDiscount2ndUsd = 20,
                //    TotalShippingFeeUsd = 4,
                //    Note = "Đơn test 1",
                //    PaymentType = 3,
                //    PaymentStatus = 3,
                //    PaymentDate = DateTime.Now,
                //    OrderStatus = 5,
                //    TrackingId = "10",
                //    UtmMedium = "cpc",
                //    UtmCampaign = "facebook1",
                //    UtmSource = "facebook1",
                //    UtmFirstTime = "usexpress1",
                //    AddressId=1212
                //};
                //var product_model = new List<ProductViewModel>()
                //{
                //   new ProductViewModel()
                //   {
                //        product_code = "B079GS4YQS",
                //        product_map_id = 23,
                //        product_name = "SP Test",
                //        price = 20.49,
                //        discount = 0,
                //        amount = 10,
                //        rating = "2000",
                //        manufacturer = "Energizer",
                //        label_id = (int)LabelType.amazon,
                //        reviews_count = 0,
                //        is_prime_eligible = true,
                //        rate = 25201,
                //        seller_id = "AMHD10D23",
                //        seller_name = "Amazon1",
                //        variations = "Variations",
                //        link_product=""
                //   },
                //    new ProductViewModel()
                //   {
                //        product_code = "B07F7PV92K",
                //        product_map_id = 24,
                //        product_name = "SP Test1",
                //        price = 21.99,
                //        discount = 0,
                //        amount = 10,
                //        rating = "2000",
                //        manufacturer = "Duracell",
                //        label_id = (int)LabelType.amazon,
                //        reviews_count =0,
                //        is_prime_eligible = true,
                //        rate = 25201,
                //        seller_id = "AMHD10D23",
                //        seller_name = "Amazon2",
                //        variations = "Variations"
                //   }
                //};
                //var order_item_model = new List<OrderItemViewModel>()
                //{
                //    new OrderItemViewModel()
                //    {
                //        ProductMapId = 23,
                //        OrderItemMapId = 830,
                //        Price = 2000,
                //        FirstPoundFee = 6,
                //        NextPoundFee = 8,
                //        ShippingFeeUs = 0,
                //        Quantity = 2,
                //    },
                //    new OrderItemViewModel()
                //    {
                //        ProductMapId = 24,
                //        Price = 1000,
                //        OrderItemMapId = 831,
                //        FirstPoundFee = 10,
                //        NextPoundFee = 12,
                //        ShippingFeeUs = 10,
                //        Quantity = 3,
                //    },
                //};
                //var image_product_model = new List<ImageProductViewModel>()
                //{
                //    new ImageProductViewModel()
                //    {
                //       Image = "https://images-na.ssl-images-amazon.com/images/I/81USLHbjxCL._AC_SX679_.jpg",
                //       ProductMapId = 23
                //    },
                //    new ImageProductViewModel()
                //    {
                //         Image = "https://m.media-amazon.com/images/I/911hpS-IrQL._AC_UL320_.jpg",
                //       ProductMapId = 24
                //    },
                //};
                //var list_note_model = new List<NoteModel>() {
                //    new NoteModel()
                //    {
                //        UserId=136,
                //        NoteMapId=136,
                //        CreateDate = DateTime.Now,
                //        //UpdateTime = DateTime.Now,
                //        Type =  (int)Constants.NoteType.ORDER,
                //        Comment = "Đơn hàng đang được chuyển về11111",
                //    },
                //    new NoteModel()
                //    {
                //        UserId=140,
                //        NoteMapId=140,
                //        CreateDate = DateTime.Now,
                //        //UpdateTime = DateTime.Now,
                //        OrderItemMapId = 831,
                //        Type =  (int)Constants.NoteType.ORDER_ITEM,
                //        Comment = "Đơn hàng đã được chuyển về VN2222",
                //    },
                //};
                //string j_param = "{'order_info': '" + JsonConvert.SerializeObject(order_model) +
                //    "','list_order_item_info': '" + JsonConvert.SerializeObject(order_item_model) +
                //      "','list_product_info': '" + JsonConvert.SerializeObject(product_model) +
                //      "','list_image_product': '" + JsonConvert.SerializeObject(image_product_model) +
                //      "','list_note_info': '" + JsonConvert.SerializeObject(list_note_model) +
                //      "'}";
                //token = CommonHelper.Encode(j_param, EncryptApi);
                #endregion

                if (!CommonHelper.GetParamWithKey(token, out objParr, EncryptApi))
                {
                    var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.EXISTS.ToString()},
                            {"msg", "Token invalid !!!"},
                            {"token",token},
                            {"order_id_response","-1"},
                        };
                    return Content(JsonConvert.SerializeObject(result));
                }
                else
                {
                    // Token hợp lệ                    
                    var order_info = JsonConvert.DeserializeObject<OrderViewModel>(objParr[0]["order_info"].ToString());
                    var list_order_item_info = JsonConvert.DeserializeObject<List<OrderItemViewModel>>(objParr[0]["list_order_item_info"].ToString());
                    var list_product_info = JsonConvert.DeserializeObject<List<ProductViewModel>>(objParr[0]["list_product_info"].ToString());
                    var list_image_product = new List<ImageProductViewModel>();



                    if (!string.IsNullOrEmpty(objParr[0]["list_image_product"].ToString()))
                    {
                        list_image_product = JsonConvert.DeserializeObject<List<ImageProductViewModel>>(objParr[0]["list_image_product"].ToString());
                    }
                    var list_note_order = new List<NoteModel>();
                    if (objParr[0]["list_note_info"] != null)
                    {
                        if (!string.IsNullOrEmpty(objParr[0]["list_note_info"].ToString()))
                        {
                            list_note_order = JsonConvert.DeserializeObject<List<NoteModel>>(objParr[0]["list_note_info"].ToString());
                        }
                    }

                    var listProductId = new Dictionary<int?, long>();
                    foreach (var item in list_product_info)
                    {
                        long product_id = 0;
                        if (!string.IsNullOrEmpty(item.variations))
                        {
                            item.variations = CommonHelper.Decode(item.variations, EncryptApi);
                        }
                        //var productInfo = await _ProductRepository.GetByProductCode(item.product_code, item.label_id);
                        product_id = await _ProductRepository.Create(item);
                        //if (productInfo == null)
                        //    product_id = await _ProductRepository.Create(item);
                        //else
                        //    product_id = productInfo.Id;

                        if (product_id == 0)
                        {
                            var result = new Dictionary<string, string>
                                {
                                    {"status",ResponseType.FAILED.ToString()},
                                    {"msg","khong tao duoc Product."},
                                    {"token",token},
                                    {"product_info",JsonConvert.SerializeObject(item).Replace("\"","'")},
                                };
                            return Content(JsonConvert.SerializeObject(result));
                        }
                        var exists = listProductId.FirstOrDefault(n => n.Value == product_id);
                        if (exists.Key == null)
                            listProductId.Add(item.product_map_id, product_id);
                    }
                    foreach (var item in list_image_product)
                    {
                        var productId = listProductId.FirstOrDefault(n => n.Key == item.ProductMapId);
                        item.ProductId = productId.Value;
                        await _ImageProductRepository.Create(item);
                    }
                    var clientMapId = (int)order_info.ClientId;
                    var client = await _ClientRepository.getClientByClientMapId(clientMapId);
                    order_info.ClientId = client != null ? client.ClientId : order_info.ClientId;

                    var listOrderItem = new List<OrderItemViewModel>();
                    foreach (var item in list_order_item_info)
                    {
                        if (list_image_product != null && list_image_product.Count > 0)
                        {
                            var image = list_image_product.FirstOrDefault(n => n.ProductMapId == item.ProductMapId);
                            item.ProductImage = image != null ? image.Image : "";
                        }
                        var productId = listProductId.FirstOrDefault(n => n.Key == item.ProductMapId);
                        item.ProductId = productId.Value;
                        listOrderItem.Add(item);
                    }

                    order_id = await _OrderRepository.CreateOrder(order_info, listOrderItem, list_note_order);

                    if (order_id > 0)
                    {
                        var list_product_data = list_product_info.GroupJoin(list_order_item_info,
                        product => product.product_map_id,
                        order_item => order_item.ProductMapId,
                        (product, order_item) => new ESModifyProductModel
                        {
                            product_code = product.product_code,
                            label_id = product.label_id,
                            product_bought_quantity = order_item.FirstOrDefault() != null ? order_item.FirstOrDefault().Quantity : 0
                        });

                        await _ProductRepository.UpdateProductQuantityAndGroupOnElastic(list_product_data);

                        var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.SUCCESS.ToString()},
                            {"msg","Add new success !"},
                            {"token",token},
                            {"order_id_response",order_id.ToString()},
                        };

                        return Content(JsonConvert.SerializeObject(result));
                    }
                    else
                    {
                        var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.FAILED.ToString()},
                            {"msg", "Add new fail !"},
                            {"token",token},
                            {"order_id_response",order_id.ToString()},
                        };

                        return Content(JsonConvert.SerializeObject(result));
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("addNewOrder - OrderController " + ex
                    + " order_id=" + order_id.ToString());
                var result = new Dictionary<string, string>
                        {
                            {"status",ResponseType.ERROR.ToString()},
                            {"msg", ex.Message},
                            {"token",token},
                            {"order_id_response","-1"},
                        };

                return Content(JsonConvert.SerializeObject(result));
            }
        }

        /// <summary>
        /// api lấy chi tiết đơn hàng bao gồm thông tin đơn hàng và
        /// chi tiết các sp của đơn hàng
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("getOrderDetail.json")]
        public async Task<ActionResult> getOrderDetail(string token)
        {
            JArray objParr = null;
            //string j_param = "{'orderNo':'UAM-0A01016'}";
            //token = CommonHelper.Encode(j_param, EncryptApi);
            try
            {
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    string orderNo = (String)(objParr[0]["orderNo"]);
                    var orderInfo = await _OrderRepository.GetOrderDetailByContractNo(orderNo);
                    if (orderInfo == null)
                    {
                        return Ok(new
                        {
                            status = ResponseType.EXISTS.ToString(),
                            order_detail = new OrderViewModel(),
                            order_item_list = new List<OrderItemViewModel>(),
                            msg = "Note exists !!!"
                        });
                    }
                    var orderItemList = await _OrderRepository.GetOrderItemList(orderInfo.Id);

                    return Ok(new
                    {
                        status = ResponseType.SUCCESS.ToString(),
                        order_detail = orderInfo,
                        order_item_list = orderItemList,
                        msg = "Successfully !!!"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = ResponseType.FAILED.ToString(),
                        token = token,
                        msg = "token valid !!!!"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getOrderDetail - OrderController " + ex.ToString());
                return Ok(new
                {
                    status = ResponseType.ERROR.ToString(),
                    token = token,
                    msg = "Exception !!!"
                });
            }
        }

        /// <summary>
        /// RULE:
        /// // Mô tả
        // VD: UAM‌-8B17666
        // U: Hàng usexpress - Đảm bảo tiêu chí 5
        // AM: Amazon, CC: Costco, BB: Bestbuy… Đảm bảo Tiêu chí 1
        // 8: Năm 2018 (9: 2019, 0: 2020,…, 7:2027, A: 2028, B:2029,…, Z: 2053)
        // B: Tháng 02 (A: Jan, B: Feb, C: Mar,…, H: Aug, K: Sep, L:Oct, M: Nov, N: Dec)
        // 17: Ngày 17
        // 666: Thứ tự của đơn hàng trong ngày, bắt đầu từ 170 (ngày + 0), không phân biệt Amazon hay Costco, cứ lấy thứ tự theo đây). Bắt đầu Ngày + 0 (VD trên là 170) đảo bảo Tiêu chí 4. 
        // Diễn giải = ngày + "0" + số lượng đơn hàng đã tạo trong ngày + 1
        // Độ dài 11 kí tự, và cố định 11 - Đảm bảo tiêu chí 2, 3
        // Khách gọi vào chỉ cần đọc 5 số cuối: 17666 - CSKH sẽ biết là đơn hàng 666 ngày 17 (chỉ cần biết ngày ko cần tháng vì các đơn thường không kéo dài đến 1 tháng nên ko bị nhầm lẫn). Đảm bảo tiêu chí 7.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("create-code.json")]
        public async Task<ActionResult> getOrderNoByDate(string token)
        {
            JArray objParr = null;
            //string j_param = "{'label_id':'1'}";

            //token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);
            try
            {
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    int label_id = (int)(objParr[0]["label_id"]);
                    var _order_no = await _OrderRepository.BuildOrderNo(label_id);

                    return Ok(new
                    {
                        status = ResponseType.SUCCESS.ToString(),
                        order_no = _order_no
                    });
                }
                else
                {
                    LogHelper.InsertLogTelegram("[API]getOrderNoByDate - OrderController: Token not Valid token =" + token);
                    return Ok(new
                    {
                        status = ResponseType.FAILED.ToString(),
                        token = token,
                        msg = "token valid !!!!"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getOrderDetail - OrderController token =" + token + " ex = " + ex);
                return null;
            }
        }

        /// <summary>
        /// api insert payment log
        /// </summary>
        /// <param name="token"></param>
        /// <createDate>8-11-2020</createDate>
        /// <author>ThangNV</author>
        /// <returns>json: status, msg,</returns>
        [HttpPost("add-payment-log.json")]
        public async Task<IActionResult> addPaymentLog(string token)
        {
            try
            {
                var paymentLogViewModel = new PaymentLogViewModel()
                {
                    response_data = "Thanh toán chuyển khoản trực tiếp thành công",
                    log_date = DateTime.Now,
                    payment_type = 2,
                    order_no = "UAM-0M08082",
                };
                //string j_param = "{'payment_log':'" + Newtonsoft.Json.JsonConvert.SerializeObject(paymentLogViewModel) + "'}";
                //token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    var productFavoriteModel = Newtonsoft.Json.JsonConvert.DeserializeObject
                        <PaymentLogViewModel>(objParr[0]["payment_log"].ToString());
                    var paymentLog = new PaymentLog(_configuration);
                    var product_result = await paymentLog.addNew(productFavoriteModel);
                    if (product_result != null)
                    {
                        return Ok(new
                        {
                            status = ResponseType.SUCCESS.ToString(),
                            msg = "Add payment log success"
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = ResponseType.FAILED.ToString(),
                            msg = "Add payment log fail. API/OrderController"
                        });
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("add-payment-log.json -" +
                        " API/OrderController: token valid !!! token =" + token);
                    return Ok(new
                    {
                        status = ResponseType.EXISTS.ToString(),
                        _token = token,
                        msg = "token invalid !!!"
                    });
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("add-payment-log.json " +
                    "- API/OrderController " + ex.Message + " token=" + token.ToString());
                return Ok(new
                {
                    status = ResponseType.ERROR.ToString(),
                    msg = ex.ToString(),
                    token = token
                });
            }
        }

        /// <summary>
        /// api cập nhật trạng thái thanh toán khi đối tác Payoo push về và cập nhật trạng thái thanh toán cho đơn hàng
        /// </summary>
        /// <param name="token">chuỗi json chứa tham số đầu vào - order_no</param>
        /// <returns></returns>
        /// <author>ThangNV</author>
        /// <createDate>9-11-2010</createDate>
        [HttpPost("payment-confirm.json")]
        public async Task<ActionResult> paymentConfirm(string token)
        {
            JArray objParr = null;
            // string j_param = "{'order_no':'TEST-UAM-2M09092'}";
            //token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);
            try
            {
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    var _order_no = objParr[0]["order_no"] == null ? string.Empty : objParr[0]["order_no"].ToString();

                    if (_order_no.Length <= 5) return Ok(new { status = ResponseType.FAILED.ToString(), order_no = _order_no, msg = "Đơn hàng không hợp lệ" });

                    var orderViewModel = await _OrderRepository.GetOrderDetailByContractNo(_order_no);
                    if (orderViewModel != null)
                    {
                        if (orderViewModel.LabelId == (int)LabelType.costco)
                        {
                            orderViewModel.OrderStatus = (int)Constants.OrderStatus.BOUGHT_ORDER;
                        }
                        else
                        {
                            orderViewModel.OrderStatus = (int)Constants.OrderStatus.PAID_ORDER;
                        }
                        orderViewModel.PaymentStatus = (int)Payment_Status.DA_THANH_TOAN;//chuyển sang trạng thái đã thanh toán
                        
                        var result = await _OrderRepository.Update(orderViewModel);
                        if (result <= 0)
                        {
                            LogHelper.InsertLogTelegram("[API-NEW] paymentConfirm - OrderController: Cập nhật trạng thái thanh toán thất bại token =" + token);
                            return Ok(new { status = ResponseType.FAILED.ToString(), order_no = _order_no, msg = "Cập nhật thất bại" });
                        }
                        else
                        {
                            return Ok(new { status = ResponseType.SUCCESS.ToString(), order_no = _order_no, msg = "[" + _order_no + "] Cập nhật trạng thái thanh toán thành công!" });
                        }
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("[API-NEW] paymentConfirm - OrderController: Đơn hàng không tồn tại trên hệ thống token =" + token);
                        return Ok(new { status = ResponseType.EXISTS.ToString(), order_no = _order_no, msg = "Đơn hàng không tồn tại trên hệ thống" });
                    }
                }
                else
                {
                    LogHelper.InsertLogTelegram("[API-NEW] paymentConfirm - OrderController: Token not Valid token =" + token);
                    return Ok(new { status = ResponseType.FAILED.ToString(), msg = "token valid !!!!" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[API-NEW] paymentConfirm - OrderController error: token =" + token + " ex = " + ex);
                return Ok(new { status = ResponseType.ERROR.ToString(), msg = "Lỗi xảy ra !!!!" + ex.ToString() });
            }
        }

        [HttpPost("add-order-log-activity.json")]
        public async Task<IActionResult> addOrderLogActivity(string token)
        {
            try
            {
                var orderLogActivityViewModel = new OrderLogActivityViewModel()
                {
                    order_no = "UAM-8N19194",
                    amount = 382865,
                    create_date = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                    email_client = "luongquynh117021@gmail.com",
                    email_user = "admin@usexpress.com.vn",
                    payment_type = 2,
                    status = 16
                };
                //string j_param = "{'order_log_activity':'" + Newtonsoft.Json.JsonConvert.SerializeObject(orderLogActivityViewModel) + "'}";
                //token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    var OrderLogActivity = new OrderLogActivity(_configuration);
                    var orderLogActivityModel = Newtonsoft.Json.JsonConvert.DeserializeObject
                        <OrderLogActivityViewModel>(objParr[0]["order_log_activity"].ToString());
                    var rs = await OrderLogActivity.addNew(orderLogActivityModel);
                    if (rs != null)
                    {
                        return Ok(new
                        {
                            status = ResponseType.SUCCESS.ToString(),
                            msg = "Add log activity sucess"
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = ResponseType.FAILED.ToString(),
                            msg = "Add log activity sucess fail. API/addOrderLogActivity"
                        });
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("add-order-log-activity.json -" +
                        " API/OrderController: token valid !!! token =" + token);
                    return Ok(new
                    {
                        status = ResponseType.EXISTS.ToString(),
                        _token = token,
                        msg = "token invalid !!!"
                    });
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("add-order-log-activity.json " +
                    "- API/OrderController " + ex.Message + " token=" + token.ToString());
                return Ok(new
                {
                    status = ResponseType.ERROR.ToString(),
                    msg = ex.ToString(),
                    token = token
                });
            }
        }

        [HttpPost("get-token.json")]
        public async Task<IActionResult> getToken(OrderLogActivityViewModel orderLogActivityViewModel)
        {
            try
            {
                orderLogActivityViewModel.create_date = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                string j_param = "{'order_log_activity':'" + Newtonsoft.Json.JsonConvert.SerializeObject(orderLogActivityViewModel) + "'}";
                string token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);
                return Ok(new
                {
                    status = ResponseType.SUCCESS.ToString(),
                    msg = "Add log activity sucess",
                    token = token,
                });
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("add-order-log-activity.json " +
                    "- API/OrderController " + ex.Message + " token=" + JsonConvert.SerializeObject(orderLogActivityViewModel));
                return Ok(new
                {
                    status = ResponseType.ERROR.ToString(),
                    msg = ex.ToString(),
                    token = ""
                });
            }
        }

        /// <summary>
        /// Minh
        /// API trả List đơn hàng để hiển thị trên trang quản lý đơn hàng của user
        /// Input: Encode (mã khách hàng, số sản phẩm trên mỗi trang, trang, từ khoá tìm kiếm)
        /// Output: Trả list "items_per_page" sản phẩm mỗi trang theo từ khoá tìm kiếm, nếu để trống từ khoá tìm kiếm thì trả về tất cả kết quả.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-fe-order-list.json")]
        public ActionResult getClientOrderListPagnition(string token)
        {
            JArray objParr = null;
            try
            {
                //Decode ra được dữ liệu
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    if (objParr[0]["client_id"].ToString() == "" || objParr[0]["order_status"].ToString() == "" || objParr[0]["page_size"].ToString() == "" || objParr[0]["current_page"].ToString() == "" || objParr[0]["client_id"].ToString() == null || objParr[0]["order_status"].ToString() == null || objParr[0]["page_size"].ToString() == null || objParr[0]["current_page"].ToString() == null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            data_list = "",
                            total_record = 0,
                            msg = "Input Data is Null"
                        });
                    }
                    //Lấy dữ liệu:
                    int client_id = Convert.ToInt32(objParr[0]["client_id"]);
                    string input_search = objParr[0]["input_search"].ToString();
                    int order_status = Convert.ToInt32(objParr[0]["order_status"]);
                    int current_page = Convert.ToInt32(objParr[0]["current_page"]);
                    int page_size = Convert.ToInt32(objParr[0]["page_size"]);

                    //Lấy thông tin từ DB
                    var orderGridModels = _OrderRepository.GetOrderListFEByClientId(client_id, input_search, order_status, current_page, page_size);

                    //-- Trả kết quả
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data_list = orderGridModels,
                        msg = "Successfully !!!"
                    });
                }
                //Nếu sai token: 
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        data_list = "",
                        msg = "Token invalid !"
                    });
                }
            }
            catch (Exception ex)
            {
                //Trả log lỗi:
                LogHelper.InsertLogTelegram("api: order/get-fe-order-list.json ==> error:  " + ex.Message);

                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    data_list = "",
                    msg = "Error on Excution !!!" + ex.ToString()
                });
            }
        }

        /// <summary>
        /// Minh
        /// API trả thông tin chi tiết đơn hàng để hiển thị trên Frontend
        /// Input: Encode (mã Order)
        /// Output: Trả thông tin chi tiết đơn hàng
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-fe-order-detail.json")]
        public ActionResult getClientOrderDetailFE(string token)
        {
            JArray objParr = null;
            try
            {
                //string j_param = "{'order_id':'16102'}";
                //token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);

                //Decode ra được dữ liệu
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {

                    //Lấy dữ liệu:
                    int order_id = Convert.ToInt32(objParr[0]["order_id"]);
                    int ClientId = Convert.ToInt32(objParr[0]["client_id"]);

                    //Lấy thông tin từ DB
                    var order_detail_obj = _OrderRepository.GetOrderDetailFEByID(order_id);
                    string j = JsonConvert.SerializeObject(order_detail_obj);
                    var detail = JsonConvert.DeserializeObject<FEOrderDetailViewModel>(j);
                    if (detail.ClientId != ClientId)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "Token invalid !",
                            token = token
                        });
                    }
                    //-- Format date
                    DateTime d = (DateTime)detail.createdOn;
                    detail.createdOn = d.ToString("dd/MM/yyyy HH:mm");
                    //-- Add price before discount:
                    detail.priceVnd = Convert.ToDecimal(detail.amountVnd + detail.totalDiscount2ndVnd + detail.totalDiscountVoucherVnd).ToString("N0");
                    //-- Format Price
                    double p = detail.amountVnd;
                    detail.amountVnd = Convert.ToDecimal(p).ToString("N0");
                    //Merge Discount property:
                    double? discount_price = detail.totalDiscount2ndVnd + detail.totalDiscountVoucherVnd;
                    detail.TotalDiscount = Convert.ToDecimal(discount_price).ToString("N0");
                    detail.totalDiscount2ndVnd = null;
                    detail.totalDiscountVoucherVnd = null;
                    //-- Add URL:
                    foreach (var record in detail.productList)
                    {
                        //-- AmoutVND Calucate: [(price) + (first pound fee) + (next pound fee) + (luxury fee)] * (current rate);
                        record.amoutVnd = Convert.ToDecimal((record.price + (record.firstPoundFee > 0 ? record.firstPoundFee : 0) + (record.nextPoundFee > 0 ? record.nextPoundFee : 0) + (record.luxuryFee > 0 ? record.luxuryFee : 0)) * detail.rateCurrent).ToString("#,##0.00");
                        record.amoutVnd = null;
                        //-- Turn of Fee Property
                        record.firstPoundFee = null;
                        record.nextPoundFee = null;
                        record.luxuryFee = null;
                        record.weight = null;
                        record.price = Convert.ToDecimal((double)record.price).ToString("N0");
                    }
                    //Trả kết quả:
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        order_detail = detail,
                        msg = "Successfully !!!"
                    });
                }
                //Nếu sai token: 
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Token invalid !",
                        token = token
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api: order/get-fe-order-detail.json ==> error:  " + ex.Message);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Error on Excution !!!"
                });
            }
        }

        /// <summary>
        /// Minh
        /// API trả thông tin chi tiết đơn hàng để hiển thị trên Frontend
        /// Input: Encode (mã Order)
        /// Output: Trả thông tin chi tiết đơn hàng
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-fe-lastest-order.json")]
        public ActionResult GetLastestRecordByClientID(string token)
        {
            JArray objParr = null;
            try
            {
                // string j_param = "{'client_id':'17367'}";
                // token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);

                //Decode ra được dữ liệu
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    //Lấy dữ liệu:
                    int ClientId = Convert.ToInt32(objParr[0]["client_id"]);
                    //Lấy thông tin từ DB
                    var oder_detail_obj = _OrderRepository.GetFELastestRecordByClientID(ClientId);
                    //Trả kết quả
                    if (oder_detail_obj == null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.EMPTY,
                            msg = "Client not found !!!"
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = oder_detail_obj == null ? (int)ResponseType.SUCCESS : (int)ResponseType.SUCCESS,
                            order_detail = oder_detail_obj,
                            msg = "Successfully !!!"
                        });
                    }
                }
                //Nếu sai token: 
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        msg = "Token invalid !"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api: order/get-fe-lastest-order.json ==> error:  " + ex.Message);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Error on Excution !!!"
                });
            }
        }

        /// <summary>
        /// Minh
        /// API trả số lượng đơn hàng mà client đã tạo
        /// Input: Encode (mã khách hàng)
        /// Output: Trả ra số đơn hàng theo các mục: tất cả, đã hoàn thành, đã thanh toán, đã khởi tạo
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-fe-order-count.json")]
        public ActionResult GetOrderCountByClientID(string token)
        {
            JArray objParr = null;
            try
            {
                //Decode ra được dữ liệu
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    //Lấy dữ liệu:
                    int ClientId = Convert.ToInt32(objParr[0]["client_id"]);
                    //Lấy thông tin từ DB
                    var order_detail_obj = _OrderRepository.GetFEOrderCountByClientID(ClientId);
                    string j = JsonConvert.SerializeObject(order_detail_obj);
                    Dictionary<string, int> dictionary = JsonConvert.DeserializeObject<Dictionary<string, int>>(j);
                    List<OrderTabModel> result = new List<OrderTabModel>();
                    result.Add(new OrderTabModel()
                    {
                        tab_index = (int)GroupStatusOrderType.all_orders,
                        tab_name = "Tất cả sản phẩm",
                        order_count = dictionary["AllOders"]
                    });
                    result.Add(new OrderTabModel()
                    {
                        tab_index = (int)GroupStatusOrderType.wait_for_payment_count,
                        tab_name = "Chờ thanh toán",
                        order_count = dictionary["WaitForPaymentCount"]
                    });
                    result.Add(new OrderTabModel()
                    {
                        tab_index = (int)GroupStatusOrderType.wait_to_receive_count,
                        tab_name = "Chờ nhận hàng",
                        order_count = dictionary["WaitToReceiveCount"]
                    });

                    result.Add(new OrderTabModel()
                    {
                        tab_index = (int)GroupStatusOrderType.received_order_count,
                        tab_name = "Đã hoàn thành",
                        order_count = dictionary["ReceivedOrderCount"]
                    });
                    /*
                    result.Add(new OrderTabModel()
                    {
                        tab_index = (int)GroupStatusOrderType.failed_order_count,
                        tab_name = "Đơn hàng thất bại",
                        order_count = dictionary["FailedOrderCount"]
                    });*/
                    //Trả kết quả
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        order_count = result,
                        msg = "Successfully !!!"
                    });
                }
                //Nếu sai token: 
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.FAILED,
                        order_count = "",
                        msg = "Token invalid !"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api: order/get-fe-order-count.json ==> error:  " + ex.Message);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    order_count = "",
                    msg = "Error on Excution !!!"
                });
            }
        }

        /// <summary>
        /// Trả ra detail đơn us news
        /// Bot đẩy sang us old
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-order-detail.json")]
        public async Task<ActionResult> GetOrderDetailForApi(string token)
        {
            JArray objParr = null;
            try
            {
                // string j_param = "{'order_id':'16620'}";
                // token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);

                //Decode ra được dữ liệu
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    int order_id = Convert.ToInt32(objParr[0]["order_id"]);
                    var detail = await _OrderRepository.GetOrderDetailForApi(order_id);
                    if (detail != null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            data = detail
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.EMPTY,
                            msg = "empty"
                        });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "error"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api: order/get-order-detail.json ==> error:  " + ex.Message);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    order_count = "",
                    msg = ex.ToString()
                });
            }
        }
        /// <summary>
        /// Trả ra ds sp trong 1 đơn hàng từ us news
        /// Bot đẩy sang us old
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-order-item-detail.json")]
        public async Task<ActionResult> GetOrderItemDetailForApi(string token)
        {
            JArray objParr = null;
            try
            {
                // string j_param = "{'order_id':'16620'}";
                // token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);

                //Decode ra được dữ liệu
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    int order_id = Convert.ToInt32(objParr[0]["order_id"]);
                    var product_list = await _OrderRepository.GetOrderItemList(order_id);
                    if (product_list != null)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            data = product_list
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.EMPTY,
                            msg = "empty"
                        });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "error"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api: order/get-order-item-detail.json ==> error:  " + ex.Message);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = ex.ToString()
                });
            }
        }

        /// <summary>        /// 
        /// endpoind này dùng để mapping lại orderid old về us news
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("mapping-order-id-old.json")]
        public async Task<ActionResult> mappingOrderIdOld(string token)
        {
            JArray objParr = null;
            try
            {
                //string j_param = "{'order_id_new':16626,'order_id_old':14771}";
                //token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);

                //Decode ra được dữ liệu
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    long order_id_new = Convert.ToInt64(objParr[0]["order_id_new"]);
                    long order_id_old = Convert.ToInt64(objParr[0]["order_id_old"]);
                    var id = await _OrderRepository.UpdateOrderMapId(order_id_new, order_id_old);
                    if (id > 0)
                    {
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
                            msg = "FAILED"
                        });
                    }
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "error"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api: order/mapping-order-id-old.json ==> error:  " + ex.Message);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    order_count = "",
                    msg = ex.ToString()
                });
            }
        }

        /// <summary>
        /// update don hang thanh toan lai
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("update-order-re-checkout.json")]
        public async Task<ActionResult> ReCheckOut(string token)
        {
            JArray objParr = null;
            try
            {
                // string j_param = "{'order_id':16830,'address_id':42563,'pay_type':5}";
                // token = CommonHelper.Encode(j_param, _configuration["KEY_TOKEN_API"]);

                //Decode ra được dữ liệu
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    long order_id = Convert.ToInt64(objParr[0]["order_id"]);
                    int address_id = Convert.ToInt32(objParr[0]["address_id"]);
                    short pay_type = Convert.ToInt16(objParr[0]["pay_type"]);

                    if (order_id > 0 && pay_type > 0)
                    {

                        var response = await _OrderRepository.UpdatePaymentReCheckOut(order_id, address_id, pay_type);
                        if (response)
                        {
                            return Ok(new
                            {
                                status = (int)ResponseType.SUCCESS
                            });
                        }
                        else
                        {
                            LogHelper.InsertLogTelegram("api: order/update-order-re-checkout.json ==> token:  " + token);
                            return Ok(new
                            {
                                status = (int)ResponseType.FAILED,
                                msg = "FAILED with token:" + token
                            });
                        }
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("api: order/update-order-re-checkout.json: token co param = 0 ==> token:  " + token);
                        return Ok(new
                        {
                            status = (int)ResponseType.FAILED,
                            msg = "FAILED with token:" + token
                        });
                    }
                }
                else
                {
                    LogHelper.InsertLogTelegram("api:ERROR  order/update-order-re-checkout.json ==> token:  " + token);
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "error"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api: order/update-order-re-checkout.json ==> error:  " + ex.Message);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = ex.ToString()
                });
            }
        }


    }
}