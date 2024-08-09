using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackagesShipmentController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PackagesShipmentController(IConfiguration Configuration)
        {
            _configuration = Configuration;
        }
        [HttpPost("get-list.json")]
        public async Task<ActionResult> GetNewItems(string token)
        {
            int status = (int)ResponseType.FAILED;
            string message = "Failed";
            dynamic data = null;
            try
            {
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _configuration["KEY_TOKEN_API"]))
                {

                }
                else
                {
                    message = "Token Invalid";
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("get-list.json - PackagesShipmentController. Error with Token: " + token + "\nError: " + ex.Message);
                status = (int)ResponseType.ERROR;
                message = "Error On Excution";
            }
            return Ok(new
            {
                status = status,
                msg = message,
                data = data
            });
        }
    }
}
