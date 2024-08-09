using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Affiliate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUglify.Helpers;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.UI.Common;
using WEB.UI.Controllers.Carts;
using WEB.UI.Controllers.Client;
using WEB.UI.FilterAttribute;
using WEB.UI.Service;
using WEB.UI.ViewModels;
using static WEB.UI.Service.renderViewToString;
using Constants = WEB.UI.Common.Constants;

namespace WEB.UI.Controllers
{
    public class ClientController : BasePeopleController
    {
        private readonly IClientRepository Repository;

        private readonly IConfiguration Configuration;
        private readonly IHttpContextAccessor HttpContextAccessor;

        public ClientController(IClientRepository clientRepository, IViewRenderService _ViewRenderService, IConfiguration _Configuration, IHttpContextAccessor httpContextAccessor)
        {
            Repository = clientRepository;

            Configuration = _Configuration;
            HttpContextAccessor = httpContextAccessor;
        }


        [Route("client/login-show")]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> showLogin()
        {
            try
            {
                string view_login = "Components/Login/Default";
                string view_register = "Components/Register/Default";

                var logon_model = new ClientLogOnViewModel();
                var register_model = new ClientViewModel
                {
                    ClientId = -1,
                    SourceRegisterId = (int)(ClientSourceType.SourceType.PC)
                };

                view_login = await this.RenderViewToStringAsync(view_login, logon_model);
                view_register = await this.RenderViewToStringAsync(view_register, register_model);

                return Json(new { status = (int)ResponseType.SUCCESS, msg = "successfully", view_login_client = view_login, view_register_client = view_register });

                // string view_register = await this.RenderViewToStringAsync("Components/Login/LoginRegister", logon_model);
                //return Json(new { status = (int)ResponseType.SUCCESS, msg = "successfully", view_login_client = viewFromCurrentController });
                //string view_register = await this.RenderViewToStringAsync("Components/Login/Default", logon_model);
                //return Json(new { status = (int)ResponseType.SUCCESS, msg = "successfully", view_login_client = view_register });

            }
            catch (Exception ex)
            {
                //  LogHelper.InsertLogTelegram("showLogin==> " + ex.ToString());
                return Json(new { status = (int)ResponseType.ERROR, msg = "Hệ thống đang trong quá trình bảo trì. Xin vui lòng quay lại sau" });
            }
        }

        /// <summary>
        /// Đăng ký tài khoản
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Email = model.Email.Trim();
                    string session_otp_name = Constants.CodeVerifyEmail + model.Email;

                    #region Check code verify
                    //if (string.IsNullOrEmpty(HttpContext.Session.GetString(session_otp_name)))
                    //{
                    //    LogHelper.InsertLogTelegram("loi ma xac thuc: " + JsonConvert.SerializeObject(model));
                    //    return Json(new { status = (int)ResponseType.FAILED, msg = "Mã số xác thực đã hết hiệu lực. Xin vui lòng gửi lại mã xác thực hoặc liên hệ bộ phận CSKH để được hỗ trợ." });
                    //}
                    //else
                    //{
                    //    string _code = (string)HttpContext.Session.GetString(session_otp_name);
                    //    if (_code.Trim() != model.CodeVerify.Trim())
                    //    {
                    //        LogHelper.InsertLogTelegram("loi ma xac thuc: email = " + model.Email + " - session parram code= " + _code + " , code khach hang nhap = " + model.CodeVerify.Trim());
                    //        return Json(new { status = (int)ResponseType.FAILED, msg = "Mã số xác thực không khớp. Xin vui lòng gửi lại mã xác thực hoặc liên hệ bộ phận CSKH để được hỗ trợ." });
                    //    }
                    //    else
                    //    {
                    //        // dk thành công xong Hủy session
                    //        HttpContext.Session.Remove(session_otp_name);
                    //    }
                    //}
                    #endregion

