using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caching.RedisWorker;
using Crawler.ScraperLib.Amazon;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utilities;
using Utilities.Contants;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AmzProductController : Controller
    {
        public IConfiguration configuration;
        private readonly RedisConn _RedisService;
        public AmzProductController(IConfiguration config, RedisConn redisService)
        {
            configuration = config;
            _RedisService = redisService;
        }

        [HttpPost("test.json")]
        public async Task<IActionResult> testProductAMZ(string asin, string page_source_html, string url_page)
        {
            var j_param = new Dictionary<string, string>
                {
                    {"asin",asin},
                    {"page_source_html",page_source_html},
                    {"url_page",url_page},
                };
            var data_product = JsonConvert.SerializeObject(j_param);
            string token = CommonHelper.Encode(data_product, configuration["KEY_TOKEN_API"]);

            return RedirectToAction("RegexElementPage", new { token = token });
        }

        [HttpPost("detail.json")]
        public async Task<ActionResult> RegexElementPage(string token)
        {
            try
            {
                //var j_param = new Dictionary<string, string>
                //{
                //    {"asin", "B07SHSKY2R"},
                //    {"page_source_html","<div class=\"a-section a-spacing-micro\"><span id=\"price_inside_buybox\"class=\"a-size-medium a-color-price\">$24.99</span><span id=\"price_inside_buybox_badge\"class=\"a-size-base a-color-price\"></span></div>" },
                //    {"url_page","https://www.amazon.com/dp/B07SHSKY2R?psc=1"},
                //};
                //var data_product = JsonConvert.SerializeObject(j_param);
                //token = CommonHelper.Encode(data_product, configuration["KEY_TOKEN_API"]);
               

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    string page_source_html = objParr[0]["page_source_html"].ToString();
                    string asin = objParr[0]["asin"].ToString();
                    string url_page = objParr[0]["url_page"].ToString();

                //    Utilities.LogHelper.InsertLogTelegram("[API] api/AmzProduct/detail - RegexElementPage from bot call asin = " + asin);

                    var lib = new Service.Lib.Common(configuration, _RedisService);
                    var rate = lib.getRateCache();

                    var amz_detail = ParserAmz.RegexElementPage(page_source_html, asin, url_page, rate);
                    if (amz_detail != null)
                    {
                        return Ok(new { status = (int)ResponseType.SUCCESS, data = amz_detail });
                    }
                    else
                    {
                        Utilities.LogHelper.InsertLogTelegram("[API] api/AmzProduct/detail - RegexElementPage  Token valid !!! error token = " + token);
                        return Ok(new { status = (int)ResponseType.EMPTY, token = token, data = amz_detail });
                    }
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegram("[API] api/AmzProduct/detail - RegexElementPage  Token valid !!! error token = " + token);
                    return Ok(new { status = ResponseType.ERROR.ToString(), token = token, msg = "Token valid !!!" });
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("[API] api/AmzProduct/detail - RegexElementPage  Token valid !!! error ex = " + ex.ToString());
                return Ok(new { status = ResponseType.ERROR.ToString(), token = token });
            }
        }
    }
}