
using Entities.ViewModels.Carts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.UI.Common;
using WEB.UI.Controllers.Order;
using WEB.UI.ViewModels;
using static Utilities.Contants.Constants;

namespace WEB.UI.Controllers.Carts
{
    public partial class ShoppingCarts
    {
        private readonly IConfiguration configuration;

        public ShoppingCarts(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        // We're using HttpContextBase to allow access to cookies.
        public string GetCartId(HttpContext context)
        {
            try
            {
                if (context.User.Identities.FirstOrDefault().IsAuthenticated)
                {
                    string email = context.User.Claims.FirstOrDefault(x => x.Type == "EMAILID").Value;
                    context.Session.SetString(WEB.UI.Common.Constants.CartSessionKey, email);
                    return context.Session.GetString(WEB.UI.Common.Constants.CartSessionKey).ToString();
                }
                else
                {
                    return string.Empty;
                    //if (string.IsNullOrEmpty(context.Session.GetString(WEB.UI.Common.Constants.CartSessionKey)))
                    //{
                    //    // Generate a new random GUID using System.Guid class
                    //    Guid tempCartId = Guid.NewGuid();
                    //    //        // Send tempCartId back to client as a cookie
                    //    context.Session.SetString(WEB.UI.Common.Constants.CartSessionKey, tempCartId.ToString());
                    //}
                }
                
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "GetCartId " + ex.Message);
                return string.Empty;
            }
        }

