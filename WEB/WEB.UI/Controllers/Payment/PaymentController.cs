using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Caching.RedisWorker;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Payment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Repositories.Repositories;
using Utilities.Contants;
using WEB.UI.Controllers.Carts;
using WEB.UI.Controllers.Order;
using WEB.UI.Controllers.Payment.Payoo;
using WEB.UI.Controllers.Payment.Service;
using WEB.UI.Controllers.Voucher.Base;
using WEB.UI.FilterAttribute;
using WEB.UI.ViewModels;
using OrderDetailViewModel = WEB.UI.ViewModels.OrderDetailViewModel;

namespace WEB.UI.Controllers.Payment
{
    [Route("[controller]")]
    public class PaymentController : Controller
    {
        private readonly IClientRepository clientRepository;
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public PaymentController(IConfiguration _Configuration, RedisConn _redisService, IClientRepository _clientRepository)
        {
            configuration = _Configuration;
            redisService = _redisService;
            clientRepository = _clientRepository;
        }

        [AjaxAuthorize()]
        [HttpGet("checkout")]
        public async Task<IActionResult> Checkout()
        {
            //Check cart có được chọn không ?
            var cart = new ShoppingCarts(configuration);
            string cart_id = cart.GetCartId(this.HttpContext);
            //get cartslist
            var cart_model = await cart.getCartListByUser(cart_id, -1, "", -1);
            if (cart_model != null)
            {
                if (cart_model.Sum(x => x.total_selected_carts) == 0)
                {
                    return Redirect("/Carts/view.html");
                }
            }
            else
            {
                return Redirect("/Carts/view.html");
            }
            ViewBag.orderId = -1;
            ViewBag.addressId = -1;
            return View();
        }

        /// <summary>
        /// Thanh toán lại
        /// </summary>
        /// <returns></returns>
        [AjaxAuthorize()]
        [HttpGet("re-checkout/{order_id}")]
        public async Task<IActionResult> ReCheckout(long order_id)
        {
            try
            {
                var order_service = new OrderService(configuration);
                var cart = new ShoppingCarts(configuration);
                long client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);

                var order_result = await order_service.getOrderDetail(order_id, client_id);
                var order_detail = JsonConvert.DeserializeObject<OrderDetailViewModel>(order_result);

                var order = await cart.getCartListByOrderId(order_detail);
                if (order == null)
                {
                    return Redirect("/Error/404");
                }
                ViewBag.orderId = order_id;
                ViewBag.order = order;
                ViewBag.addressId = order_detail.addressId;
                return View("/Views/Payment/Checkout.cshtml");
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("update order with usexpress NEW error [order_id = " + order_id + " ] ex =" + ex.ToString());
                return Redirect("/Error/404");
            }
        }

