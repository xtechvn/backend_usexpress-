using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caching.RedisWorker;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyModel;
using Microsoft.VisualBasic;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Superpower.Model;
using Utilities;
using Utilities.Contants;
using WEB.API.Common;
using WEB.API.Model.Carts;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : Controller
    {

        private readonly IConfiguration configuration;
        private readonly IVoucherRepository voucherRepository;
        private readonly IOrderRepository orderRepository;
        private readonly RedisConn _RedisService;
        public VoucherController(IConfiguration _Configuration, IVoucherRepository _VoucherRepository, IOrderRepository _OrderRepository, RedisConn redisService)
        {
            configuration = _Configuration;
            voucherRepository = _VoucherRepository;
            orderRepository = _OrderRepository;
            _RedisService = redisService;
        }

        [HttpPost("test.json")]
        public async Task<IActionResult> TestVoucher(string voucher_name, string email_user_current, int label_id)
        {
            string j_param = "{'voucher_name': '" + voucher_name + "', 'email_user_current': '" + email_user_current + "', 'label_id': " + label_id + "}";
            string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

            return RedirectToAction("ApplyVoucher", new { token = token });
        }
        /// <summary>
        /// Hàm này sẽ lấy ra số tiền được giảm của Voucher
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("apply.json")]
        public async Task<IActionResult> ApplyVoucher(string token)
        {
            JArray objParr = null;
            bool is_voucher_valid = false;
            double _total_price_sale = 0;
            double rate_current = 0;
            try
            {
                if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    LogHelper.InsertLogTelegram("[API] VoucherController - ApplyVoucher Token invalid!!! => token= " + token.ToString() + " voucher name = " + objParr.ToString());
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Token invalid !!!" });
                }
                else
                {
                    string voucher_name = objParr[0]["voucher_name"].ToString();
                    string email_user_current = objParr[0]["email_user_current"].ToString();
                    int label_id = Convert.ToInt32(objParr[0]["label_id"]);
                    // string cart_id_choice = objParr[0]["cart_id_choice"].ToString(); // list cart user chọn


                    var lib = new Service.Lib.Common(configuration, _RedisService);
                    var cart = new ShoppingCarts(configuration);
                    rate_current = Convert.ToDouble(lib.getRateCache());
                    #region VALIDATION
                    //1. Check hợp lệ
                    if (voucher_name.Length < 3 && email_user_current.IndexOf("@") == -1)
                    {
                        return Ok(new { status = (int)ResponseType.EXISTS, msg = "Mã " + voucher_name + " không hợp lệ. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    }

                    //2. Check null
                    var voucher = await voucherRepository.getDetailVoucher(voucher_name);
                    if (voucher == null)
                    {
                        return Ok(new { status = (int)ResponseType.EXISTS, msg = "Mã " + voucher_name + " không tồn tại trong hệ thống. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    }

                    // 3. Hiệu lực voucher
                    if (voucher.RuleType != VoucherRuleType.AMZ_DISCOUNT_FPF)
                    {
                        DateTime current_date = DateTime.Now;
                        if (!(current_date >= voucher.Cdate && current_date <= voucher.EDate))
                        {
                            return Ok(new { status = (int)ResponseType.FAILED, msg = "Mã " + voucher_name + " đã hết hiệu lực. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                        }
                    }

                    // 4. Kiểm tra nhóm khách hàng thỏa mãn voucher
                    string[] group_list_user = string.IsNullOrEmpty(voucher.GroupUserPriority) ? null : voucher.GroupUserPriority.Split(',');
                    //1 Kiểm tra user đăng nhập có nằm trong nhóm user này không                       
                    if (group_list_user != null)
                    {
                        var find_email = Array.FindAll(group_list_user, s => s.Equals(email_user_current));
                        if (find_email.Count() == 0)
                        {
                            return Ok(new { status = (int)ResponseType.FAILED, msg = "Mã " + voucher_name + " không hợp lệ. Vui lòng liên hệ với bộ phận CSKH. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                        }
                    }

                    //5. Kiểm tra voucher này có được giới hạn nhãn hàng không
                    if (voucher.StoreApply != null)
                    {
                        if (voucher.StoreApply != "-1")
                        {
                            // Kiểm tra store mã voucher này có nằm trong store cart thanh toán không ?
                            string store_current_cart = "," + label_id + ",";
                            string store_apply_voucher = "," + voucher.StoreApply + ",";
                            if (store_apply_voucher.IndexOf(store_current_cart) == -1)
                            {
                                return Ok(new { status = (int)ResponseType.FAILED, msg = "Mã " + voucher_name + " không áp dụng cho nhãn hàng này. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                            }
                        }
                    }
                    #endregion

                    //6. Thưc hiện Apply rule theo type
                    // switch (voucher.RuleType)
                    // {
                    #region GIAM_GIA_VOUCHER_TREN_PHI_MUA_HO
                    // case VoucherRuleType.GIAM_GIA_VOUCHER_TREN_PHI_MUA_HO:

                    //1. Phí được giảm giá bao gồm:                        
                    // Phí luxury: được loại đi từ giỏ hàng rồi
                    // Phí giảm % trên phí mua hộ của 1 sản phẩm có giá cao nhất.
                    //int total_item_cart = cart.GetCount(cart_id, store_id); // tong so san pham trong gio hang

                    double total_fee_not_luxury = 0; //                         
                    double limit_total_discount = voucher.LimitTotalDiscount ?? 1000000; // Số tiền tối đa được giảm lay tu db

                    #region VALIDATION VOUCHER

                    // Nếu voucher được set is_limit_voucher = 1 (true) nghĩa là voucher sẽ được giới hạn số lần sử dụng ở trường limituser
                    // Nếu is_limit_voucher  = 0 (false) thì sẽ hiểu là: mỗi 1 tài khoản sẽ được giới hạn số lần sử dụng ở trường limituser
                    if (voucher.RuleType != VoucherRuleType.AMZ_DISCOUNT_FPF)
                    {
                        if (voucher.IsLimitVoucher == true)
                        {
                            var total_used = orderRepository.GetTotalVoucherUse(voucher.Id, ""); // Lay  ra so lan voucher da duoc su dung
                            if (total_used == -1)
                            {
                                return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " đã hết số lần sử dụng. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                            }
                            else if (total_used >= voucher.LimitUse)
                            {
                                return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " đã hết số lần sử dụng. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                            }
                        }
                        else
                        {
                            var total_client_use = orderRepository.GetTotalVoucherUse(voucher.Id, email_user_current); // lay ra so lan voucher da duoc su dung cua 1 user
                            if (total_client_use >= voucher.LimitUse)
                            {
                                return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " đã hết số lần sử dụng với tài khoản của bạn. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                            }

                        }
                    }

                    // Kiểm tra giới hạn số tiền của đơn hàng
                    if (voucher.MinTotalAmount > 0)
                    {
                        var shopping_cart = new ShoppingCarts(configuration);
                        var cart_result = await shopping_cart.FindCartSelectedByEmail(email_user_current, -1);

                        double total_order_amount_vnd = cart_result.Sum(x => x.amount_last_vnd);
                        if (total_order_amount_vnd < voucher.MinTotalAmount)
                        {
                            string _msg = "Để sử dụng mã này.Tổng giá trị đơn hàng của bạn phải trên " + (voucher.MinTotalAmount ?? 1000000).ToString("N0") + " đ";
                            return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = _msg + ". Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                        }
                    }
                    #endregion




                    // Tổng tiền phí mua hộ ko tính phí luxury với giỏ hàng chưa được khởi tạo đơn hàng
                    // is_max_price_product: true là lấy ra phí mua hộ cao nhất của sản phẩm  | false: là lấy ra tổng phí mua hộ trừ luxury

                    if ((voucher.IsMaxPriceProduct ?? false))
                    {
                        //Lấy ra tổng phí mua hộ của sản phẩm có giá cao nhất
                        // is_free_luxury: true là được miễn phí cả luxury. false là ko được miễn phí
                        //is_price_first_pound_fee: true: số tiền first pound fee của sản phẩm có giá cao nhất
                        total_fee_not_luxury = await cart.getMaxPriceBuyNoluxuryInCart(label_id, email_user_current, voucher.IsFreeLuxury ?? false, voucher.IsMaxFee ?? false, voucher.IsPriceFirstPoundFee ?? false);
                    }
                    else
                    {
                        // lay ra tong phi mua ho tru luxury
                        total_fee_not_luxury = await cart.getTotalFeeNoluxuryInCart(label_id, email_user_current);
                    }

                    if (total_fee_not_luxury > 0)
                    {
                        //1. Chiết khấu trên phí mua hộ đã trừ. Tính ra số tiền sau khi được trừ    
                        double percent = Convert.ToDouble(voucher.PriceSales);

                        //  Chiết khấu phí mua hộ  đã trừ luxury
                        double price_sale_off = 0;
                        switch (voucher.Unit)
                        {
                            case UnitType.PHAN_TRAM:
                                //Tinh số tiền giảm theo %
                                price_sale_off = Convert.ToDouble(total_fee_not_luxury * (percent / 100)) * rate_current; // so tien duoc giam tu khi mua ho theo don vi %
                                break;
                            case UnitType.VIET_NAM_DONG:
                                price_sale_off = Convert.ToDouble(voucher.PriceSales); //Math.Min(Convert.ToDouble(voucher.LimitTotalDiscount), total_fee_not_luxury) ;
                                break;

                            default:
                                return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Mã " + voucher_name + " không hợp lệ. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                        }

                        // Nếu giảm 100% thì sẽ giảm toàn bộ chi phí mua hộ của sp đó trừ luxury
                        price_sale_off = price_sale_off == 0 ? Convert.ToDouble(total_fee_not_luxury) : price_sale_off;

                        // Nếu số tiền chiết khấu vượt quá 1 triệu thì sẽ chỉ được 1 triệu
                        _total_price_sale = Math.Min(limit_total_discount, price_sale_off);

                        if (_total_price_sale > 0)
                        {
                            is_voucher_valid = true; // ghi nhan trang thai hop le cho voucher
                        }
                        else
                        {
                            LogHelper.InsertLogTelegram("[API] VoucherController - ApplyVoucher: Số tiền giảm k hợp lệ, token = " + token + "--_total_price_sale = " + _total_price_sale);
                        }
                    }
                    else
                    {
                        return Ok(new { status = ((int)ResponseType.FAILED).ToString(), msg = "Hiện tại giỏ hàng của bạn trống. Xin vui lòng chọn sản phẩm" });
                    }
                    // break;
                    #endregion

                    //    default:
                    //        return Ok(new { status = (int)ResponseType.FAILED, msg = "Mã " + voucher_name + " không tồn tại. Vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    //}


                    return Ok(new { status = is_voucher_valid ? ((int)ResponseType.SUCCESS).ToString() : ((int)ResponseType.FAILED).ToString(), msg = "success", rule_type = voucher.RuleType, unit = voucher.Unit, price_sale = voucher.PriceSales, expire_date = (voucher.EDate ?? DateTime.Now).ToString("dd-MM-yyyy"), desc = voucher.Description, voucher_id = voucher.Id, total_price_sale = Math.Round(_total_price_sale) });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[API] VoucherController - ApplyVoucher ex =  " + ex + " token=" + token.ToString());
                return Ok(new { status = (int)ResponseType.ERROR, msg = "Token invalid !!!" });
            }
        }

        /// <summary>
        /// Lấy ra ds voucher public
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("get-list-public.json")]
        public async Task<IActionResult> getVoucherPublic(string token)
        {
            try
            {
                JArray objParr = null;
                // string j_param = "{'key_slave': 'get-list-public'}";
                //token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                string cache_key = CacheType.VOUCHER_PUBLIC;

                if (!CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    LogHelper.InsertLogTelegram("[API get-list-public.json] VoucherController - getVoucherPublic Token invalid!!! => token= " + token.ToString() + " getVoucherPublic = " + objParr.ToString());
                    return Ok(new { status = (int)ResponseType.FAILED, msg = "Token invalid !!!" });
                }
                else
                {
                    var j_data = await _RedisService.GetAsync(cache_key, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    // Kiểm tra có trong cache ko
                    if (j_data != null)
                    {
                        return Content(j_data); // trả thẳng từ cache ra luôn
                    }

                    string key_slave = objParr[0]["key_slave"].ToString();
                    var list_vc = await voucherRepository.getListVoucherPublic(true);
                    if (list_vc.Count() > 0)
                    {

                        var response = list_vc.Select(n => new
                        {
                            voucher_name = n.Code,
                            voucher_id = n.Id,
                            desc = n.Description,
                            discount = n.Unit == UnitType.PHAN_TRAM ? Convert.ToInt32(n.PriceSales) + "%" : (Convert.ToInt32(n.PriceSales) / 1000) + "k",
                            unit = n.Unit,
                            from_date = n.Cdate,
                            to_date = n.EDate,
                            expire_date = (n.EDate ?? DateTime.Now).ToString("dd-MM-yyyy"),
                            rule_type = n.RuleType
                        });

                        // Tạo Object
                        var obj_response = new
                        {
                            status = (int)ResponseType.SUCCESS,
                            data = response,
                            msg = "success"
                        };

                        // Có data set lên Redis
                        _RedisService.Set(cache_key, JsonConvert.SerializeObject(obj_response), Convert.ToInt32(configuration["Redis:Database:db_common"]));

                        return Content(JsonConvert.SerializeObject(obj_response));
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("[API] VoucherController - getVoucherPublic hiện tại hệ thống chưa có mã voucher public nào:  + token = " + token.ToString());
                        return Ok(new { status = (int)ResponseType.EMPTY, msg = "voucher empty !" });
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[API] VoucherController - getVoucherPublic ex =  " + ex + " token=" + token.ToString());
                return Ok(new { status = (int)ResponseType.ERROR, msg = "Token invalid !!!" });
            }
        }


    }
}