using Caching.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEB.UI.Controllers.Carts;
using WEB.UI.Controllers.LandingPage;

namespace WEB.UI.ViewComponents
{
    public class landingPageViewComponent : ViewComponent
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public string FOLDER_KEY { get; set; }
        public landingPageViewComponent(IConfiguration _configuration, RedisConn _redisService)
        {
            configuration = _configuration;
            redisService = _redisService;
        }
        /// <summary>
        /// Lấy ra từ khóa tìm kiếm có thông tin dc tim kiếm nhiều nhất
        /// </summary>
        /// <returns></returns>
        public async Task<IViewComponentResult> InvokeAsync(string view_comp, string cache_name)
        {
            
            var landing = new primeDay2020(configuration, redisService, cache_name);
            var obj_product_list = await landing.getProductByCategoryId();

            return View(view_comp, obj_product_list);
        }
    }
}
