using Caching.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using WEB.UI.Controllers.Product;
using WEB.UI.Service;

namespace WEB.UI.ViewComponents
{
    public class groupProductViewComponent : ViewComponent
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public readonly IViewRenderService ViewRenderService;
        public groupProductViewComponent(IViewRenderService _ViewRenderService, RedisConn _redisService, IConfiguration _Configuration)
        {
            ViewRenderService = _ViewRenderService;
            redisService = _redisService;
            configuration = _Configuration;
        }

        /// <summary>
        /// Lấy ra từ khóa tìm kiếm có thông tin dc tim kiếm nhiều nhất
        /// //Convert.ToInt32(configuration["Redis:Database:db_folder"])
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync(int group_id, string view, int skip, int take, int redis_db_index)
        {
            try
            {
                var product = new ProductGroup(configuration, redisService);                

                // Lấy ra ds san pham trong chuyên mục đầu tiên
                
                var data_feed = await product.getProductListByCacheName(group_id, redis_db_index, skip, take);
                if (data_feed != null)
                {
                    return View(view, data_feed);
                }
                else
                {
                    return Content("...");
                }
            }
            catch (System.Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("fr groupProductViewComponent " + ex.Message);
                return Content("");
            }
        }


    }
}
