using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Contants;

namespace WEB.UI.FilterAttribute
{
    public class ValidationOrder : ActionFilterAttribute
    {
        public string[] order_params { get; set; }

        public ValidationOrder(params string[] _order_params)
        {
            order_params = _order_params;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {

            long? address_id = -1;
            int? pay_type = -1;
            string bank_code = string.Empty;
            int bresult = (int)ResponseType.SUCCESS;
            string msg = string.Empty;
            string back_link = context.HttpContext.Request.Path.Value;


            string signInPageUrl = "/account/login-popup/" + System.Net.WebUtility.UrlEncode(back_link); // Chuyển về trang chủ và bật Lightbox login lên


            // string notAuthorizedUrl = "/account/login";
            bool IsAjaxRequest = context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (IsAjaxRequest)
            {
                if (context.ActionArguments.ContainsKey("order_id"))
                {
                    long? order_id = context.ActionArguments["order_id"] as long?;
                    if (order_id <= 0)
                    {
                        bresult = (int)ResponseType.FAILED;
                        msg = "Đơn hàng không hợp lệ. Xin vui lòng liên hệ bộ phận CSKH để được hỗ trợ ";
                    }
                }

                if (context.ActionArguments.ContainsKey("address_id"))
                {
                    address_id = context.ActionArguments["address_id"] as long?;
                }
                else
                {
                    bresult = (int)ResponseType.FAILED;
                    msg = "Địa chỉ không hợp lệ. Xin vui lòng liên hệ bộ phận CSKH để được hỗ trợ";
                }

                if (context.ActionArguments.ContainsKey("pay_type"))
                {
                    pay_type = context.ActionArguments["pay_type"] as int?;
                    switch (pay_type)
                    {
                        case (int)PaymentType.ATM_PAYOO_PAY:
                        case (int)PaymentType.USEXPRESS_BANK:
                            if (context.ActionArguments.ContainsKey("bank_code"))
                            {
                                bank_code = context.ActionArguments["bank_code"] as string;
                                if (bank_code == null)
                                {
                                    bresult = (int)ResponseType.FAILED;
                                    msg = "Bạn chưa chọn ngân hàng thanh toán.";
                                }
                            }
                            break;
                        case (int)PaymentType.VISA_PAYOO_PAY:
                            break;
                        default:
                            bresult = (int)ResponseType.FAILED;
                            msg = "Loại thanh toán không hợp lệ. Xin vui lòng liên hệ bộ phận CSKH để được hỗ trợ";
                            break;
                    }

                }
                else
                {
                    bresult = (int)ResponseType.FAILED;
                    msg = "Bạn chưa chọn loại thanh toán";
                }

                if (bresult != 0)
                {
                    var jsonResult = new JsonResult(new { status = bresult, msg = msg });
                    context.Result = jsonResult;
                }
            }
            else
            {
                context.Result = new RedirectResult(signInPageUrl);
            }
        }
    }
}
