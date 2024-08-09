using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Utilities;
using Utilities.Contants;
using WEB.UI.Controllers.Carts;
using WEB.UI.Controllers.Voucher.Base;
using WEB.UI.FilterAttribute;

namespace WEB.UI.Controllers.Voucher
{
    [Route("[controller]")]
    public class VoucherController : Controller
    {
        private readonly IConfiguration configuration;

        public VoucherController(IConfiguration _Configuration)
        {
            configuration = _Configuration;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="voucher_name"></param>
        /// <param name="label_id"></param>
        /// <param name="lst_key_cart_id"></param>
        /// <param name="voucher_choice_id">Tính ra tổng số tiền được giảm</param>
        /// <returns></returns>
        [Route("apply.json")]
        [HttpPost]
        public async Task<IActionResult> getVoucher(string voucher_search, int label_id, string lst_key_cart_id)
        {
            string token_tele = configuration["telegram_log_error_fe:Token"];
            string group_id_tele = configuration["telegram_log_error_fe:GroupId"];
            string KEY_TOKEN_API = configuration["KEY_TOKEN_API"];
            string domain_us_api_new = configuration["url_api_usexpress_new"] + "api/voucher/apply.json";
            try
            {
                string email = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "EMAILID").Value.ToString();

                // save cart choice
                if (!string.IsNullOrEmpty(lst_key_cart_id))
                {
                    var cart = new ShoppingCarts(configuration);
                    string cart_id = cart.GetCartId(this.HttpContext);
                    var result_save_choive = await cart.saveProductChoice(lst_key_cart_id, cart_id);
                }

                // apply voucher
                var voucher = new VoucherService(domain_us_api_new, email, voucher_search, token_tele, group_id_tele, label_id, KEY_TOKEN_API);
                var response_detail = await voucher.getPriceSaleVoucher();
                if (response_detail != null)
                {

                    double total_price_sale_vc = response_detail.total_price_sale;

                    if (total_price_sale_vc > 0 && response_detail.status == (int)ResponseType.SUCCESS && (response_detail.rule_type != VoucherRuleType.AMZ_DISCOUNT_FPF))
                    {
                        return Ok(new { status = response_detail.status, msg = response_detail.msg_response, data = response_detail });
                    }
                    else
                    {
                        return Ok(new { status = (int)ResponseType.FAILED, msg = response_detail.msg_response });
                    }
                }
                else
                {
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Mã voucher không hợp lệ" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(token_tele, group_id_tele, "getVoucher error: " + ex.ToString() + " voucher_name =" + voucher_search);
                return Ok(new { status = (int)ResponseType.FAILED, msg = "Mã voucher không hợp lệ" });
            }
        }


        [Route("get-list.json")]
        [HttpPost]
        public async Task<IActionResult> getListVoucherPublic()
        {
            string token_tele = configuration["telegram_log_error_fe:Token"];
            string group_id_tele = configuration["telegram_log_error_fe:GroupId"];
            string KEY_TOKEN_API = configuration["KEY_TOKEN_API"];

            try
            {
                string domain_us_api_new = configuration["url_api_usexpress_new"] + "api/voucher/get-list-public.json";
                var voucher = new VoucherService(domain_us_api_new, "", "", token_tele, group_id_tele, -1, KEY_TOKEN_API);
                var response = await voucher.getVoucherListPublic();
                if (response != null)
                {
                    return Ok(new { status = (int)ResponseType.SUCCESS, data = response });
                }
                else
                {
                    return Ok(new { status = (int)ResponseType.EMPTY });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(token_tele, group_id_tele, "getListVoucherPublic error: " + ex.ToString());
                return Ok(new { status = (int)ResponseType.ERROR });
            }
        }

        /// <summary>
        /// Tính tổng số tiền được giảm của voucher sau khi đã chọn xong
        /// </summary>
        /// <returns></returns>
      
        [Route("approve-voucher.json")]
        [AjaxAuthorize()]
        [HttpPost]
        public async Task<IActionResult> calculatePriceSale(string voucher_selected, int label_id, string lst_key_cart_id)
        {
            string token_tele = configuration["telegram_log_error_fe:Token"];
            string group_id_tele = configuration["telegram_log_error_fe:GroupId"];
            string KEY_TOKEN_API = configuration["KEY_TOKEN_API"];
            string domain_us_api_new = configuration["url_api_usexpress_new"] + "api/voucher/apply.json";
            string _msg = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(voucher_selected))
                {
                    int total_prod = 0;
                    double _total_price_sale_vc = 0;
                    string email = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "EMAILID").Value.ToString();

                    #region Save Product Choice
                    if (string.IsNullOrEmpty(lst_key_cart_id))
                    {
                        return Ok(new { status = (int)ResponseType.FAILED, msg = "Bạn vẫn chưa chọn sản phẩm nào để mua." });
                    }

                    var cart = new ShoppingCarts(configuration);
                    var save_cart = await cart.saveProductChoice(lst_key_cart_id, email);
                    if (!save_cart)
                    {
                        LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[FR approve-voucher.json ] calculatePriceSale error: save cart error lst_key_cart_id=" + lst_key_cart_id + "- email = " + email);
                        return Ok(new { status = (int)ResponseType.FAILED, msg = "Mã voucher không hợp lệ" });
                    }

                    #endregion

                    if (!string.IsNullOrEmpty(lst_key_cart_id))
                    {
                        total_prod = await cart.getTotalCartsByUser(this.HttpContext);
                    }

                    var arr_voucher_choice = voucher_selected.Split(",");
                    var obj_voucher_detail = new List<object>();
                    string msg_check_voucher = string.Empty;
                    for (int i = 0; i <= arr_voucher_choice.Length - 1; i++)
                    {
                        string vc_item = arr_voucher_choice[i].Trim();

                        var voucher = new VoucherService(domain_us_api_new, email, vc_item, token_tele, group_id_tele, label_id, KEY_TOKEN_API);
                        var response = await voucher.getPriceSaleVoucher();
                        if (response.rule_type == VoucherRuleType.AMZ_DISCOUNT_FPF)
                        {
                            if (total_prod <= 1)
                            {
                                response.total_price_sale = 0;
                                msg_check_voucher += "Mã voucher " + response.voucher_name + " chỉ được áp dụng khi bạn mua từ 2 sản phẩm trở lên với store Amazon";
                                LogHelper.InsertLogTelegram(token_tele, group_id_tele, "getListVoucherPublic(hacker) _msg: " + _msg);
                            }
                        }
                        if (response.status != (int)ResponseType.SUCCESS)
                        {
                            msg_check_voucher = msg_check_voucher != string.Empty ? ", " : "";
                            msg_check_voucher += response.msg_response;
                        }
                        else
                        {
                            var vc_detail = new
                            {
                                vc_name = vc_item,
                                price_sale = response.total_price_sale,
                                rule_type = response.rule_type
                            };
                            obj_voucher_detail.Add(vc_detail);

                            _total_price_sale_vc += response.total_price_sale;
                        }
                    }

                    // Lấy ra tổng số tiền trong giỏ được chọn
                    double _total_price_cart = await cart.getCartListByListId(lst_key_cart_id);
                    // Tính tổng số tiền sau giảm trong giỏ được check 
                    double _total_amount_last = _total_price_cart - _total_price_sale_vc;


                    return Ok(new { status = msg_check_voucher != string.Empty ? (int)ResponseType.FAILED : (int)ResponseType.SUCCESS, msg = msg_check_voucher != string.Empty ? msg_check_voucher : _msg, total_price_cart = _total_price_cart, total_price_sale_vc = _total_price_sale_vc, total_amount_last = Math.Min(_total_price_cart, _total_amount_last), voucher_detail = obj_voucher_detail });
                }
                else
                {
                    return Ok(new { status = (int)ResponseType.EMPTY });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(token_tele, group_id_tele, "getListVoucherPublic error: " + ex.ToString());
                return Ok(new { status = (int)ResponseType.ERROR });
            }
        }

    }
}