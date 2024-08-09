using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caching.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;

namespace WEB.UI.Controllers.Store
{
    public class CostcoController : Controller
    {
        private readonly IConfiguration Configuration;
        private readonly RedisConn redisService;

        public CostcoController( IConfiguration _Configuration, RedisConn _redisService)
        {            
            Configuration = _Configuration;
            redisService = _redisService;
        }

        [Route("store/costco")]
        [HttpGet]
        public IActionResult CostcoStore()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "getProductGroupChoice  error: " + ex.ToString());
                return Redirect("/Error/2");
            }
        }
       

    }
}