                    long client_id = await Repository.addNewClient(model);
                    if (client_id > 0)
                    {
                        #region Khởi tạo địa chỉ mặc định
                        var address_model = new AddressClientViewModel
                        {
                            ClientId = client_id,
                            ReceiverName = model.ClientName,
                            Phone = model.Phone,
                            ProvinceId = "-1",
                            DistrictId = "-1",
                            WardId = "-1",
                            Address = string.Empty,
                            Status = (int)StatusType.BINH_THUONG,
                            IsActive = true,
                            CreatedOn = DateTime.Now
                        };
                        long address_id = await Repository.addNewAddressClient(address_model);
                        if (address_id <= 0)
                        {
                            LogHelper.InsertLogTelegram("Khởi tạo địa chỉ thất bại" + JsonConvert.SerializeObject(model));
                        }

                        #endregion

                        model.ClientId = client_id;
                        // Tự động login
                        var userToken = genTokenJwt(GetUserClaims(model));
                        if (userToken != null)
                        {
                            //Save token in session object
                            HttpContext.Session.SetString(Constants.JWToken, userToken);

                            // Save Cookie                   
                            Response.Cookies.Append(
                                Constants.USEXPRESS_ACCESS_TOKEN,
                                userToken,
                                new CookieOptions()
                                {
                                    Path = "/",
                                    HttpOnly = false,
                                    Secure = false,
                                    Expires = DateTime.Now.AddYears(1)
                                }
                            );

                            #region Push Queue để mapping sang hệ thống cũ
                            var client = new ClientService(Configuration);
                            client.pushClientToQueue(client_id, -1);
                            #endregion


                            return Json(new { status = (int)ResponseType.SUCCESS, msg = "Chúc mừng bạn đã đăng ký thành công !" });
                        }
                        else
                        {
                            LogHelper.InsertLogTelegram("Error create new user: " + JsonConvert.SerializeObject(model));
                            return Json(new { status = (int)ResponseType.ERROR, msg = "Lỗi xảy ra trong quá trình khởi tạo tài khoản. Liên hệ với bộ phận CSKH để được hỗ trợ" });
                        }

                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("Có 1 khách hàng khởi tạo thất bại thông tin đăng ký" + JsonConvert.SerializeObject(model));
                        return Json(new { status = (int)ResponseType.ERROR, msg = "Hệ thống đang trong quá trình bảo trì. Xin vui lòng quay lại sau" });
                    }
                }
                else
                {

                    string sError =
                        ViewData.ModelState.Values.SelectMany(modelState => modelState.Errors)
                            .Aggregate("", (current, item) => current + ("<span>" + item.ErrorMessage + "</span>"));
                    LogHelper.InsertLogTelegram("Loi gap phai khi khach hang dang ky tai khoan" + sError);
                    return Json(new { status = (int)ResponseType.FAILED, msg = sError });

                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("client add new" + ex.ToString() + JsonConvert.SerializeObject(model));
                return Json(new { status = (int)ResponseType.ERROR, msg = "Hệ thống đang trong quá trình bảo trì. Xin vui lòng quay lại sau" });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(ClientLogOnViewModel model)
        {
            try
            {
                string host = HttpContextAccessor.HttpContext.Request.Host.Value;
                

                //Get user details for the user who is trying to login - JRozario
                var user = Repository.getClientDetailByEmail(model.Email.Trim()).Result;
                //Authenticate User, Check if its a registered user in DB
                if (user == null)
                {
                    return Json(new { status = (int)ResponseType.FAILED, msg = "Email không tồn tại trong hệ thống. Xin vui lòng thử lại" });
                }

                // check password
                string password_ui = PresentationUtils.Encrypt(model.Password);
                if (model.Email.ToLower() == user.Email.Trim().ToLower() && password_ui == user.Password)
                {
                    var cart = new ShoppingCarts(Configuration);
                    string shopping_cart_id = cart.GetCartId(this.HttpContext);

                    var userToken = genTokenJwt(GetUserClaims(user));

                    //Save token in session object
                    HttpContext.Session.SetString(Constants.JWToken, userToken);

                    // Save Cookie                   
                    Response.Cookies.Append(
                        Constants.USEXPRESS_ACCESS_TOKEN,
                        userToken,
                        new CookieOptions()
                        {
                            Path = "/",
                            HttpOnly = false,
                            Secure = false,
                            Expires = DateTime.Now.AddYears(1)
                        }
                    );

                    #region MAPPING CART
                    // Check cart có sp ko

                    //get cartslist
                    int total_carts = await cart.getTotalCartsByUser(this.HttpContext);
                    if (total_carts > 0)
                    {
                        //mirror                    
                        bool result = await cart.MappingToCart(shopping_cart_id, user.Email);
                        if (!result)
                        {
                            LogHelper.InsertLogTelegram("Mapping cart that bai khi dang nhap thanh cong tai khoan voi email =" + user.Email);
                        }
                    }
                    #endregion
                    return Json(new { status = (int)ResponseType.SUCCESS, access_token = userToken, msg = "Chào mừng bạn đã quay trở lại với Website Usexpress.vn", back_link = System.Net.WebUtility.UrlDecode(model.return_url) });
                }
                else
                {
                    return Json(new { status = (int)ResponseType.FAILED, msg = "Email hoặc mật khẩu không đúng. Xin vui lòng thử lại" });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Login" + ex.ToString() + JsonConvert.SerializeObject(model));
                return Json(new { status = (int)ResponseType.ERROR, msg = "Email hoặc mật khẩu không đúng. Xin vui lòng thử lại" });
            }
        }

        [HttpGet]
        [Route("account/login-popup/{back_link}")]
        public IActionResult LoginAjax(string back_link)
        {
            try
            {
                string ReturnUrl = string.IsNullOrEmpty(back_link) ? "/" : System.Net.WebUtility.UrlDecode(back_link);
                if (User.Identity.IsAuthenticated)
                {
                    return Redirect(ReturnUrl);
                }
                else
                {
                    ViewBag.is_show_popup_login = true;
                    return View("~/Views/Home/Index.cshtml");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("LoginAjax" + ex.ToString());
                return Redirect("/");
            }
        }

        public IActionResult Logoff()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete(Constants.USEXPRESS_ACCESS_TOKEN);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            try
            {
                var model = new ForgotPasswordViewModel();
                return PartialView("/Views/Client/ForgotPassword.cshtml", model);
            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("ForgotPassword==> " + ex.ToString());
                return Json(new { status = (int)ResponseType.ERROR, msg = "Hệ thống đang trong quá trình bảo trì. Xin vui lòng quay lại sau" });
            }
        }

        [HttpGet]
        [Route("account/change-password/{token}")]
        public async Task<IActionResult> ChangePassword(string token)
        {
            try
            {
                JArray objParr = null;
                token = token.Replace("-", "+").Replace("_", "/");
                //detect token
                if (CommonHelper.GetParamWithKey(token, out objParr, Constants.prive_key_forget_password))
                {
                    string receive_email = objParr[0]["receive_email"].ToString();
                    DateTime create_date = Convert.ToDateTime(objParr[0]["create_date"]);
                    DateTime current_date = DateTime.Now;
                    var total_days = (current_date - create_date).TotalDays;
                    if (total_days > 7) // expired link trong 7 days. Vuot qua bao loi
                    {
                        return PartialView("/Views/Client/ExpireChangePassword.cshtml");
                    }
                    else
                    {

                        var model = new ClientChangePasswordViewModel
                        {
                            Email = receive_email
                        };
                        return PartialView("/Views/Client/ChangePassword.cshtml", model);
                    }
                }
                else
                {
                    return PartialView("/Views/Client/ExpireChangePassword.cshtml");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangePassword==> " + ex.ToString() + " token= " + token);
                return Redirect("/Error/404");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitChangePassword([FromForm]ClientChangePasswordViewModel model)
        {
            try
            {
                bool isValid = Repository.checkEmailExist(model.Email, -1).Result;
                if (isValid)
                {
                    return Json(new { status = (int)ResponseType.FAILED, msg = "Email của bạn không tồn tại trong hệ thống, xin vui lòng nhập lại" });
                }
                else
                {
                    var client = new ClientService(Configuration);
                    model.PasswordNew = EncodeHelpers.MD5Hash(model.PasswordNew);
                    model.ConfirmPasswordNew = EncodeHelpers.MD5Hash(model.ConfirmPasswordNew);
                    var rs = await client.UpdateClientChangePassword(model);
                    if (rs)
                    {
                        return Json(new { status = (int)ResponseType.SUCCESS, msg = "Đổi mật khẩu thành công" });
                    }
                    else
                    {
                        LogHelper.InsertLogTelegram("HttpPost ChangePassword==> doi mk that bai cho  Email = " + model.Email);
                        return Json(new { status = (int)ResponseType.SUCCESS, msg = "Đổi mật khẩu thất bại. Xin vui lòng liên hệ với bộ phận CSKH để được trợ giúp" });
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("HttpPost ChangePassword==> " + ex.ToString() + " clientid= " + model.ClientId);
                return Json(new { status = (int)ResponseType.ERROR });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            try
            {
                //validation email có tồn tại trong hệ thống không
                bool isValid = Repository.checkEmailExist(model.EmailFogot, -1).Result;
                if (isValid)
                {
                    return Json(new { status = (int)ResponseType.FAILED, msg = "Email của bạn không tồn tại trong hệ thống, xin vui lòng nhập lại" });
                }

                var j_param = new Dictionary<string, string>
                {
                    {"receive_email",model.EmailFogot },
                    {"create_date",DateTime.Now.ToString()}
                };
                string body_text = await System.IO.File.ReadAllTextAsync(@"wwwroot\Templates\ForgotPassword.html", System.Text.Encoding.UTF8);
                string Domain = Request.GetDisplayUrl().Split("/C").First();
                string email_title = "[UsExpress.VN] Yêu cầu lấy lại mật khẩu " + model.EmailFogot + ".";
                string token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), Constants.prive_key_forget_password);
                string link_change = Domain + "/account/change-password/" + token.Replace("+", "-").Replace("/", "_");
                body_text = body_text.Replace("${Customer}", model.EmailFogot.Split('@').First().ToString()).Replace("${Link_chage}", link_change)
                    .Replace("${date_time}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
                    .Replace("${device_ip}", HttpContext.Connection.RemoteIpAddress.ToString());
                string email_body = body_text;

                var client = new ClientService(Configuration);
                var respose = await client.sendMail(model.EmailFogot, email_title, email_body, "", "quynhluong@usexpress.vn");
                if (!respose)
                {
                    LogHelper.InsertLogTelegram("ForgotPassword==> Lỗi ko gửi được mật khẩu tới cho email: " + model.EmailFogot + ". Link token truy cập là: " + link_change);
                }
                return Json(new { status = (int)ResponseType.SUCCESS });

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ForgotPassword==> " + ex.ToString());
                return Json(new { status = (int)ResponseType.ERROR, msg = "Hệ thống đang trong quá trình bảo trì. Xin vui lòng quay lại sau" });
            }
        }

        /// <summary>
        /// Sử dụng: @User.FindFirst("Name").Value để get ngoài view các trường này
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private IEnumerable<Claim> GetUserClaims(ClientViewModel user)
        {
            IEnumerable<Claim> claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, user.ClientName),
                new Claim("USERID", user.ClientId.ToString()),
                new Claim("EMAILID", user.Email),
                new Claim("PHONE", user.Phone),
                new Claim("PASSWORD", user.Password),
                new Claim("REFERRAL_ID", user.ReferralId??"")
            };
            return claims;
        }


        #region VALIDATION
        [HttpPost]
        public JsonResult checkPhoneExist(string Phone, long ClientId)
        {
            try
            {
                bool isValid = Repository.checkPhoneExist(Phone, ClientId).Result;

                return Json(isValid);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }
        [HttpPost]
        public JsonResult checkEmailExist(string Email, long ClientId)
        {
            try
            {
                bool isValid = Repository.checkEmailExist(Email, ClientId).Result;
                return Json(isValid);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }

        /// <summary>
        /// Kiem tra password old cua user
        /// </summary>
        /// <param name="ClientId"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult checkPasswordOldExist(string PasswordOld)
        {
            try
            {
                string _password_old = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "PASSWORD").Value;

                return Json(EncodeHelpers.MD5Hash(PasswordOld) == _password_old);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }


        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("client/otp-codes")]
        public async Task<IActionResult> sendCodeVeirfy(string email)
        {
            string session_otp_name = Constants.CodeVerifyEmail + email;
            string code_verify_email = string.Empty;
            try
            {

                //if (!string.IsNullOrEmpty(HttpContext.Session.GetString(session_otp_name)))
                //{
                //    Thread.Sleep(1000);
                //    code_verify_email = HttpContext.Session.GetString(Constants.CodeVerifyEmail);

                //    //resend code toi email khach hang
                //    var obj_result = Repository.sendMailCode(email, code_verify_email);
                //    DateTime current_date = DateTime.Now;
                //    return Json(new { status = (int)ResponseType.SUCCESS, msg = "Mã số xác thực đã được gửi tới địa chỉ email của bạn", year = current_date.Year, month = current_date.Month, day = current_date.Day, hours = current_date.Hour, minutes = current_date.Minute, seconds = current_date.Second + 59 });
                //}

                // Check email
                if (StringHelpers.IsValidEmail(email))
                {
                    // Kiểm tra email này có trong hệ thống chưa
                    bool is_valid = await Repository.checkEmailExist(email.Trim(), -1);
                    if (!is_valid)
                    {
                        return Json(new { status = (int)ResponseType.EXISTS, msg = "Email này đã được sử dụng" });
                    }
                    else
                    {
                        //send code toi email khach hang
                        var obj_result = Repository.sendMailCode(email, code_verify_email);

                        if (obj_result.Result == string.Empty)
                        {
                            LogHelper.InsertLogTelegram("sendCodeVeirfy==> Mã code verify cho email: " + email + " không sinh ra được code cho khách hàng");
                            return Json(new { status = (int)ResponseType.FAILED, msg = "Hệ thống gặp sự cố. Liên hệ với bộ phận CSKH để được hỗ trợ" });
                        }
                        else
                        {
                            //lưu session trong 1 khoảng time
                            //HttpContext.Session.SetString(session_otp_name, obj_result.Result);

                            DateTime current_date = DateTime.Now;
                            return Json(new { status = (int)ResponseType.SUCCESS, msg = "Mã số xác thực đã được gửi tới địa chỉ email của bạn", year = current_date.Year, month = current_date.Month, day = current_date.Day, hours = current_date.Hour, minutes = current_date.Minute, seconds = current_date.Second + 59 });
                        }
                    }
                }
                else
                {
                    return Json(new { status = (int)ResponseType.FAILED, msg = "Định dạng email không hợp lệ" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("sendCodeVeirfy==> " + ex.ToString());
                return Json(new { status = (int)ResponseType.ERROR, msg = "Hệ thống gặp sự cố. Liên hệ với bộ phận CSKH để được hỗ trợ" });
            }
        }

        /// <summary>
        /// Chọn địa chỉ giao hàng
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>

        [HttpGet]
        [AjaxAuthorize()]
        [Route("account/address")]
        //[AjaxAuthorize(new[] { "RoleName", "AnotherRoleName" })]        
        public async Task<IActionResult> getAddressReceiver()
        {
            try
            {
                //Check cart có được chọn không ?
                var cart = new ShoppingCarts(Configuration);
                string cart_id = cart.GetCartId(this.HttpContext);
                //get cartslist
                var cart_model = await cart.getCartListByUser(cart_id, -1, "", 1);
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

                long client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);
                var address_list = await Repository.GetClientAddressList(client_id);
                return View("~/Views/Payment/AddressReceiverList.cshtml", address_list);

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getAddressReceiver==> " + ex.ToString());
                return Content("Hệ thống gặp sự cố");
            }
        }

        /// <summary>
        /// Thêm mới địa chỉ / sửa địa chỉ giao hàng
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AjaxAuthorize()]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> addNewAddress([FromForm]AddressReceiverOrderViewModel model)
        {
            string token = string.Empty;
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new { status = ResponseType.FAILED, msg = "Xin vui lòng nhập đầy đủ thông tin" });
                }
                var stt = Lib.CorrectAddressModel(model);
                if (stt != null)
                {
                    return Ok(new { status = ResponseType.FAILED, msg = stt });
                }
                string KEY_TOKEN_API = Configuration["KEY_TOKEN_API"];
                var address = new AddressClientViewModel
                {
                    Id = model.Id,
                    ClientId = model.ClientId,
                    ReceiverName = model.ReceiverName,
                    IsActive = model.IsActive,
                    Phone = model.Phone,
                    WardId = model.WardId,
                    ProvinceId = model.ProvinceId,
                    DistrictId = model.DistrictId,
                    CreatedOn = DateTime.Now,
                    Status = (int)(StatusType.BINH_THUONG),
                    Address = model.Address,
                    order_id = model.order_id
                };

                string j_param = "{'address_item':'" + Newtonsoft.Json.JsonConvert.SerializeObject(address) + "'}";
                token = CommonHelper.Encode(j_param, KEY_TOKEN_API);
                string token_tele = Configuration["telegram_log_error_fe:Token"];
                string group_id_tele = Configuration["telegram_log_error_fe:GroupId"];

                var connect_api_us = new ConnectApi(Configuration["url_api_usexpress_new"] + "api/Client/add-new-address.json", token_tele, group_id_tele, token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status == ResponseType.SUCCESS.ToString())
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        msg = _msg,
                        id = JsonParent[0]["address_id"].ToString()
                    });
                }
                else
                {
                    //LogHelper.InsertLogTelegram(token_tele, group_id_tele, "addNewAddress error from frontend: " + _msg.ToString() + " token =" + token);
                    return Ok(new { status = status, msg = _msg });
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[FR] addNewAddress==> model = " + JsonConvert.SerializeObject(model) + " " + ex.ToString() + "--token: " + token);
                return Json(new { status = (int)ResponseType.ERROR, msg = "Hệ thống gặp sự cố. Liên hệ với bộ phận CSKH để được hỗ trợ" });
            }
        }

        [AjaxAuthorize()]
        [HttpPost]
        [Route("client/address/{address_id}/{order_id}.html")]
        public async Task<IActionResult> editAddressClient(long address_id, long order_id)
        {
            try
            {
                string html_detail = string.Empty;
                #region LOCATION

                var obj_province = new List<Province>();
                var obj_district = new List<District>();
                var obj_ward = new List<Ward>();
                var address_receiver = new AddressReceiverOrderViewModel();
                #endregion
                long client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);

                if (address_id > 0)
                {

                    var address_detail = await Repository.getAddressClientById(address_id);

                    address_receiver = new AddressReceiverOrderViewModel
                    {
                        Id = address_id,
                        ReceiverName = address_detail.ReceiverName,
                        Phone = address_detail.Phone,
                        ProvinceId = address_detail.ProvinceId,
                        DistrictId = address_detail.DistrictId,
                        WardId = address_detail.WardId,
                        Address = address_detail.Address,
                        ProvinceListReceiver = obj_province,
                        DistrictListReceiver = obj_district,
                        WardListReceiver = obj_ward,
                        IsActive = address_detail.IsActive,
                        ClientId = client_id,
                        order_id = order_id
                    };
                    html_detail = address_detail == null ? string.Empty : await this.RenderViewToStringAsync("Components/addressReceiverAddNew/default", address_receiver);
                }
                else
                {
                    // add new
                    address_receiver = new AddressReceiverOrderViewModel
                    {
                        ProvinceId = "-1",
                        DistrictId = "-1",
                        WardId = "-1",
                        ProvinceListReceiver = obj_province,
                        DistrictListReceiver = obj_district,
                        WardListReceiver = obj_ward,
                        ClientId = client_id
                    };
                    html_detail = await this.RenderViewToStringAsync("Components/addressReceiverAddNew/default", address_receiver);
                }

                return Json(new { status = (int)ResponseType.SUCCESS, render_address_detail = html_detail });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[FR] editAddressClient==> address_id = " + address_id + " " + ex.ToString());
                return Json(new { status = (int)ResponseType.ERROR, msg = "Hệ thống gặp sự cố. Liên hệ với bộ phận CSKH để được hỗ trợ" });
            }
        }


        [HttpPost]
        [Route("client/location.json")]
        public async Task<IActionResult> getLocation(int type, string id)
        {
            try
            {
                var service = new ClientService(Configuration);
                var data = await service.getLocationData(type, id);
                return Json(new { status = (int)ResponseType.SUCCESS, result = data });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getProvince==>[type = " + type + ", id = " + id + "] " + ex.ToString());
                return Json(new { status = (int)ResponseType.ERROR, msg = "Hệ thống gặp sự cố. Liên hệ với bộ phận CSKH để được hỗ trợ" });
            }
        }

        [AjaxAuthorize()]
        [HttpPost]
        [Route("client/address/delete/{address_id}.json")]
        public async Task<IActionResult> deleteAddress(long address_id)
        {
            long client_id = -1;
            try
            {
                string KEY_TOKEN_API = Configuration["KEY_TOKEN_API"];
                client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);
                string j_param = "{'address_id':'" + address_id + "','client_id':'" + client_id + "'}";
                string token = CommonHelper.Encode(j_param, KEY_TOKEN_API);
                string token_tele = Configuration["telegram_log_error_fe:Token"];
                string group_id_tele = Configuration["telegram_log_error_fe:GroupId"];

                var connect_api_us = new ConnectApi(Configuration["url_api_usexpress_new"] + "api/Client/delete-address.json", token_tele, group_id_tele, token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status == ResponseType.SUCCESS.ToString())
                {
                    #region Push Queue để mapping sang hệ thống cũ
                    var client = new ClientService(Configuration);
                    client.pushClientToQueue(client_id, -1);
                    #endregion

                    return Ok(new
                    {
                        status = ResponseType.SUCCESS,
                        msg = _msg
                    });
                }
                else
                {
                    LogHelper.InsertLogTelegram(token_tele, group_id_tele, "deleteAddress error from frontend: " + _msg.ToString() + " token =" + token);
                    return Ok(new { status = ResponseType.EMPTY, msg = "deleteAddress error " });
                }

            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("deleteAddress==>[address_id = " + address_id + ", client_id = " + client_id + "] " + ex.ToString());
                return Json(new { status = (int)ResponseType.ERROR, msg = "Hệ thống gặp sự cố. Liên hệ với bộ phận CSKH để được hỗ trợ" });
            }
        }

