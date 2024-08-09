using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Cells.Charts;
using Caching.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Utilities.Contants;
using WEB.UI.Controllers.Carts;
using WEB.UI.Controllers.Product;
using WEB.UI.Service;

namespace WEB.UI.Controllers.LandingPage
{

    public class LandingController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public readonly IViewRenderService ViewRenderService;
        public LandingController(IViewRenderService _ViewRenderService, RedisConn _redisService, IConfiguration _Configuration)
        {
            ViewRenderService = _ViewRenderService;
            redisService = _redisService;
            configuration = _Configuration;
        }



        [HttpGet("primeday")]
        public IActionResult primeday2021()
        {
            return View("~/Views/Landing/PrimeDay/Index.cshtml");
        }

        [HttpGet("blackfriday")]
        public IActionResult Index()
        {
            return View("~/Views/Landing/BlackFriday2020/Index.cshtml");
        }

        [HttpPost("/Landing/group-list.json")]
        public async Task<IActionResult> getProductByFolderId(int folder_id, int top)
        {
            try
            {
                string cache_name = CacheType.FOLDER + folder_id;
                var landing = new primeDay2020(configuration, redisService, cache_name);
                var gr = new ProductGroup(configuration, redisService);
                var obj_product_list = await gr.getProductListByCacheName(folder_id, 2, 0, top);


                var _render_product_list = obj_product_list == null ? string.Empty : await this.RenderViewToStringAsync("/Views/Shared/Components/landingPage/primeDay2020/boxProductSale/product_list_tab.cshtml", obj_product_list);
                return Ok(new
                {
                    status = obj_product_list == null ? ResponseType.ERROR : ResponseType.SUCCESS,
                    render_product_list = _render_product_list
                });
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("web getProductByFolderId " + ex.Message);
                return Ok(new { status = ResponseType.ERROR, msg = ex.ToString() });
            }
        }

        [HttpGet("event_8_3")]
        public IActionResult event832021()
        {
            return View();
        }
    }
}