        [AjaxAuthorize()]
        [HttpPost("banks-list.json")]
        public async Task<IActionResult> getBankListAtm()
        {
            try
            {
                string cache_key = CacheType.BANK_LIST_PAYOO;
                var bank_list = new List<PayooBankViewModel>();
                var j_banks = await redisService.GetAsync(cache_key, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                if (!string.IsNullOrEmpty(j_banks))
                {
                    bank_list = JsonConvert.DeserializeObject<List<PayooBankViewModel>>(j_banks);
                }
                else
                {
                    var payoo = new PayooService(configuration);
                    bank_list = await payoo.getBankList();
                    redisService.Set(cache_key, JsonConvert.SerializeObject(bank_list), DateTime.Now.AddDays(1), Convert.ToInt32(configuration["Redis:Database:db_common"]));
                }

                return Ok(new { status = bank_list == null ? ResponseType.EMPTY : ResponseType.SUCCESS, bank_data = bank_list });
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("getBankListAtm !!! exx =" + ex.ToString());
                return Ok(new { status = ResponseType.FAILED.ToString() });
            }
        }

        /// <summary>
        /// step 1: Push order new qua api        
        /// step 2: Push queue để đồng bộ với us old
        /// step3: Render Payoo --> Redirect to Payoo
        /// </summary>
        /// <param name="address_id"></param>
        /// <param name="pay_type"></param>
        /// <param name="bank_code"></param>
        /// <returns></returns>
        [AjaxAuthorize()]
        [ValidationOrder(new[] { "address_id", "pay_type", "bank_code" })]
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder(int label_id, long address_id, int pay_type, string bank_code, string voucher_choice, bool is_force_pay, string affiliate = "")
        {
            string _url_payoo_redirect = string.Empty;
            // string KEY_TOKEN_API = configuration["KEY_TOKEN_API"];
            try
            {
                string url_api = configuration["url_api_usexpress_new"]; //+ "api/order/addNewOrder";
                string url_api_voucher = configuration["url_api_usexpress_new"] + "api/voucher/apply.json";
                string token_tele = configuration["telegram_log_error_fe:Token"];
                string group_id_tele = configuration["telegram_log_error_fe:GroupId"];
                string key_token_api = configuration["KEY_TOKEN_API"];

                #region step 1: Create Order New By api
                var client_detail = await clientRepository.GetAddressReceiverByAddressId(address_id);
                string email_client = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "EMAILID").Value.ToString();
                long client_id = client_detail[0].ClientId;

                // carts list by client
                var cart = new ShoppingCarts(configuration);
                var cart_detail = await cart.getCartListByUser(email_client, label_id, "", 1);

                if (cart_detail == null || cart_detail.Count() <= 0)
                {
                    Utilities.LogHelper.InsertLogTelegram("[FR-NEW: action: payment/create] email:" + email_client + " đã back lại trang thanh toán");
                    return Ok(new { status = (int)ResponseType.EMPTY });
                }
                else
                {

                    #region validation Voucher                      
                    var voucher_expire = new List<VoucherEntitiesViewModel>(); // voucher hợp lệ

                    if (!string.IsNullOrEmpty(voucher_choice))
                    {
                        var voucher_service = new VoucherService(url_api_voucher, email_client, voucher_choice, token_tele, group_id_tele, label_id, key_token_api);
                        var vc_check = await voucher_service.getVoucherList();
                        // Trường hợp vc hết hạn thông báo vào hỏi is force payment
                        voucher_expire = vc_check.Where(x => x.status != (int)ResponseType.SUCCESS).ToList();
                        if (!is_force_pay) // check xem có cho phép bỏ qua voucher không hợp lệ hay không
                        {
                            if (voucher_expire.Count() > 0) // kiểm tra tính hợp lệ của voucher
                            {
                                return Ok(new { status = (int)ResponseType.CONFIRM, voucher_name = string.Join(", ", voucher_expire.Select(x => x.voucher_name)), is_force_pay = true });
                            }
                        }
                        // vc hợp lệ
                        voucher_expire = vc_check.Where(x => x.status == (int)ResponseType.SUCCESS).ToList();
                    }
                    #endregion

                    var payment = new PaymentConnection(configuration, url_api, email_client, label_id, token_tele, group_id_tele, client_id);

                    // check aff
                    var obj_aff = string.IsNullOrEmpty(affiliate) ? null : await payment.ObjAffiliate(affiliate);

                    var order_result = await payment.pushCreateNewOrder(cart_detail, client_detail[0], pay_type, voucher_expire, obj_aff, address_id);


                    #endregion

                    #region CONNECT PAYOO
                    if (order_result.order.Id > 0 && order_result.order.AmountVnd > 50000)
                    {
                        //step 1: Empty Cart
                        var result_empty = await cart.emptyCartChoice(email_client, label_id);
                        if (!result_empty)
                        {
                            Utilities.LogHelper.InsertLogTelegram("[FR-NEW: action: payment/create] result_empty error [email_client = " + email_client + ", label_id = " + label_id + " ]");
                        }

                        //step 2: Push queue để đồng bộ với us old
                        // Thực hiện push queue để consummer get push sang us old. Đi vào luồng mua hàng
                        payment.pushOrderToQueue(order_result.order.Id);

                        if (pay_type == (int)PaymentType.ATM_PAYOO_PAY || pay_type == (int)PaymentType.VISA_PAYOO_PAY)
                        {
                            //step 3:  Render Payoo --> Redirect to Payoo
                            var payoo = new PayooService(configuration);

                            var payoo_config = new PayooConfigViewModel
                            {
                                ApiPayooCheckout = configuration["payoo:ApiPayooCheckout"],
                                ShopID = configuration["payoo:ShopID"],
                                ShopDomain = configuration["payoo:ShopDomain"],
                                BusinessUsername = configuration["payoo:BusinessUsername"],
                                ShippingDays = configuration["payoo:ShippingDays"],
                                ShopBackUrl = configuration["payoo:ShopBackUrl"],
                                ShopTitle = configuration["payoo:ShopTitle"],
                                NotifyUrl = configuration["payoo:NotifyUrl"],
                                ChecksumKey = configuration["payoo:ChecksumKey"],
                                EmailUsexpress = configuration["email_cskh"],
                                OrderNo = order_result.order.OrderNo,
                                TotalAmountLast = order_result.order.AmountVnd ?? 0,
                                CustomerName = order_result.order.ClientName,
                                Phone = order_result.order.Phone,
                                Address = order_result.order.Address,
                                CardType = pay_type,
                                BankCode = bank_code,
                                OrderDescription = order_result.order_description
                            };
                            _url_payoo_redirect = await payoo.RedirectToPayoo(payoo_config);
                            return Ok(new { status = _url_payoo_redirect != string.Empty ? (int)ResponseType.SUCCESS : (int)ResponseType.ERROR, order_id = order_result.order.Id, msg = "Create Order New successfully !!!", url_payoo_redirect = _url_payoo_redirect, payment_type = pay_type });
                        }
                        else
                        {
                            // CKTT
                            return Ok(new { status = (int)ResponseType.SUCCESS, order_no = order_result.order.OrderNo, order_id = order_result.order.Id, amount = (order_result.order.AmountVnd ?? 0).ToString("N0"), payment_type = pay_type });
                        }
                    }
                    else
                    {
                        Utilities.LogHelper.InsertLogTelegram("[FR-NEW: action: payment/create] CreateOrder error [address_id = " + address_id + ", pay_type = " + pay_type + ", bank_code = " + bank_code + " ]");
                        return Ok(new { status = (int)ResponseType.FAILED, msg = "create order failted", execute_time_payoo = 0 });
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("CreateOrder with usexpress NEW error [address_id = " + address_id + ", pay_type = " + pay_type + ", bank_code = " + bank_code + " ] ex =" + ex.ToString());
                return Ok(new { status = (int)ResponseType.FAILED });
            }
        }

        /// <summary>
        /// Thanh toán lại
        /// </summary>
        /// <param name="order_id"></param>
        /// <returns></returns>
        [AjaxAuthorize()]
        [ValidationOrder(new[] { "order_id", "address_id", "pay_type", "bank_code" })]
        [HttpPost("Update")]
        public async Task<IActionResult> RePaymentOrder(long order_id, int pay_type, string bank_code, int address_id)
        {
            string _url_payoo_redirect = string.Empty;
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                string url_api_voucher = configuration["url_api_usexpress_new"] + "api/voucher/apply.json";
                string token_tele = configuration["telegram_log_error_fe:Token"];
                string group_id_tele = configuration["telegram_log_error_fe:GroupId"];
                string key_token_api = configuration["KEY_TOKEN_API"];
                long client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);
                var order_detail = new WEB.UI.ViewModels.OrderDetailViewModel();
                var order_sv = new OrderService(configuration);
                var j_order_result = await order_sv.getOrderDetail(order_id, client_id);

                if (j_order_result != string.Empty)
                {
                    order_detail = JsonConvert.DeserializeObject<WEB.UI.ViewModels.OrderDetailViewModel>(j_order_result);
                }
                else
                {
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Hệ thống gặp sự cố. Liên hệ bộ phận CSKH", execute_time_payoo = 0 });
                }

                #region CONNECT PAYOO
                if (order_detail.id > 0)
                {
                    // Cập nhật trạng thái chuyển khoản
                    var payment = new PaymentConnection(configuration, url_api, string.Empty, -1, token_tele, group_id_tele, client_id);

                    var order_result = await payment.UpdatePaymentReCheckOut(order_id, address_id, Convert.ToInt16(pay_type));

                    if (pay_type == (int)PaymentType.ATM_PAYOO_PAY || pay_type == (int)PaymentType.VISA_PAYOO_PAY)
                    {
                        //step 3:  Render Payoo --> Redirect to Payoo
                        var payoo = new PayooService(configuration);

                        var payoo_config = new PayooConfigViewModel
                        {
                            ApiPayooCheckout = configuration["payoo:ApiPayooCheckout"],
                            ShopID = configuration["payoo:ShopID"],
                            ShopDomain = configuration["payoo:ShopDomain"],
                            BusinessUsername = configuration["payoo:BusinessUsername"],
                            ShippingDays = configuration["payoo:ShippingDays"],
                            ShopBackUrl = configuration["payoo:ShopBackUrl"],
                            ShopTitle = configuration["payoo:ShopTitle"],
                            NotifyUrl = configuration["payoo:NotifyUrl"],
                            ChecksumKey = configuration["payoo:ChecksumKey"],
                            EmailUsexpress = configuration["email_cskh"],
                            OrderNo = order_detail.orderNo,
                            TotalAmountLast = Convert.ToDouble(order_detail.amountVnd),
                            CustomerName = order_detail.clientName,
                            Phone = order_detail.phone,
                            Address = order_detail.address,
                            CardType = pay_type,
                            BankCode = bank_code,
                            OrderDescription = order_detail.note
                        };

                        var sw_payoo = new Stopwatch();

                        _url_payoo_redirect = await payoo.RedirectToPayoo(payoo_config);

                        return Ok(new { status = _url_payoo_redirect != string.Empty ? (int)ResponseType.SUCCESS : (int)ResponseType.ERROR, msg = "Update Order Successfully !!!", order_id = order_id, url_payoo_redirect = _url_payoo_redirect, payment_type = pay_type });
                    }
                    else
                    {
                        // CKTT
                        return Ok(new { status = (int)ResponseType.SUCCESS, order_no = order_detail.orderNo, amount = (Convert.ToDouble(order_detail.amountVnd)).ToString("N0"), payment_type = pay_type, order_id = order_id });
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("[FR-NEW: action: payment/update-order] re-checkout error [order_id = " + order_id + " ]");
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Hệ thống gặp sự cố. Liên hệ bộ phận CSKH", execute_time_payoo = 0 });
                }

                #endregion

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("update order with usexpress NEW error [address_id = " + address_id + ", pay_type = " + pay_type + ", bank_code = " + bank_code + " ] ex =" + ex.ToString());
                return Ok(new { status = (int)ResponseType.FAILED });
            }
        }


        [AjaxAuthorize()]
        [HttpPost("confirm-transfer")]
        public IActionResult confirmTransfer(string order_no, string amount_transfer)
        {
            string token_tele = configuration["telegram_monitor_order:token"];
            string group_id_tele = configuration["telegram_monitor_order:group_id"];
            try
            {
                string email_client = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "EMAILID").Value.ToString();
                Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "Tài khoản " + email_client + " đã xác nhận chuyển khoản trực tiếp " + amount_transfer + " cho đơn hàng " + order_no + ". Vào lúc: " + DateTime.Now.ToString("dd-MM-yyyy HH:ss"));
                return Ok(new { status = ResponseType.SUCCESS });
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "confirmTransfer !!! exx =" + ex.ToString());
                return Ok(new { status = ResponseType.FAILED.ToString() });
            }
        }

        /// <summary>
        /// Chờ xác nhận thanh toán với hình thức CKTT
        /// </summary>
        /// <param name="order_no"></param>
        /// <param name="amount_transfer"></param>
        /// <returns></returns>
        [AjaxAuthorize()]
        [HttpGet("confirm-bank/{order_no}")]
        public IActionResult waitingPay(string order_no)
        {
            try
            {
                bool is_success = Convert.ToBoolean(order_no.Length > 5);
                ViewBag.is_success = is_success;
                ViewBag.payment_type = PaymentType.USEXPRESS_BANK;
                ViewBag.order_no = order_no;
                if (!is_success) Utilities.LogHelper.InsertLogTelegram("waitingPay error payment CKTT: Mã đơn " + order_no + " thanh toán thất bại");
                return View("~/Views/Payment/Confirm.cshtml");
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("waitingPay order_no = " + order_no + " ex =" + ex.ToString());
                return Content("Đơn hàng này của bạn thanh toán thất bại. Vui lòng liên hệ cskh@usexpress.vn hoặc số hotline 1900.633.600 để được hỗ trợ.");
            }


        }


    }
}