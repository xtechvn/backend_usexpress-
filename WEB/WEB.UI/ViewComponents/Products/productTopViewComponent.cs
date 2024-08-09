using Caching.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

using WEB.UI.Controllers.Order;
using WEB.UI.Controllers.Product;
using WEB.UI.Service;
using WEB.UI.ViewModels;

namespace WEB.UI.ViewComponents
{
    public class productTopViewComponent : ViewComponent
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public readonly IViewRenderService ViewRenderService;
        public productTopViewComponent(IViewRenderService _ViewRenderService, RedisConn _redisService, IConfiguration _Configuration)
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
        public async Task<IViewComponentResult> InvokeAsync(int campaign_id, string view, int skip, int take, int redis_db_index)
        {
            try
            {
                var product = new ProductGroup(configuration, redisService);

                //Lấy ra tên chuyên mục
                var group_product_service = new GroupProductService(configuration, redisService);
                var group_list = await group_product_service.getGroupProductDetail(campaign_id, skip, take); // Lấy ra danh sách chuyên mục cua 1 chien dich

                if (group_list == null) return Content("");

                // Lấy ra ds san pham trong chuyên mục đầu tiên
                var folder_id_first = group_list.Count() > 0 ? group_list[0].id.ToString() : "-1";
                var data_feed = await product.getProductListByCacheName(Convert.ToInt32(folder_id_first), redis_db_index, skip, take);

                var model = new ProductTopEntitiesViewModel
                {
                    obj_tab = group_list,
                    product_list = data_feed,
                    campaign_id = campaign_id
                  
                };
                return View(view, model);
            }
            catch (System.Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("fr productTopViewComponent " + ex.Message);
                return Content("");
            }
        }


    }
}