        [AjaxAuthorize()]
        [HttpPost]
        [Route("client/address-receiver/detail.json")]
        public async Task<IActionResult> getAddressDefaultByClientId(long address_id)
        {
            long client_id = -1;
            try
            {
                client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);
                var address_list = await Repository.GetClientAddressList(client_id);


                if (address_list.Count() > 0)
                {

                    var address_default = new AddressModel(); //address_id > 0 ? address_list.FirstOrDefault(x => x.Id == address_id) : address_list.FirstOrDefault(x => x.IsActive);
                    if (address_id > 0)
                    {
                        address_default = address_list.FirstOrDefault(x => x.Id == address_id);
                    }
                    else if (address_list.Count() == 1)
                    {
                        address_default = address_list.FirstOrDefault();
                    }
                    else
                    {
                        address_default = address_list.FirstOrDefault(x => x.IsActive);
                        if (address_default == null)
                        {
                            address_default = address_list.FirstOrDefault();
                        }
                    }

                    return Ok(new { status = ResponseType.SUCCESS, result = address_default });
                }
                else
                {
                    LogHelper.InsertLogTelegram("client/address-receiver/detail.json==> chua co dia chi nao trong clientid= " + client_id);
                    return Ok(new { status = ResponseType.EMPTY, msg = "Xin vui lòng bổ sung địa chỉ giao hàng" });
                }

            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("Lỗi client/address-receiver/detail.json" + ex.ToString());
                return Json(new { status = (int)ResponseType.ERROR, msg = "Hệ thống gặp sự cố. Liên hệ với bộ phận CSKH để được hỗ trợ" });
            }
        }

        [HttpPost]
        [Route("client/survey/add-new")]
        public async Task<IActionResult> addNewFeedBackClient(int function_id, string feedback)
        {
            try
            {
                if (function_id < 0)
                {
                    return Json(new { status = (int)ResponseType.FAILED, msg = "Vui lòng chọn tính năng mà bạn muốn góp ý." });
                }
                if (feedback == null || feedback.Trim() == "" || feedback.Replace(" ", "") == "")
                {
                    return Json(new { status = (int)ResponseType.FAILED, msg = "Vui lòng không để trống phần Nội dung góp ý cải thiện." });
                }
                if (feedback.Length < 3)
                {
                    return Json(new { status = (int)ResponseType.FAILED, msg = "Nhận xét phải từ 3 ký tự trở lên." });
                }
                else if (feedback.Length > 4000)
                {
                    return Json(new { status = (int)ResponseType.FAILED, msg = "Nhận xét không được vượt quá 4000 ký tự." });
                }
                var email = User.Identity.Name;
                var remoteIpAddress = (email == null || email == "") ? @"IP-" + HttpContext.Connection.RemoteIpAddress.ToString() : email;

                var client = new ClientService(Configuration);
                var result = await client.addNewFeedBackClient(function_id, feedback.Replace("\"", " ").Replace("'", " "), remoteIpAddress);
                return Json(new { status = (result == true ? (int)ResponseType.SUCCESS : (int)ResponseType.ERROR) });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[FR-ClientController] addNewFeedBackClient" + ex.ToString());
                return Json(new { status = (int)ResponseType.ERROR });
            }
        }

        /// <summary>
        /// Quản lý thông tin khách hàng
        /// </summary>
        /// <param name="function_id"></param>
        /// <param name="feedback"></param>
        /// <returns></returns>
        [HttpGet]
        [AjaxAuthorize()]
        [Route("thong-tin-tai-khoan")]
        public async Task<IActionResult> ClientManagerInfo()
        {
            try
            {
                var obj_province = new List<Province>();
                var obj_district = new List<District>();
                var obj_ward = new List<Ward>();

                long client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);
                var client_detail = await Repository.getClientDetail(client_id);
                if (client_detail != null)
                {
                    var model = new ClientInfoViewModel()
                    {
                        ClientId = client_detail.ClientId,
                        ReceiverName = client_detail.ClientName,
                        Email = client_detail.Email,
                        Phone = client_detail.Phone,
                        FullAddress = client_detail.FullAddress,
                        BirthdayDayList = Lib.BindNumberToDropDownlist(1, 32, ""),
                        BirthdayMonthList = Lib.BindNumberToDropDownlist(1, 13, ""),
                        BirthdayYearList = Lib.BindNumberToDropDownlist(DateTime.Now.AddYears(-80).Year, DateTime.Now.AddYears(-5).Year, ""),
                        BirthdayDay = client_detail.DateOfBirth.Day,
                        BirthdayMonth = client_detail.DateOfBirth.Month,
                        BirthdayYear = client_detail.DateOfBirth.Year,
                        ProvinceId = client_detail.ProvinceId,
                        DistrictId = client_detail.DistrictId,
                        WardId = client_detail.WardId,
                        ProvinceListReceiver = obj_province,
                        DistrictListReceiver = obj_district,
                        WardListReceiver = obj_ward,
                        Gender = client_detail.Gender,
                        Id = client_detail.AddressId
                    };

                    return View("/Views/Client/ManagerAccount.cshtml", model);
                }
                else
                {
                    LogHelper.InsertLogTelegram("[FR-ClientController] ClientManagerInfo: Ko tim thay khach hang clientid = " + client_id);
                    return Content("Hệ thống đang bảo trì. Xin vui lòng quay lại sau");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[FR-ClientController] ClientManagerInfo" + ex.ToString());
                return Content("Hệ thống đang bảo trì. Xin vui lòng quay lại sau");
            }
        }

        /// <summary>
        /// Cập nhật thông tin tài khoản
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AjaxAuthorize()]
        public async Task<IActionResult> UpdateClientInfo([FromForm]ClientInfoViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Ok(new { status = ResponseType.FAILED, msg = "Xin vui lòng nhập đầy đủ thông tin" });
                }
                string KEY_TOKEN_API = Configuration["KEY_TOKEN_API"];

                // get client_id login
                model.ClientId = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);

                // Mã hóa mật khẩu
                if (!string.IsNullOrEmpty(model.PasswordOld) && !string.IsNullOrEmpty(model.PasswordNew) && !string.IsNullOrEmpty(model.ConfirmPasswordNew))
                {
                    model.PasswordNew = EncodeHelpers.MD5Hash(model.PasswordNew);
                    model.PasswordOld = EncodeHelpers.MD5Hash(model.PasswordOld);
                    model.ConfirmPasswordNew = EncodeHelpers.MD5Hash(model.ConfirmPasswordNew);
                }
                else 
                {
                    model.PasswordOld = string.Empty;
                    model.PasswordNew = string.Empty;
                }

                string j_param = "{'client_info':'" + Newtonsoft.Json.JsonConvert.SerializeObject(model) + "'}";
                string token = CommonHelper.Encode(j_param, KEY_TOKEN_API);
                string token_tele = Configuration["telegram_log_error_fe:Token"];
                string group_id_tele = Configuration["telegram_log_error_fe:GroupId"];

                var connect_api_us = new ConnectApi(Configuration["url_api_usexpress_new"] + "api/client/update-client-info.json", token_tele, group_id_tele, token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string msg_api = JsonParent[0]["msg"].ToString();

                if (status == ResponseType.SUCCESS.ToString())
                {
                    return Ok(new
                    {
                        status = ResponseType.SUCCESS,
                        msg = "Update Successfully !!!"
                    });
                }
                else
                {
                    LogHelper.InsertLogTelegram(token_tele, group_id_tele, "[FR-ClientController] update-client-info error from frontend: msg_api = " + msg_api.ToString() + " token =" + token);
                    return Ok(new { status = ResponseType.EMPTY, msg = "update-client-info error ", msg_api = msg_api });
                }

            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("[FR-ClientController] update-client-info: = " + ex.ToString() + " model = " + JsonConvert.SerializeObject(model));
                return Content("Hệ thống đang bảo trì. Xin vui lòng quay lại sau");
            }
        }

        [HttpGet]
        [AjaxAuthorize()]
        [Route("so-dia-chi")]
        public async Task<IActionResult> ShowAddressManager()
        {
            try
            {
                long client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);
                string ReceiverName = HttpContext.User.Identities.ToList()[0].Name;
                var address_list = await Repository.GetClientAddressList(client_id);

                ViewBag.ReceiverName = ReceiverName;
                return View("~/Views/Client/ManagerAddress.cshtml", address_list);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[FR-ClientController] ShowAddressManager" + ex.ToString());
                return Redirect("/Error/404");
            }
        }


        [AjaxAuthorize()]
        [HttpPost]
        [Route("client/aff/register.json")]
        public async Task<IActionResult> RegisterAffiliate()
        {
            try
            {
                long client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);

                var client = new ClientService(Configuration);
                string referral_id = await client.registerAffiliate(client_id);
                if (referral_id != "")
                {
                    return Json(new { status = (int)ResponseType.SUCCESS, referral_id = referral_id });
                }
                else
                {
                    LogHelper.InsertLogTelegram("HttpPost RegisterAffiliate==> dang ky aff that bai cho client_id = " + client_id);
                    return Json(new { status = (int)ResponseType.FAILED });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[FR-ClientController] client/aff/register.json" + ex.ToString());
                return Json(new { status = (int)ResponseType.ERROR });
            }
        }

        [HttpGet]
        [AjaxAuthorize()]
        [Route("tao-link-gioi-thieu")]
        public async Task<IActionResult> createAffiliate()
        {
            try
            {
                long client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);
                var client = new ClientService(Configuration);
                var detail = await client.getClientDetail(client_id);

                string ReceiverName = HttpContext.User.Identities.ToList()[0].Name;
                bool is_register_aff = detail.IsRegisterAffiliate;

                ViewBag.ReceiverName = ReceiverName;
                ViewBag.is_register_aff = is_register_aff;
                ViewBag.domain = Request.GetDisplayUrl().ToLower().Split("/t").First();
                return View("~/Views/Client/CreateAffiliate.cshtml");
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[FR-ClientController] createAffiliate" + ex.ToString());
                return Redirect("/Error/404");
            }
        }

        [HttpPost]
        [AjaxAuthorize()]
        [Route("client/aff/add-link")]
        public async Task<IActionResult> addNewAffiliate(string link_aff,string referral_first_id)
        {
            try
            {
                long client_id = Convert.ToInt64(HttpContext.User.Claims.FirstOrDefault(x => x.Type == "USERID").Value);
                string REFERRAL_ID = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "REFERRAL_ID").Value;
                link_aff = System.Web.HttpUtility.UrlDecode(link_aff);

                if (string.IsNullOrEmpty(link_aff))
                {
                    return Json(new { status = (int)ResponseType.FAILED, msg = "Link giới thiệu không được để trống" });
                }
                else
                {
                    string Domain = Request.GetDisplayUrl().ToLower().Split("/c").First();
                    if (link_aff.IndexOf(Domain) == -1)
                    {
                        LogHelper.InsertLogTelegram("[FR-ClientController] add-link-aff. khach hang nhập link_aff =" + link_aff + " không hợp lệ");
                        return Json(new { status = (int)ResponseType.FAILED, msg = "Link giới thiệu không hợp lệ" });
                    }
                }

                if (string.IsNullOrEmpty(REFERRAL_ID))
                {
                    if (string.IsNullOrEmpty(referral_first_id)) {
                        LogHelper.InsertLogTelegram("[FR-ClientController] add-link-aff. khach hang co client_id =" + client_id + " chưa đc đăng ký aff");
                        return Json(new { status = (int)ResponseType.FAILED, msg = "Bạn chưa đăng ký Affiliate. Xin vui lòng liên hệ với bộ phận CSKH để được hỗ trợ" });
                    }
                    else
                    {
                        REFERRAL_ID = referral_first_id;
                    }                    
                }


                var client = new ClientService(Configuration);
                link_aff = link_aff + "?utm_source=usexpress&utm_medium=us_" + REFERRAL_ID;

                var obj_link = new MyAffiliateLinkViewModel
                {
                    client_id = client_id,
                    create_date = DateTime.Now,
                    link_aff = link_aff
                };
                var rs = await client.addNewAffiliate(obj_link);

                return Json(new { status = rs, link_aff = link_aff });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[FR-ClientController] add-link-aff" + ex.ToString());
                return Json(new { status = (int)ResponseType.ERROR, msg = "Link giới thiệu không hợp lệ" });
            }
        }


    }
}