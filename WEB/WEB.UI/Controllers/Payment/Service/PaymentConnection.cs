using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Affiliate;
using Entities.ViewModels.Carts;
using Entities.ViewModels.Payment;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.UI.Common;
using WEB.UI.Controllers.Carts;
using WEB.UI.ViewModels;

namespace WEB.UI.Controllers.Payment.Service
{
    public partial class PaymentConnection
    {

        private readonly IConfiguration configuration;
        public string domain_us_api_new { get; set; }
        public string token_tele { get; set; }
        public string group_id_tele { get; set; }
        public long client_id { get; set; }
        public string email { get; set; }

        public int label_id { get; set; }

        public PaymentConnection(IConfiguration _configuration, string _domain_us_api_new, string _email, int _label_id, string _token_tele, string _group_id_tele, long _client_id)
        {
            configuration = _configuration;
            domain_us_api_new = _domain_us_api_new;
            label_id = _label_id;
            email = _email;
            // encrypt_api = _encrypt_api;
            token_tele = _token_tele;
            group_id_tele = _group_id_tele;
            client_id = _client_id;
        }

        /// <summary>
        /// Khởi tạo đơn hàng 
        /// </summary>
        /// <param name="obj_cart"></param>
        /// <param name="client_detail"></param>
        /// <param name="pay_type"></param>
        /// <param name="voucher_valid">List các vc hợp lệ</param>
        /// <returns></returns>
        public async Task<OrderEntitiesViewModel> pushCreateNewOrder(List<CartsViewModels> obj_cart, AddressReceiverOrderViewModel client_detail, int pay_type, List<VoucherEntitiesViewModel> voucher_valid, AffiliateViewModel aff, long address_id)
        {
            string token = string.Empty;
            try
            {
                var order_entities = new OrderEntitiesViewModel();
                string order_description = string.Empty;
                var list_order_item_model = new List<OrderItemViewModel>();
                var product_model = new List<ProductViewModel>();
                double _total_discount_2nd_vnd = 0; // Số tiền dc giảm theo chính sách
                double _total_discount_voucher_vnd = 0; // Tổng tiền giảm của voucher
                double _total_price_sale = 0; // tong tien giam gia
                string _voucher_name = string.Empty;
                string _voucher_mkt = string.Empty;

                #region INPUT ORDER
                // step 1: Lấy ra mã đơn hàng
                string order_no = await getNewOrderNo(label_id);
                if (string.IsNullOrEmpty(order_no))
                {
                    Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[USEXPRESS_NEW]: Lỗi khi khởi tạo số hợp đồng cho clientid= " + client_detail.ClientId);
                    return null;
                }

                // step3: Lấy ra chi tiết sản phẩm trong giỏ đã chọn
                var cart_list = obj_cart.FirstOrDefault();

                // step 4: Lấy ra tỉ giá hiện tại
                // Với đơn dạng fix tỷ giá sẽ chạy theo tỷ giá
                double rate_current = 0;
                if (order_no.ToUpper().IndexOf("UCC") >= 0)
                {
                    rate_current = cart_list.cart_item.ToList()[0].rate_current;// với mã UCC hiểu là sản phẩm manual nhập tay
                }
                else
                {
                    rate_current = Utilities.Common.CommonHelper.getRateCurrent(domain_us_api_new + "api/ServicePublic/rate.json");
                }


                #region step 5: Tính tổng số tiền được giảm theo chính sách của USEXPRESS

                if (voucher_valid.Count() > 0)
                {
                    _voucher_name = string.Join(",", voucher_valid.Select(x => x.voucher_name));

                    // Chính sách chung. Tại 1 thời điểm chỉ có 1 chính sách cho 1 nhãn
                    var obj_vc_2nd = voucher_valid.FirstOrDefault(x => x.rule_type == VoucherRuleType.AMZ_DISCOUNT_FPF || x.rule_type == VoucherRuleType.JOMA_DISCOUNT);
                    _total_discount_2nd_vnd = obj_vc_2nd != null ? obj_vc_2nd.total_price_sale : 0;


                    // Voucher marketing

                    var obj_vc_mkt = obj_vc_2nd != null ? voucher_valid.Where(x => x.voucher_id != obj_vc_2nd.voucher_id).ToList() : voucher_valid.ToList();

                    _total_discount_voucher_vnd = obj_vc_mkt.Count() > 0 ? obj_vc_mkt.Sum(x => x.total_price_sale) : 0;
                    _voucher_mkt = obj_vc_mkt.Count() > 0 ? obj_vc_mkt[0].voucher_name : "";

                }

                _total_price_sale = _total_discount_2nd_vnd + _total_discount_voucher_vnd; // Tổng tiền được giảm

                #endregion

                // Tổng giá trị đơn hàng
                double _PriceVnd = cart_list.total_amount_cart;
                double _AmountVnd = cart_list.total_amount_cart - _total_price_sale;

                var order_model = new OrderViewModel
                {
                    ClientId = client_id,
                    UserId = -1,
                    LabelId = (short)label_id,
                    OrderNo = order_no,
                    ClientName = client_detail.ReceiverName,
                    Email = email,
                    Phone = client_detail.Phone,
                    Address = client_detail.FullAddress,
                    CreatedOn = DateTime.Now,
                    RateCurrent = rate_current,
                    PriceVnd = _PriceVnd,
                    AmountVnd = _AmountVnd,

                    TotalDiscount2ndUsd = _total_discount_2nd_vnd == 0 ? 0 : Math.Round(_total_discount_2nd_vnd / rate_current),// ver new sẽ ngắt giảm từ sp thứ 2 trở lên khi mua mà sẽ quy đổi sang voucher
                    TotalDiscount2ndVnd = _total_discount_2nd_vnd,

                    TotalShippingFeeUsd = cart_list.total_shipping_fee_us,
                    TotalShippingFeeVnd = cart_list.total_shipping_fee_us * rate_current,

                    TotalDiscountVoucherVnd = _total_discount_voucher_vnd, // tong so tien duoc giam
                    TotalDiscountVoucherUsd = _total_discount_voucher_vnd == 0 ? 0 : _total_discount_voucher_vnd / rate_current,

                    UtmMedium = aff == null ? "Direct" : aff.utm_medium,
                    UtmCampaign = aff == null ? "" : aff.utm_campaign,
                    UtmSource = aff == null ? "Direct" : aff.utm_source,
                    UtmFirstTime = aff == null ? "Direct" : aff.utm_first_time,

                    Voucher = _voucher_mkt,
                    VoucherName = _voucher_name, //listing voucher
                    Discount = (_total_price_sale / _PriceVnd) * 100, // chiet khau % cua don hang quy doi
                    PriceUsd = _PriceVnd / rate_current,
                    AmountUsd = _AmountVnd / rate_current,

                    Note = "Đơn được khởi tạo trực tiếp từ website usexpress new",
                    PaymentType = (short)pay_type,
                    PaymentStatus = (short)OrderConstants.Payment_Status.CHUA_THANH_TOAN,
                    PaymentDate = Convert.ToDateTime("2000-01-01 00:00:00.000"),
                    OrderStatus = (short)Utilities.Contants.Constants.OrderStatus.CREATED_ORDER,
                    AddressId = address_id
                };
                #endregion

                #region INPUT PRODUCT + ORDERITEM + PRODUCT IMAGE
                // create product
                var variations = new Dictionary<string, string>();
                var image_product_model = new List<ImageProductViewModel>();
                int d = -1;
                foreach (var item in cart_list.cart_item.ToList())
                {
                    var variation_current = item.product_detail.list_variations == null ? null : item.product_detail.list_variations.Where(x => x.selected).ToList();
                    var ImageProduct = new ImageProductViewModel
                    {
                        ProductMapId = d,
                        Image = item.product_detail.image_thumb ?? string.Empty
                    };
                    image_product_model.Add(ImageProduct);

                    // Những trường cần thống kê cho report sẽ lưu vào đây
                    var model = new ProductViewModel
                    {
                        product_code = item.product_detail.product_code,
                        product_map_id = d,
                        product_name = item.product_detail.product_name.Replace("'", ""),
                        price = item.product_detail.price + item.product_detail.shiping_fee,
                        discount = 0,
                        amount = item.product_detail.amount,
                        rating = item.product_detail.rating,
                        manufacturer = CommonHelper.RemoveSpecialCharacters(item.product_detail.seller_name),
                        seller_name = CommonHelper.RemoveSpecialCharacters(item.product_detail.seller_name),
                        label_id = label_id,
                        reviews_count = item.product_detail.reviews_count,
                        is_prime_eligible = item.product_detail.is_prime_eligible,
                        rate = rate_current,
                        seller_id = item.product_detail.seller_id,
                        variations = string.Empty,
                        list_variations = variation_current == null ? null : variation_current,
                        link_product = item.product_detail.keywork_search
                    };
                    product_model.Add(model);

                    var order_item_model = new OrderItemViewModel
                    {
                        OrderItemMapId = d,
                        ProductMapId = d,
                        DiscountShippingFirstPound = 0,
                        Price = model.price,
                        ProductCode = model.product_code,
                        ProductImage = model.product_name,
                        FirstPoundFee = item.product_detail.list_product_fee.list_product_fee[FeeBuyType.FIRST_POUND_FEE.ToString()],
                        NextPoundFee = item.product_detail.list_product_fee.list_product_fee[FeeBuyType.NEXT_POUND_FEE.ToString()],
                        ShippingFeeUs = item.product_detail.shiping_fee,
                        LuxuryFee = item.product_detail.list_product_fee.list_product_fee[FeeBuyType.LUXURY_FEE.ToString()],
                        Quantity = item.quantity,
                        Weight = item.product_detail.list_product_fee.list_product_fee.ContainsKey(FeeBuyType.ITEM_WEIGHT.ToString()) ? item.product_detail.list_product_fee.list_product_fee[FeeBuyType.ITEM_WEIGHT.ToString()] : 1
                    };
                    list_order_item_model.Add(order_item_model);

                    order_description += order_description != string.Empty ? ", " : ": ";
                    order_description += model.product_name + " - Số lượng: " + order_item_model.Quantity;

                    d -= 1;
                }
                order_model.Note = order_description; // update note cho PAYOO
                #endregion

                #region GEN TOKEN                      

                var options_api = new OrderEntitiesApiViewModel
                {
                    order_info = order_model,
                    list_order_item_info = list_order_item_model,
                    list_product_info = product_model,
                    list_note_order = null,
                    list_image_product = image_product_model
                };

                token = CommonHelper.Encode(JsonConvert.SerializeObject(options_api), configuration["KEY_TOKEN_API_2"]);
                #endregion

                #region CREATE ORDER NEW BY API USEXPRESS NEW
                string url_api_push_order = domain_us_api_new + "api/order/addNewOrder";

                var connect_api_us = new ConnectApi(url_api_push_order, token_tele, group_id_tele, token);

                var response_api_order = await connect_api_us.CreateHttpRequest();
                JArray JsonParent = JArray.Parse("[" + response_api_order + "]");
                string order_status = JsonParent[0]["status"].ToString().Replace("\"", "");
                string result_order_id = JsonParent[0]["order_id_response"].ToString().Replace("\"", "");
                if (order_status.ToLower() == "success" && result_order_id != "-1")
                {
                    order_model.Id = Convert.ToInt64(result_order_id);
                    order_entities = new OrderEntitiesViewModel
                    {
                        order = order_model,
                        order_item = list_order_item_model,
                        order_description = order_description
                    };
                    //  Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[USEXPRESS_NEW: " + result_order_id + "] Khởi tạo đơn hàng " + order_model.OrderNo + " thành công !");                   
                }
                else
                {
                    string msg = JsonParent[0]["msg"].ToString().Replace("\"", "");
                    Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[USEXPRESS_NEW] Khởi tạo đơn hàng thất bại ! error response api = " + msg + "--> token add new order" + token);
                }

                #endregion

                return order_entities;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "pushCreateNewOrder in PaymentConnection.cs error from frontend: obj_cart=" + JsonConvert.SerializeObject(obj_cart) + "==, ex=> " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Sinh mã đơn hàng
        /// </summary>
        /// <param name="label_id"></param>
        /// <returns></returns>
        private async Task<string> getNewOrderNo(int label_id)
        {
            string order_no = string.Empty;
            try
            {
                string url_api_push_order = domain_us_api_new + "api/order/create-code.json";
                string j_param = "{\"label_id\":\"" + label_id + "\"}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api_push_order, token_tele, group_id_tele, token);
                string data = await connect_api_us.CreateHttpRequest();
                JArray JsonParent = JArray.Parse("[" + data + "]");
                string order_status = JsonParent[0]["status"].ToString().Replace("\"", "");

                if (order_status.ToLower() == "success")
                {
                    order_no = JsonParent[0]["order_no"].ToString().Replace("\"", "");
                }
                return order_no;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "getOrderNo in PaymentConnection.cs error from frontend: " + ex.ToString());
                return string.Empty;
            }
        }

        public async Task<OrderViewModel> getOrderDetail(string order_no)
        {
            try
            {
                string endpoint = domain_us_api_new + "api/order/getOrderDetail.json";
                string j_param = "{\"orderNo\":\"" + order_no + "\"}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(endpoint, token_tele, group_id_tele, token);
                string data = await connect_api_us.CreateHttpRequest();
                JArray JsonParent = JArray.Parse("[" + data + "]");
                string order_status = JsonParent[0]["status"].ToString().Replace("\"", "");

                if (order_status.ToLower() == "success")
                {
                    var j_order_detail = JsonParent[0]["order_detail"].ToString();
                    var order_detail = JsonConvert.DeserializeObject<OrderViewModel>(j_order_detail);
                    return order_detail;
                }
                return null;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "getOrderDetail in PaymentConnection.cs error from frontend: order_no =" + order_no + " " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Gửi trang thái thanh toán thành công của đơn hàng từ FE NEW -> FE OLD để tiến hành mua tự động
        /// </summary>
        /// <param name="order_no"></param>
        /// <returns></returns>
        public async Task<bool> UpdatePaymentOrderToUsOld(string domain_us_api_old, string order_no)
        {
            string token = string.Empty;
            string endpoint = string.Empty;
            try
            {
                endpoint = domain_us_api_old + "ApiUsexpress/activeAutoBuy";
                string j_param = "{\"order_no\":\"" + order_no + "\"}";
                token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(endpoint, token_tele, group_id_tele, token);
                string data = await connect_api_us.CreateHttpRequest();
                JArray JsonParent = JArray.Parse("[" + data + "]");
                string order_status = JsonParent[0]["status"].ToString().Replace("\"", "");

                if (order_status.ToLower() != "success")
                {
                    var msg = JsonParent[0]["msg"].ToString();
                    Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[FR NEW] PushOrderToUsOld method activeAutoBuy in PaymentConnection.cs error from frontend: order_no =" + order_no + ", msg =" + msg);
                }
                else
                {

                    #region Process 4: Cập nhật trạng thái đơn về us new sau khi hoan tat cac quy trinh ben us old
                    // us new se cap nhat trạng thái đơn sau khi api nay update thah cong mọi quy trình
                    endpoint = configuration["url_api_usexpress_new"] + "api/payment-confirm.json";
                    j_param = "{\"order_no\":\"" + order_no + "\"}";
                    token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                    var connect_api_us_new = new ConnectApi(endpoint, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                    var response_api = await connect_api_us.CreateHttpRequest();
                    // Nhan ket qua tra ve                            
                    var json_parent_new = JArray.Parse("[" + response_api + "]");
                    string status = JsonParent[0]["status"].ToString();
                    string _msg = JsonParent[0]["msg"].ToString();

                    if (status != ResponseType.SUCCESS.ToString())
                    {
                        Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[order_no = " + order_no + "] push Order ToQueue: reponse push api:" + _msg);
                    }

                    #endregion
                }

                return order_status.ToLower() != "success" ? false : true;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "getOrderDetail in PaymentConnection.cs error from frontend: token =" + token + " , endpoint= " + endpoint + ",error =" + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Push Order to queue để Job thực hiện mapping sang us old
        /// order_id: của us new. Job sẽ vào queue get detail để push qua us old
        /// </summary>
        public async void pushOrderToQueue(long order_id)
        {
            try
            {
                var j_param = new Dictionary<string, string>
                {
                    {"data_push",order_id.ToString()},
                    {"type",TaskQueueName.order_new_convert_queue},
                };
                string token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), configuration["KEY_TOKEN_API_2"]);

                string url_api = configuration["url_api_usexpress_new"] + "api/QueueService/data-push.json";

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status != ResponseType.SUCCESS.ToString())
                {
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[order_id = " + order_id + "] push Order ToQueue: reponse push api:" + _msg);
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[order_id = " + order_id + "] pushOrderToQueue " + ex.Message);
            }
        }


        public async Task<bool> UpdatePaymentReCheckOut(long order_id, int address_id, short pay_type)
        {
            string order_no = string.Empty;
            try
            {
                string url_api_push_order = domain_us_api_new + "api/order/update-order-re-checkout.json";
                string j_param = "{\"order_id\":\"" + order_id + "\",\"address_id\":\"" + address_id + "\",\"pay_type\":\"" + pay_type + "\"}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api_push_order, token_tele, group_id_tele, token);
                string data = await connect_api_us.CreateHttpRequest();
                JArray JsonParent = JArray.Parse("[" + data + "]");
                int order_status = Convert.ToInt32(JsonParent[0]["status"].ToString().Replace("\"", ""));
                return order_status == (int)ResponseType.SUCCESS;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[FR] UpdatePaymentReCheckOut in PaymentConnection.cs error from frontend: " + ex.ToString());
                return false;
            }
        }

        //yYWJiWoFYu4SlKN0AKpGcehEoombjZMi0uW8P70gW4gtMoZn,454,referral,accesstrade
        public async Task<AffiliateViewModel> ObjAffiliate(string s_param)
        {
            try
            {
                var arr = s_param.Split(",");
                if (arr.Length >= 4)
                {
                    var model = new AffiliateViewModel
                    {
                        utm_medium = arr[0],
                        utm_campaign = arr[1],
                        utm_first_time = arr[2],
                        utm_source = arr[3]
                    };
                    return model;
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[FR] ObjAffiliate in PaymentConnection.cs error from frontend: tong field cua s_param < 4" + " s_param =" + s_param);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[FR] ObjAffiliate in PaymentConnection.cs error from frontend: " + ex.ToString() + " s_param =" + s_param);
                return null;
            }
        }

    }
}
