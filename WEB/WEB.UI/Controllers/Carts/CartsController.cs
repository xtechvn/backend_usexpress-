using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using Entities.ViewModels;
using Entities.ViewModels.Carts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;
using Utilities;
using Utilities.Contants;
using WEB.UI.Common;
using WEB.UI.Controllers.Voucher.Base;
using WEB.UI.FilterAttribute;
using WEB.UI.Service;

namespace WEB.UI.Controllers.Carts
{
    [Route("[controller]")]
    public class CartsController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public CartsController(RedisConn _redisService, IConfiguration _Configuration)
        {
            redisService = _redisService;
            configuration = _Configuration;
        }
        // Xây dựng attribute filter nhưng sản phẩm hàng cấm trên này
        [AjaxAuthorize()]
        [HttpPost("add-to-cart")]
        [AllowAnonymous]
        public async Task<IActionResult> addCart(string product_code, string seller_id, int label_id)
        {
            string KEY_TOKEN_API = configuration["KEY_TOKEN_API"];
            try
            {
                string cache_key = CacheHelper.cacheKeyProductDetail(product_code, label_id);
                var product_detail = new ProductViewModel();

                var j_product_detail = await redisService.GetAsync(cache_key, Convert.ToInt32(configuration["Redis:Database:db_product_amazon"]));
                if (string.IsNullOrEmpty(j_product_detail))
                {
                    // Kiểm tra trong ES có không
                    string INDEX_ES_PRODUCT = configuration["DataBaseConfig:Elastic:index_product_search"];
                    string ES_HOST = configuration["DataBaseConfig:Elastic:Host"];
                    var ESRepository = new ESRepository<object>(ES_HOST);
                    product_detail = ESRepository.getProductDetailByCode(INDEX_ES_PRODUCT, product_code, label_id);

                    if (product_detail == null)
                    {
                        // bật lên lightbox thông báo ngoài view. Sản phẩm đã cập nhật lại giá bán. Xin vui lòng chờ trong giây lát. Reload lại page detail
                        return Ok(new { status = ResponseType.EMPTY, msg = "Sản phẩm không tồn tại", is_refresh = true }); // refresh lại trang để crawl lại data
                    }
                }
                else
                {
                    // Đọc trong Redis
                    product_detail = JsonConvert.DeserializeObject<ProductViewModel>(j_product_detail);
                }

                // add sản phẩm vào giỏ hàng. push db
                var cart = new ShoppingCarts(configuration);
                string shopping_cart_id = cart.GetCartId(this.HttpContext);

                // Remove param
                product_detail.color_images = null;
                product_detail.list_variations = null;
                product_detail.product_specification = null;
                product_detail.product_infomation = null;
                product_detail.product_related = null;
                product_detail.product_infomation_HTML = null;                
                var cart_model = new CartItemViewModels()
                {
                    cart_id = shopping_cart_id, // dinh danh user                  
                    quantity = 1,
                    seller_id = seller_id,//sellerid ma user chon                    
                    product_code = product_code,
                    cart_status = (int)StatusType.BINH_THUONG,
                    product_detail = product_detail,
                    create_date = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString(),
                    update_last = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString(),
                    rate_current = product_detail.product_type == ProductType.FIXED_AMOUNT_VND ? product_detail.rate : CommonHelper.getRateCurrent(configuration["url_api_usexpress_new"] + "api/ServicePublic/rate.json"),
                    label_id = label_id
                };

                #region PUSH API
                string j_param = "{'cart_item':'" + Newtonsoft.Json.JsonConvert.SerializeObject(cart_model).Replace("'", "") + "'}";
                string token = CommonHelper.Encode(j_param, KEY_TOKEN_API);
                string url_api = configuration["url_api_usexpress_new"] + "api/carts/addnew.json";
                string token_tele = configuration["telegram_log_error_fe:Token"];
                string group_id_tele = configuration["telegram_log_error_fe:GroupId"];

                var connect_api_us = new ConnectApi(url_api, token_tele, group_id_tele, token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status == ResponseType.SUCCESS.ToString())
                {
                    return Ok(new { status = ResponseType.SUCCESS, msg = _msg });
                }
                else
                {
                    LogHelper.InsertLogTelegram(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "add-to-cart error from frontend: " + _msg.ToString() + " token =" + token);
                    return Ok(new { status = ResponseType.EMPTY, msg = "add-to-cart error", url_api = url_api, token = token });
                }

                #endregion
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("web addToCart " + ex.Message);
                return Ok(new { status = ResponseType.ERROR, msg = ex.ToString() });
            }
        }

