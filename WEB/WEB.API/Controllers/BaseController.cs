using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WEB.API.Common;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        public static string EncryptApi = ReadFile.LoadConfig().EncryptApi;
        public static string QUEUE_KEY_API = ReadFile.LoadConfig().QUEUE_KEY_API;


        public Dictionary<string, string> ResponseApi(string status, string msg, string token, string execute_time)
        {
            try
            {
                var result = new Dictionary<string, string>
                        {
                            {"status",status},
                            {"msg", msg},
                            {"token",token},
                            {"execute_time",execute_time}
                        };
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }       


        
    }
}