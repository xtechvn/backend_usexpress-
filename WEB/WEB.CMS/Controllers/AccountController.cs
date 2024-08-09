using DAL;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OtpNet;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Utilities.Common;
using Utilities.Contants;
using WEB.CMS.Common;
using WEB.CMS.Customize;
using WEB.CMS.Models;

namespace WEB.CMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _UserRepository;
        private readonly IMFARepository _mFARepository;
        private readonly IConfiguration _configuration;
        public AccountController(IUserRepository userRepository, IMFARepository mFARepository, IConfiguration configuration)
        {
            _UserRepository = userRepository;
            _mFARepository = mFARepository;
            _configuration = configuration;
        }
        /// <summary>
        /// Function Login cũ
        /// </summary>
        /// <param name="requestPath"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login(string requestPath = null)
        {
            var model = new AccountModel()
            {
                ReturnUrl = requestPath,
            };
            return View(model);
        }
        /// <summary>
        /// Function thực hiện xử lý đăng nhập bước 1.
        /// </summary>
        /// <param name="entity"> Thông tin đăng nhập</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AccountModel entity)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var model = await _UserRepository.CheckExistAccount(entity);
                    if (model != null)
                    {
                        //Kiểm tra trạng thái tài khoản
                        if (model.Entity.Status != 0)
                        {
                            ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa");
                        }
                        else
                        {
                            if (_configuration["Config:On_QC_Environment"] == "1")
                            {
                                // Tạo Cookie
                                var claims = new List<Claim>();
                                claims.Add(new Claim(ClaimTypes.NameIdentifier, model.Entity.Id.ToString()));
                                claims.Add(new Claim(ClaimTypes.Name, model.Entity.UserName));
                                claims.Add(new Claim(ClaimTypes.Email, model.Entity.Email));
                                claims.Add(new Claim(ClaimTypes.Role, string.Join(",", model.RoleIdList)));
                                var claimsIdentity = new ClaimsIdentity(
                                    claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                var authProperties = new AuthenticationProperties
                                {
                                    AllowRefresh = true,
                                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
                                    IsPersistent = entity.RememberMe,
                                };
                                await HttpContext.SignInAsync(
                                    CookieAuthenticationDefaults.AuthenticationScheme,
                                    new ClaimsPrincipal(claimsIdentity),
                                    authProperties);
                                //Logging successful login to cms:
                                LoggingActivity.AddLog(model.Entity.Id, model.Entity.UserName, (int)LogActivityType.LOGIN_CMS, "Login user " + model.Entity.UserName + " thành công trong môi trường QC.");
                                // Redirect
                                return Redirect(entity.ReturnUrl ?? "/");
                            }
                            else
                            {
                                // Lấy thông tin:
                                Mfauser mfa_detail = await _mFARepository.get_MFA_DetailByUserID(model.Entity.Id);
                                // Nếu tài khoản chưa thiết lập 2FA,  redirect về trang setup 2fa.
                                if (mfa_detail == null)
                                {
                                    var create_2fa_status = await ForceCreateMFA(model);
                                    if (create_2fa_status)
                                    {
                                        var user_detail = new Dictionary<string, string>();
                                        user_detail.Add("UserID", model.Entity.Id.ToString());
                                        user_detail.Add("UserName", model.Entity.UserName);
                                        user_detail.Add("time_exprire", DateTime.Now.ToUniversalTime().AddHours(1).ToString());
                                        TempData["token"] = Utilities.CommonHelper.Encode(JsonConvert.SerializeObject(user_detail), ReadFile.LoadConfig().KEY_TOKEN_API);
                                        return RedirectToAction("Setup", "Account");
                                    }
                                    else
                                        ModelState.AddModelError(string.Empty, "Không thể đăng nhập vào lúc này, vui lòng liên hệ với bộ phận kỹ thuật.");

                                }
                                //Nếu đã thiết lập 2FA:
                                else
                                {
                                    if (mfa_detail.Status == 0)
                                    {
                                        var user_detail = new Dictionary<string, string>();
                                        user_detail.Add("UserID", model.Entity.Id.ToString());
                                        user_detail.Add("UserName", model.Entity.UserName);
                                        user_detail.Add("time_exprire", DateTime.Now.ToUniversalTime().AddHours(1).ToString());
                                        TempData["token"] = Utilities.CommonHelper.Encode(JsonConvert.SerializeObject(user_detail), ReadFile.LoadConfig().KEY_TOKEN_API);
                                        return RedirectToAction("Setup", "Account");
                                    }
                                    else
                                    {
                                        //-- Nếu không bắt được url trước khi chuyển hướng tới login, redirect về trang chủ sau khi đăng nhập thành công
                                        if (entity.ReturnUrl == null) entity.ReturnUrl = "/";
                                        //-- Tạo temp data rồi đẩy sang trang otp, tránh tình trạng url quá dài và sử dụng lại url để login chỉ bằng otp.
                                        TempData["token"] = Utilities.CommonHelper.Encode(JsonConvert.SerializeObject(entity), ReadFile.LoadConfig().KEY_TOKEN_API);
                                        //-- Điều hướng về trang OTP
                                        return RedirectToAction("Authenticate", "Account");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Thông tin sai:
                        ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác");
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Login", "Account");
            }
            return View(entity);
        }

        /// <summary>
        /// Trả view OTP
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Authenticate(string requestPath = null)
        {
            try
            {
                // Kiểm tra nếu temp data có token
                if (TempData.ContainsKey("token"))
                {
                    var token = TempData["token"].ToString();
                    MFAAccountViewModel mFARecord = new MFAAccountViewModel()
                    {
                        MFA_token = token,
                        ReturnUrl = requestPath
                    };
                    return View(mFARecord);
                }
                else
                {
                    //Không được redirect từ trang login, hoặc "login" không truyền token sang, redirect lại về trang
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Account");
            }
        }
        /// <summary>
        /// Xử lý OTP từ google authenticator
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Authenticate(MFAAccountViewModel record)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string text_error = "Mã OTP nhập vào không chính xác, vui lòng thử lại.";
                    if (record.MFA_Code == null)
                    {
                        //Nếu sai OTP hoặc quá hạn:
                        ModelState.AddModelError(string.Empty, text_error);
                    }
                    else
                    {
                        // Kiểm tra lại token.
                        AccountModel entity = JsonConvert.DeserializeObject<AccountModel>(Utilities.CommonHelper.Decode(record.MFA_token, ReadFile.LoadConfig().KEY_TOKEN_API));
                        var model = await _UserRepository.CheckExistAccount(entity);
                        // Nếu đúng thông tin
                        if (model != null)
                        {
                            // Lấy thông tin MFA
                            var mfa_record = await _mFARepository.get_MFA_DetailByUserID(model.Entity.Id);
                            string otp_code = "";
                            int remainingTime = 0;
                            switch (record.MFA_type)
                            {
                                case 0:
                                    {
                                        // Convert sang byte
                                        var bytes = Base32OTPEncoding.ToBytes(mfa_record.SecretKey.Trim());
                                        // Xây mã OTP
                                        var totp = new Totp(bytes);
                                        otp_code = totp.ComputeTotp();
                                        remainingTime = totp.RemainingSeconds();
                                        text_error = "Mã OTP nhập vào không chính xác, vui lòng thử lại.";
                                        var compare_otp = await CompareOTP(mfa_record.UserId, record.MFA_Code, record.MFA_timenow);
                                        //Kiểm tra OTP tạo ra với OTP gửi lên và thời gian
                                        if (compare_otp)
                                        {
                                            // Tạo Cookie
                                            var claims = new List<Claim>();
                                            claims.Add(new Claim(ClaimTypes.NameIdentifier, model.Entity.Id.ToString()));
                                            claims.Add(new Claim(ClaimTypes.Name, model.Entity.UserName));
                                            claims.Add(new Claim(ClaimTypes.Email, model.Entity.Email));
                                            claims.Add(new Claim(ClaimTypes.Role, string.Join(",", model.RoleIdList)));
                                            var claimsIdentity = new ClaimsIdentity(
                                                claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                            var authProperties = new AuthenticationProperties
                                            {
                                                AllowRefresh = true,
                                                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
                                                IsPersistent = entity.RememberMe,
                                            };
                                            await HttpContext.SignInAsync(
                                                CookieAuthenticationDefaults.AuthenticationScheme,
                                                new ClaimsPrincipal(claimsIdentity),
                                                authProperties);
                                            //Logging successful login to cms:
                                            LoggingActivity.AddLog(model.Entity.Id, model.Entity.UserName, (int)LogActivityType.LOGIN_CMS, "Login user "+model.Entity.UserName+ " thành công với OTP: " + record.MFA_Code);
                                            // Redirect
                                            return Redirect(entity.ReturnUrl ?? "/");
                                        }
                                        else
                                        {
                                            //Nếu sai OTP hoặc quá hạn:
                                            ModelState.AddModelError(string.Empty, text_error);
                                        }
                                    }
                                    break;
                                case 1:
                                    {
                                        /*
                                        otp_code = mfa_record.BackupCode;
                                        record.MFA_timenow = DateTime.Now.ToUniversalTime().AddSeconds(remainingTime);
                                        text_error = "Mã dự phòng nhập vào không chính xác, vui lòng thử lại hoặc liên hệ với bộ phận kỹ thuật.";

                                        string backupcode_input = BackupCodeMD5FromInput(record.MFA_Code, mfa_record);
                                        if (backupcode_input == mfa_record.BackupCode.Trim())
                                        {
                                            // Tạo Cookie
                                            var claims = new List<Claim>();
                                            claims.Add(new Claim(ClaimTypes.NameIdentifier, model.Entity.Id.ToString()));
                                            claims.Add(new Claim(ClaimTypes.Name, model.Entity.UserName));
                                            claims.Add(new Claim(ClaimTypes.Email, model.Entity.Email));
                                            claims.Add(new Claim(ClaimTypes.Role, string.Join(",", model.RoleIdList)));
                                            var claimsIdentity = new ClaimsIdentity(
                                                claims, CookieAuthenticationDefaults.AuthenticationScheme);
                                            var authProperties = new AuthenticationProperties
                                            {
                                                AllowRefresh = true,
                                                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1),
                                                IsPersistent = entity.RememberMe,
                                            };
                                            await HttpContext.SignInAsync(
                                                CookieAuthenticationDefaults.AuthenticationScheme,
                                                new ClaimsPrincipal(claimsIdentity),
                                                authProperties);
                                            // Redirect
                                            return Redirect(entity.ReturnUrl ?? "/");
                                        }
                                        else
                                        {
                                            //Nếu sai OTP hoặc quá hạn:
                                            ModelState.AddModelError(string.Empty, text_error);
                                        }
                                        */
                                    }
                                    break;
                                default:
                                    {
                                        text_error = "Vui lòng chọn phương thức xác thực khác và thử lại.";
                                        ModelState.AddModelError(string.Empty, text_error);
                                    }
                                    break;
                            }

                        }
                    }

                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
                return View(record);
            }
            catch (Exception)
            {
                return RedirectToAction("Login", "Account");
            }
        }
        [HttpGet]
        public async Task<IActionResult> Setup()
        {
            try
            {
                if (TempData.ContainsKey("token"))
                {
                    string token = TempData["token"].ToString();
                    var user_json = Utilities.CommonHelper.Decode(token, ReadFile.LoadConfig().KEY_TOKEN_API);
                    Dictionary<string, string> user_detail = JsonConvert.DeserializeObject<Dictionary<string, string>>(user_json);
                    if (user_detail != null || user_detail["UserID"] != null || user_detail["UserName"] != null || user_detail["time_exprire"] != null)
                    {
                        DateTime time_exprire = DateTime.Parse(user_detail["time_exprire"]);
                        if (time_exprire > DateTime.Now.ToUniversalTime())
                        {
                            var mfa_record = await _mFARepository.get_MFA_DetailByUserID(Convert.ToInt32(user_detail["UserID"]));
                            ViewBag.token = token;
                            ViewBag.QRCodeUri = GenerateQRCode(mfa_record);
                            ViewBag.SecretKey = FormatKey(mfa_record.SecretKey);
                            ViewBag.UserName = user_detail["UserName"];
                            ViewBag.Status = mfa_record.Status;
                            return View();
                        }

                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("Login", "Account");
        }
        [HttpPost]
        public async Task<ActionResult> ConfirmMFA(string token)
        {
            try
            {
                var user_json = Utilities.CommonHelper.Decode(token, ReadFile.LoadConfig().KEY_TOKEN_API);
                Dictionary<string, string> user_detail = JsonConvert.DeserializeObject<Dictionary<string, string>>(user_json);
                if (user_detail != null || user_detail["UserID"] != null || user_detail["UserName"] != null || user_detail["time_exprire"] != null)
                {
                    DateTime time_exprire = DateTime.Parse(user_detail["time_exprire"]);
                    if (time_exprire > DateTime.Now.ToUniversalTime())
                    {
                        var mfa_detail = await _mFARepository.get_MFA_DetailByUserID(Convert.ToInt32(user_detail["UserID"]));
                        mfa_detail.Status = 1;
                        var result = await _mFARepository.UpdateAsync(mfa_detail);
                        if (result == "Success")
                        {
                            return new JsonResult(new
                            {
                                status = ResponseType.SUCCESS.ToString(),
                                msg = "Kích hoạt bảo mật 2 lớp thành công."
                            });
                        }

                    }
                }
                return new JsonResult(new
                {
                    status = ResponseType.FAILED.ToString(),
                    msg = "Thao tác không thành công, vui lòng thử lại."
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = ResponseType.ERROR.ToString(),
                    msg = "Có lỗi trong quá trình xử lý, vui lòng liên hệ bộ phận kỹ thuật."
                });
            }
        }
        [HttpPost]
        public async Task<ActionResult> OTPTest(MFAAccountViewModel record)
        {
            try
            {
                string token = record.MFA_token;
                var user_json = Utilities.CommonHelper.Decode(token, ReadFile.LoadConfig().KEY_TOKEN_API);
                Dictionary<string, string> user_detail = JsonConvert.DeserializeObject<Dictionary<string, string>>(user_json);
                int _UserId = Convert.ToInt32(user_detail["UserID"]);
                var mfa_record = await _mFARepository.get_MFA_DetailByUserID(_UserId);
                switch (record.MFA_type)
                {
                    case 0:
                        {
                            bool compare_value = false;
                            // Lấy thông tin MFA
                            // Convert sang byte
                            var bytes = Base32OTPEncoding.ToBytes(mfa_record.SecretKey.Trim());
                            // Xây mã OTP
                            var totp = new Totp(bytes);
                            var otp_code = totp.ComputeTotp();
                            //var remainingTime = totp.RemainingSeconds();
                            //Kiểm tra OTP tạo ra với OTP gửi lên và thời gian
                            if (record.MFA_Code == otp_code)
                            {
                                compare_value = true;
                            }
                            else
                            {
                                compare_value = false;
                            }
                            // var compare_value = await CompareOTP(_UserId, record.MFA_Code, record.MFA_timenow);
                            if (compare_value)
                            {
                                return new JsonResult(new
                                {
                                    status = ResponseType.SUCCESS.ToString(),
                                    msg = "Xác thực thành công đối với mã OTP đã nhập."
                                });
                            }
                            else
                            {
                                return new JsonResult(new
                                {
                                    status = ResponseType.FAILED.ToString(),
                                    msg = "Xác thực không thành công, vui lòng kiểm tra lại cài đặt."
                                });
                            }
                        }
                    case 1:
                        {
                            string backupcode_input = BackupCodeMD5FromInput(record.MFA_Code, mfa_record);
                            if (backupcode_input == mfa_record.BackupCode.Trim())
                            {
                                return new JsonResult(new
                                {
                                    status = ResponseType.SUCCESS.ToString(),
                                    msg = "Xác thực thành công đối với mã dự phòng đã nhập."
                                });
                            }
                            else
                            {
                                return new JsonResult(new
                                {
                                    status = ResponseType.FAILED.ToString(),
                                    msg = "Xác thực không thành công, vui lòng kiểm tra lại mã."
                                });
                            }
                        }
                    default:
                        {
                            return new JsonResult(new
                            {
                                status = ResponseType.FAILED.ToString(),
                                msg = "OTP nhập lên không thuộc phương thức hợp lệ, vui lòng thử lại"
                            });
                        }
                }
            }
            catch (Exception)
            {
                return new JsonResult(new
                {
                    status = ResponseType.FAILED.ToString(),
                    msg = "Lỗi trong quá trình xử lý, vui lòng liên hệ với bộ phận kỹ thuật."
                });
            }
        }
        /// <summary>
        /// Function Đăng xuất
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Reset mật khẩu
        /// </summary>
        /// <param name="EmailOrUserName"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string EmailOrUserName)
        {
            try
            {
                var rs = await _UserRepository.ResetPassword(EmailOrUserName);
                if (rs)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Lấy lại mật khẩu thành công. Vui lòng kiểm tra Email của bạn để lấy mật khẩu"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Email hoặc tên đăng nhập không tồn tại."
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }
        private async Task<bool> CompareOTP(int user_id, string otp_code_input, DateTime time_now)
        {
            try
            {
                // Lấy thông tin MFA
                var mfa_record = await _mFARepository.get_MFA_DetailByUserID(user_id);
                // Convert sang byte
                var bytes = Base32OTPEncoding.ToBytes(mfa_record.SecretKey.Trim());
                // Xây mã OTP
                var totp = new Totp(bytes);
                DateTime time = await Utilities.Common.TimeCorrection.GetCurrentDateTime();
                var otp_code = totp.ComputeTotp(time);
                var remainingTime = totp.RemainingSeconds(time);
                //Kiểm tra OTP tạo ra với OTP gửi lên
                if (otp_code_input == otp_code)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }
        private string BackupCodeMD5FromInput(string input, Mfauser mfa_record)
        {
            try
            {
                MD5 md5_generator = MD5.Create();
                string hash_str = input.Trim() + "_" + mfa_record.UserId.ToString().Trim() + "_" + mfa_record.Username.ToString().Trim() + "_" + mfa_record.UserCreatedYear.ToString().Trim();
                byte[] hash_byte = System.Text.Encoding.ASCII.GetBytes(hash_str);
                string backupcode_input = Base32Encoding.ToString(md5_generator.ComputeHash(hash_byte));
                return backupcode_input.Trim();
            }
            catch (Exception)
            {
                return null;
            }
        }
        private async Task<bool> ForceCreateMFA(UserDetailViewModel client_detail)
        {
            try
            {
                Mfauser new_mfa_record = new Mfauser()
                {
                    UserId = client_detail.Entity.Id,
                    Email = client_detail.Entity.Email.Trim(),
                    Username = client_detail.Entity.UserName.Trim(),
                    SecretKey = "",
                    Status = 1,
                    BackupCode = "",
                    UserCreatedYear = client_detail.Entity.CreatedOn.Value.Year.ToString()
                };
                string secret_key = await GenerateSecretKeyAsync(client_detail.Entity.Id);
                if (secret_key == null)
                {
                    return false;
                }
                new_mfa_record.SecretKey = secret_key.Trim();
                string backupcode = new Random().Next(0, 99999999).ToString(new string('0', 8));
                string backup_code_md5 = BackupCodeMD5FromInput(backupcode, new_mfa_record);
                if (backup_code_md5 == null)
                {
                    return false;

                }
                new_mfa_record.BackupCode = backup_code_md5;
                long result = await _mFARepository.CreateAsync(new_mfa_record);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private async Task<string> GenerateSecretKeyAsync(int user_id)
        {
            try
            {
                SHA256 sHA256 = SHA256.Create();
                var client_detail = await _UserRepository.GetDetailUser(user_id);
                /*Secret Key Generate*/
                string SecretKey = "";
                string random_int_begin = new Random().Next(0, 99999999).ToString(new string('0', 8));
                string random_int_last = new Random().Next(0, 99999999).ToString(new string('0', 8));
                // 12345678_55_minh.nq_11111111_minhnguyen@usexpress.vn
                string base_text = random_int_begin.Trim() + "_" + client_detail.Entity.Id + "_" + client_detail.Entity.UserName.Trim() + "_" + random_int_last.Trim() + "_" + client_detail.Entity.Email.Trim();
                byte[] base_text_in_bytes = System.Text.Encoding.ASCII.GetBytes(base_text);
                byte[] hash_text_sha256 = sHA256.ComputeHash(base_text_in_bytes);
                SecretKey = Base32OTPEncoding.ToString(hash_text_sha256);
                //Get 32 first char from base32 string, as google authenticator secret key length
                SecretKey = SecretKey.Substring(0, 32).Trim();
                return SecretKey.Trim();
            }
            catch (Exception)
            {
                return null;
            }
        }
        private string GenerateQRCode(Mfauser result) //int user_id
        {
            try
            {
                if (result != null)
                {
                    string label_name = "USExCMS-" + result.Username.Trim();
                    string secret_key = result.SecretKey.Trim();
                    string issuer = "US-Express";
                    string otp_auth_url = @"" + "otpauth://totp/" + issuer + ":" + label_name + "?secret=" + secret_key + "&issuer=" + issuer + "";
                    return otp_auth_url;
                }
                return null;

            }
            catch (Exception)
            {
                return null;
            }
        }
        private string FormatKey(string unformattedKey)
        {
            try
            {
                return Regex.Replace(unformattedKey.Trim(), ".{4}", "$0 ");
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