        [AjaxAuthorize()]
        [HttpPost("update-to-cart.json")]
        [AllowAnonymous]
        public async Task<IActionResult> updateCart(string key_id, int quantity)
        {
            try
            {
                if (string.IsNullOrEmpty(key_id))
                {
                    return Ok(new { status = ResponseType.EMPTY, msg = "Thông tin sản phẩm trong giỏ hàng đã hết hạn. Xin vui lòng thêm lại vào giỏ hàng" });
                }

                if (quantity > 10000)
                {
                    return Ok(new { status = ResponseType.FAILED, msg = "Hiện tại Usexpress chỉ hỗ trợ cho phép mua sản phẩm không được vượt quá 10000" });
                }

                // add sản phẩm vào giỏ hàng. push db
                var cart = new ShoppingCarts(configuration);
                var cart_item = new CartItemViewModels
                {
                    id = key_id,
                    quantity = quantity == 0 ? 1 : quantity,
                    rate_current = CommonHelper.getRateCurrent(configuration["url_api_usexpress_new"] + "api/ServicePublic/rate.json")
                };
                var cart_detail = await cart.updateCart(cart_item);

                if (cart_detail != null)
                {
                    return Ok(new { status = ResponseType.SUCCESS, msg = "Successfully", amount_last_vnd = cart_detail.amount_last_vnd });
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("web update-to-cart by frontend  loi khi cap nhat so luong ");
                    return Ok(new { status = ResponseType.ERROR, msg = "Update cart loi khi cap nhat so luong" });
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("web addToCart " + ex.Message);
                return Ok(new { status = ResponseType.ERROR, msg = ex.ToString() });
            }
        }

        [AjaxAuthorize()]
        [Route("view")]
        public async Task<IActionResult> viewCart()
        {
            try
            {
                var cart = new ShoppingCarts(configuration);

                //get cartslist
                int total_carts = await cart.getTotalCartsByUser(this.HttpContext);
                ViewBag.total_carts = total_carts;
                return View();
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("web viewCart " + ex.Message);
                ViewBag.total_carts = 0;
                return View();
            }
        }

        [AjaxAuthorize()]
        [HttpPost("delete-item-cart.json")]
        [AllowAnonymous]
        public async Task<IActionResult> deleteItemCart(string key_id)
        {
            try
            {
                if (string.IsNullOrEmpty(key_id) || key_id.Length < 15)
                {
                    return Ok(new { status = ResponseType.EMPTY, msg = "key_cart_id không đúng định dạng" });
                }

                // add sản phẩm vào giỏ hàng. push db
                var cart = new ShoppingCarts(configuration);

                var cart_result = await cart.deleteCart(key_id);

                if (cart_result)
                {
                    return Ok(new { status = ResponseType.SUCCESS, msg = "Successfully" });
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("deleteItemCart failed !!!");
                    return Ok(new { status = ResponseType.ERROR, msg = "delete cart loi" });
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("web deleteItemCart " + ex.Message);
                return Ok(new { status = ResponseType.ERROR, msg = ex.ToString() });
            }
        }

        /// <summary>
        /// Lưu danh sách sp được chọn trong giỏ hàng
        /// </summary>
        /// <param name="lst_key_id"></param>
        /// <returns></returns>
        [AjaxAuthorize()]
        [HttpPost("save-carts-choice.json")]
        [AllowAnonymous]
        public async Task<IActionResult> saveCartsChoice(string lst_key_id, string list_voucher, int label_id)
        {
            string token_tele = configuration["telegram_log_error_fe:Token"];
            string group_id_tele = configuration["telegram_log_error_fe:GroupId"];
            try
            {
                double _total_price_sale_vc = 0;
                string _noti_voucher = string.Empty;
                string _voucher_change = string.Empty;
                if (string.IsNullOrEmpty(lst_key_id))
                {
                    return Ok(new { status = ResponseType.EMPTY, msg = "Xin lòng chọn sản phẩm trong giỏ hàng" });
                }

                // add sản phẩm vào giỏ hàng. push db
                var cart = new ShoppingCarts(configuration);
                string cart_id = cart.GetCartId(this.HttpContext);
                var result_save_choive = await cart.saveProductChoice(lst_key_id, cart_id);

                #region  Vaidation voucher
                if (!string.IsNullOrEmpty(list_voucher))
                {
                    var arr_voucher_choice = list_voucher.Split(",");
                    for (int i = 0; i <= arr_voucher_choice.Length - 1; i++)
                    {
                        string domain_us_api_new = configuration["url_api_usexpress_new"] + "api/voucher/apply.json";
                        string KEY_TOKEN_API = configuration["KEY_TOKEN_API"];
                        string email = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "EMAILID").Value.ToString();
                        string vc_name = arr_voucher_choice[i].Trim();
                        // Kiểm tra tính hợp lệ của vc
                        // Kiểm tra voucher lặp

                        var sv_voucher = new VoucherService(domain_us_api_new, email, vc_name, token_tele, group_id_tele, label_id, KEY_TOKEN_API);
                        var vc_detail = await sv_voucher.getPriceSaleVoucher();
                        if (!(vc_detail.status == (int)ResponseType.SUCCESS))
                        {
                            Utilities.LogHelper.InsertLogTelegram("[FR]CartController -  save-carts-choice.json.json: Mã voucher gặp sự cố có thể đã hết hạn vc_name = " + vc_name + " email = " + email);
                            _noti_voucher = "Rất tiếc! Không thể tìm thấy mã voucher " + vc_name + " hoặc mã đã hết hiệu lực. Bạn vui lòng kiểm tra lại mã hoặc liên hệ với bộ phận CSKH để được hỗ trợ";
                            //return Ok(new { status = ResponseType.FAILED, msg = "Rất tiếc! Không thể tìm thấy mã voucher " + vc_name + " hoặc mã đã hết hiệu lực. Bạn vui lòng kiểm tra lại mã hoặc liên hệ với bộ phận CSKH để được hỗ trợ" });
                        }
                        else
                        {
                            if (_voucher_change != string.Empty) _voucher_change += ",";
                            _voucher_change += vc_name;
                            _total_price_sale_vc += vc_detail.total_price_sale;
                        }
                    }
                }
                #endregion

                // Lấy ra tổng số tiền trong giỏ được chọn
                double _total_price_cart = await cart.getCartListByListId(lst_key_id);
                if (result_save_choive)
                {
                    var cart_info = new
                    {
                        total_price_cart = _total_price_cart,
                        total_price_sale_vc = _total_price_sale_vc,
                        total_amount_last = _total_price_cart - _total_price_sale_vc
                    };
                    return Ok(new { status = ResponseType.SUCCESS, msg = "Save success", voucher_change = _voucher_change, noti_voucher = _noti_voucher, link_next_step = "/account/address", cart_info = cart_info });
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("[FR] web save-cart-choice.json frontend  loi khi save lst_key_id =" + lst_key_id);
                    return Ok(new { status = ResponseType.ERROR, msg = "Hệ thống gặp sự cố. Liên hệ với bộ phần CSKH để được hỗ trợ" });
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("[FR] web save-cart-choice.json " + ex.Message);
                return Ok(new { status = ResponseType.ERROR, msg = "Hệ thống gặp sự cố. Liên hệ với bộ phần CSKH để được hỗ trợ" });
            }
        }

        //[AjaxAuthorize()]
        //[HttpPost("empty-item-cart.json")]
        //[AllowAnonymous]
        //public async Task<IActionResult> emptyItemCart(string cart_id, int label_id)
        //{
        //    try
        //    {
        //        var cart = new ShoppingCarts(configuration);

        //        var cart_result = await cart.emptyCartChoice(cart_id, label_id);

        //        if (cart_result)
        //        {
        //            return Ok(new { status = ResponseType.SUCCESS, msg = "Successfully" });
        //        }
        //        else
        //        {
        //            Utilities.LogHelper.InsertLogTelegram("emptyItemCart failed !!!");
        //            return Ok(new { status = ResponseType.ERROR, msg = "emptyItemCart loi" });
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Utilities.LogHelper.InsertLogTelegram("web emptyItemCart " + ex.Message);
        //        return Ok(new { status = ResponseType.ERROR, msg = ex.ToString() });
        //    }
        //}

    }
}