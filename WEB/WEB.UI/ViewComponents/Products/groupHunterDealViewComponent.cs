using Caching.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

using WEB.UI.Controllers.Order;
using WEB.UI.Controllers.Product;
using WEB.UI.Service;
using WEB.UI.ViewModels;

namespace WEB.UI.ViewComponents
{
    public class groupHunterDealViewComponent : ViewComponent
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public readonly IViewRenderService ViewRenderService;
        public groupHunterDealViewComponent(IViewRenderService _ViewRenderService, RedisConn _redisService, IConfiguration _Configuration)
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
        public async Task<IViewComponentResult> InvokeAsync(int campaign_id, string view, int skip, int take)
        {
            try
            {
                var product = new ProductGroup(configuration, redisService);

                //Lấy ra tên chuyên mục
                var group_product_service = new GroupProductService(configuration,redisService);
                var group_list = await group_product_service.getGroupProductDetail(campaign_id, skip, take); // Lấy ra danh sách chuyên mục cua 1 chien dich

                if (group_list == null) return Content("");                
                
                return View(view, group_list);
            }
            catch (System.Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("fr groupHunterDealViewComponent " + ex.Message);
                return Content("");
            }
        }
    }
}
