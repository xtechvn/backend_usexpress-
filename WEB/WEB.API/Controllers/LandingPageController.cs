using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Utilities.Contants;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LandingPageController : Controller
    {
        private readonly RedisConn _redisService;
        private readonly IConfiguration _Configuration;
        public LandingPageController(RedisConn redisService, IConfiguration Configuration)
        {
            _redisService = redisService;
            _Configuration = Configuration;
        }


        [HttpPost("campaign/product_list.json")]
        public async Task<ActionResult> getProductByCacheName(string key_cache)
        {
            int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_folder"]);

            var product_detail = await _redisService.GetAsync(key_cache, db_index);
            if (!string.IsNullOrEmpty(product_detail))
            {
                return Ok(new { status = ResponseType.SUCCESS.ToString(), product_data = product_detail, msg = ResponseType.SUCCESS.ToString() });
            }
            else
            {
                int group_id = -1;
                try
                {
                    group_id = Convert.ToInt32(key_cache.Trim().Replace("GROUP_PRODUCT_", ""));
                    if (group_id > 0)
                    {
                        IESRepository<object> _ESRepository = new ESRepository<object>(_Configuration["DataBaseConfig:Elastic:Host"]);
                        var data = await _ESRepository.getProductListByGroupProductId("product", new List<int>() { group_id }, 0, 50, "all");
                        if (data != null && data.obj_lst_product_result != null && data.obj_lst_product_result.Count > 0)
                        {
                            product_detail = JsonConvert.SerializeObject(data.obj_lst_product_result);
                            _redisService.Set(key_cache, product_detail,DateTime.Now.AddHours(4), db_index);
                            return Ok(new { status = ResponseType.SUCCESS.ToString(), product_data = product_detail, msg = ResponseType.SUCCESS.ToString() });
                        }
                    }
                }
                catch (Exception)
                {

                }
                return Ok(new { status = ResponseType.EMPTY.ToString(), msg = "khong tim thay san pham nao trong cache name: " + key_cache });
            }
        }
    }
}