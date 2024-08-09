using Caching.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WEB.UI.Controllers.LandingPage;
using WEB.UI.Controllers.Product;
using WEB.UI.Service;

namespace WEB.UI.ViewComponents
{
    public class dataFeedViewComponent : ViewComponent
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public readonly IViewRenderService ViewRenderService;
        public dataFeedViewComponent(IViewRenderService _ViewRenderService, RedisConn _redisService, IConfiguration _Configuration)
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
        public async Task<IViewComponentResult> InvokeAsync(string cache_name, string view, int skip, int take, int redis_db_index)
        {
            
            var product = new ProductGroup(configuration, redisService);
            var data_feed = await product.getProductListByCacheName(-1, redis_db_index, skip, take);
         

            return View(view, data_feed);
        }
    }
}