        public async Task<int> getTotalCartsByUser(HttpContext context)
        {
            var cart_id = GetCartId(context);
            // User phải có cart_id mới dc truy xuất. Cart_id dc sinh ra khi user add cart 1 sp dau tien bat ky
            if (!string.IsNullOrEmpty(cart_id))
            {
                string ShoppingCartId = GetCartId(context);
                try
                {
                    string j_param = "{'cart_id':'" + ShoppingCartId + "','label_id': -1,'key_id':'-1'}";
                    string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);
                    string url_api = configuration["url_api_usexpress_new"] + "api/carts/find-by-cart-id.json";

                    var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                    var response_api = await connect_api_us.CreateHttpRequest();
                    // Nhan ket qua tra ve                            
                    var JsonParent = JArray.Parse("[" + response_api + "]");
                    string status = JsonParent[0]["status"].ToString();
                    string _msg = JsonParent[0]["msg"].ToString();

                    if (status == ResponseType.SUCCESS.ToString())
                    {
                        if (JsonParent[0]["list_cart"].Count() > 0)
                        {
                            var carts_list = JsonConvert.DeserializeObject<List<CartItemViewModels>>(JsonParent[0]["list_cart"].ToString());
                            return carts_list.Sum(x => x.quantity);
                        }
                        else return 0;
                    }
                    else
                    {
                        Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getTotalCarts _msg response api = " + _msg);
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getTotalCarts _msg error = " + ex.ToString());
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        public async Task<bool> MappingToCart(string cart_id_old, string cart_id_new)
        {
            try
            {
                string j_param = "{'cart_new_id':'" + cart_id_new + "','cart_old_id':'" + cart_id_old + "'}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);
                string url_api = configuration["url_api_usexpress_new"] + "api/carts/mapping-to-carts.json";

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status == ResponseType.SUCCESS.ToString())
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "MappingToCart _msg error = " + ex.ToString());
                return false;
            }
        }

        public async Task<List<CartsViewModels>> getCartListByUser(string cart_id, int label_id, string key_id, int i_selected)
        {
            try
            {
                string j_param = "{'cart_id':'" + cart_id + "', 'label_id':'" + label_id + "', 'key_id':'" + key_id + "'}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);
                string url_api = configuration["url_api_usexpress_new"] + "api/carts/find-by-cart-id.json";

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status == ResponseType.SUCCESS.ToString())
                {
                    var obj_list_cart = new List<CartsViewModels>();
                    var data_cart = JsonConvert.DeserializeObject<List<CartItemViewModels>>(JsonParent[0]["list_cart"].ToString());
                    if (data_cart != null)
                    {
                        if (data_cart.Count() > 0)
                        {
                            //Lọc theo selected: -1: all | 0:false | 1: true
                            data_cart = data_cart.Where(x => x.is_selected == (i_selected == -1 ? x.is_selected : Convert.ToBoolean(i_selected))).ToList();

                            // Nhóm đơn hàng theo nhãn hàng
                            var group_cart = data_cart.GroupBy(p => new
                            {
                                StoreId = p.label_id
                            }).Select(group =>
                                new
                                {
                                    label_id = group.Key.StoreId,
                                    cart_group = group.ToList(),
                                    total_amount_cart = group.Where(x => x.is_selected).Sum(p => p.amount_last_vnd), // Tien hang vnd 
                                    total_discount_amount = group.Where(x => x.is_selected).Sum(p => p.price_discount_first_pound), // Tổng số tiền được giảm theo Rule của từng store         
                                    total_discound_voucher = 0, // triển khai voucher sẽ ghép vô
                                    total_selected_carts = group.Where(x => x.is_selected).Count(),
                                    total_shipping_fee_us = group.Where(x => x.is_selected).Sum(p => p.product_detail.list_product_fee.list_product_fee["TOTAL_SHIPPING_FEE"])
                                });


                            foreach (var item in group_cart)
                            {
                                var item_cart_group = new CartsViewModels
                                {
                                    label_id = item.label_id,
                                    label_name = LabelNameType.GetLabelName(item.label_id).ToString(),
                                    cart_item = item.cart_group,
                                    total_product = item.cart_group.Count(),
                                    total_amount_cart = item.total_amount_cart, // tien hang
                                    total_discount_amount = item.total_discount_amount,      // so tien duoc giam                                                                   
                                    total_amount_last = item.total_amount_cart - (item.total_discount_amount + item.total_discound_voucher), //tong tien sau giam
                                    total_selected_carts = item.total_selected_carts,
                                    total_shipping_fee_us = item.total_shipping_fee_us
                                };
                                obj_list_cart.Add(item_cart_group);
                            }
                            return obj_list_cart;
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getCartListByUser _msg error = " + ex.ToString());
                return null;
            }
        }

        public async Task<List<CartsViewModels>> getCartListByOrderId(OrderDetailViewModel order_model)
        {
            try
            {
                var obj_list_cart = new List<CartsViewModels>();
                var obj_product_list = new List<CartItemViewModels>();

                int _id = 0;

                // Chỉ thực hiện khi đơn là CHỜ THANH TOÁN
                if (order_model.orderStatus != (int)OrderStatus.CREATED_ORDER)
                {
                    return null;
                }

                foreach (var item in order_model.productList)
                {
                    var _image_size_product = new List<Entities.ViewModels.ImageSizeViewModel>
                        {
                            new Entities.ViewModels.ImageSizeViewModel { Thumb = item.imageThumb }
                        };
                    var _list_product_fee = new Entities.ViewModels.ProductFeeViewModel
                    {
                        amount_vnd = Convert.ToDouble(item.price)
                    };

                    var _product_detail = new Entities.ViewModels.ProductViewModel
                    {
                        product_code = item.productCode,
                        product_name = item.title,
                        image_size_product = _image_size_product,
                        seller_name = item.sellerName,
                        id = _id,
                        list_product_fee = _list_product_fee
                    };

                    var model = new CartItemViewModels
                    {
                        product_code = item.productCode,
                        product_detail = _product_detail,
                        quantity = item.quantity,
                        amount_last = Convert.ToDouble(item.price),
                        amount_last_vnd = Convert.ToDouble(item.price) * item.quantity
                    };

                    obj_product_list.Add(model);
                    _id += 1;
                }


                var item_cart_group = new CartsViewModels
                {
                    label_id = order_model.labelId,
                    label_name = LabelNameType.GetLabelName(order_model.labelId).ToString(),
                    cart_item = obj_product_list,
                    total_product = order_model.productList.Count(),
                    total_amount_cart = Convert.ToDouble(order_model.priceVnd), // tien hang
                    total_discount_amount = Convert.ToDouble(order_model.amountVnd) - Convert.ToDouble(order_model.priceVnd),      // so tien duoc giam                                                                   
                    total_amount_last = Convert.ToDouble(order_model.amountVnd), //tong tien sau giam
                    total_selected_carts = order_model.productList.Count(),
                    total_shipping_fee_us = 0 // ko can gan mac dinh = 0
                };
                obj_list_cart.Add(item_cart_group);

                return obj_list_cart;

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getCartListByUser _msg error = " + ex.ToString());
                return null;
            }
        }

        public async Task<double> getCartListByListId(string lst_key_id)
        {
            try
            {
                string j_param = "{'lst_key_id':'" + lst_key_id + "'}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);
                string url_api = configuration["url_api_usexpress_new"] + "api/carts/get-total-price-cart.json";

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    double total_price_cart = Convert.ToDouble(JsonParent[0]["total_price_cart"]);
                    return total_price_cart;
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "response api getCartListByListId  error = " + _msg);
                }
                return 0;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getCartListByListId _msg error = " + ex.ToString());
                return 0;
            }
        }

        public async Task<CartItemViewModels> updateCart(CartItemViewModels cart_item)
        {
            try
            {
                string j_param = "{'cart_item':'" + Newtonsoft.Json.JsonConvert.SerializeObject(cart_item) + "'}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);
                string url_api = configuration["url_api_usexpress_new"] + "api/carts/update-by-key-id.json";

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status == ResponseType.SUCCESS.ToString())
                {
                    string data_cart = _msg;
                    return JsonConvert.DeserializeObject<CartItemViewModels>(data_cart);
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "response api error = " + _msg);
                }
                return null;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "updateCart _msg error = " + ex.ToString());
                return null;
            }
        }

        public async Task<bool> deleteCart(string key_id)
        {
            try
            {
                string j_param = "{'key_id':'" + key_id + "'}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);
                string url_api = configuration["url_api_usexpress_new"] + "api/carts/delete.json";

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status == ResponseType.SUCCESS.ToString())
                {
                    return true;
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "response api deleteCart  error = " + _msg);
                }
                return false;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "deleteCart _msg error = " + ex.ToString());
                return false;
            }
        }

        public async Task<bool> saveProductChoice(string lst_key_id, string cart_id)
        {
            try
            {
                string j_param = "{'lst_key_id':'" + lst_key_id + "', 'cart_id':'" + cart_id + "'}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);
                string url_api = configuration["url_api_usexpress_new"] + "api/carts/save-product-choice.json";

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status == ResponseType.SUCCESS.ToString())
                {
                    return true;
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "response api saveProductChoice  error = " + _msg);
                }
                return false;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "saveProductChoice _msg error = " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Empty cart sau khi tạo đơn thành công
        /// </summary>
        /// <param name="cart_id"></param>
        /// <returns></returns>
        public async Task<bool> emptyCartChoice(string cart_id, int label_id)
        {
            try
            {
                string j_param = "{'cart_id':'" + cart_id + "', 'label_id':" + label_id + "}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);
                string url_api = configuration["url_api_usexpress_new"] + "api/carts/empty-cart.json";

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status == ResponseType.SUCCESS.ToString())
                {
                    return true;
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "response api emptyCartChoice error = " + _msg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "emptyCartChoice _msg error = " + ex.ToString());
                return false;
            }
        }
    }
}
