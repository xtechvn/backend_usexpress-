using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using Utilities.ViewModels;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : BaseController
    {
        public IConfiguration _configuration;
       
        public MailController(IConfiguration Configuration)
        {
            _configuration = Configuration;

        }
        [Route("send-email")]
        public ActionResult SendEmail(string token)
        {
            JArray objParr = null;
            try
            {
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {
                    //Lấy dữ liệu:
                    string receive_email = objParr[0]["receive_email"].ToString();
                    string email_title = objParr[0]["email_title"].ToString();
                    string email_body = objParr[0]["email_body"].ToString();
                    string[] cc_email = objParr[0]["cc_email"].ToString().Split(",")[0] != "" ? objParr[0]["cc_email"].ToString().Split(",") : null;
                    string[] bcc_email = objParr[0]["bcc_email"].ToString().Split(",")[0]!=""? objParr[0]["bcc_email"].ToString().Split(",") : null;
                    EmailAccountModel account = new EmailAccountModel()
                    {
                        Host = _configuration.GetValue<string>("SMTP_Bot_Email:host"),
                        Port = _configuration.GetValue<int>("SMTP_Bot_Email:port"),
                        Email= _configuration.GetValue<string>("SMTP_Bot_Email:username"),
                        Password= _configuration.GetValue<string>("SMTP_Bot_Email:password"),
                        Display_name= _configuration.GetValue<string>("SMTP_Bot_Email:display_name")
                    };
                    //Trả kết quả:
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        sent_status =  EmailHelper.SendEmail(account,receive_email, email_title, email_body, cc_email, bcc_email),
                        msg = "Send Request to Mail Server Success !!!"
                    });
                }
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
                LogHelper.InsertLogTelegram("api: email/send-email  " + ex.Message);
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Error on Excution !!!"
                });
            }
        }
    }
}
