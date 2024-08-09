using System;

using System.Threading.Tasks;
using Caching.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Utilities;
using WEB.UI.Controllers.Product.Base;

namespace WEB.UI.Controllers.Category
{
    [Route("[controller]")]
    public class ListItemController : BaseProductController
    {
        private readonly IConfiguration Configuration;
        private readonly RedisConn redisService;

        public ListItemController(IConfiguration _Configuration, RedisConn _redisService)
        {
            Configuration = _Configuration;
            redisService = _redisService;
        }

        /// <summary>
        /// Lấy ra danh sách nhóm hàng của 1 chiến dịch
        /// </summary>
        /// <param name="_campaign_id"></param>
        /// <returns></returns>
        [HttpPost("get-item-menu")]
        public async Task<IActionResult> loadMenuItem(int _campaign_id)
        {
            try
            {
                return ViewComponent("", new { campaign_id = _campaign_id, view = "/Views/Shared/Components/product/blog/aaa.cshtml" });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(Configuration["telegram_log_error_fe:Token"], Configuration["telegram_log_error_fe:GroupId"], "[FE] ListItemController get-item-menu error: " + ex.ToString() + " campaign_id =" + _campaign_id);
                return Content("");
            }
        }
    }
}