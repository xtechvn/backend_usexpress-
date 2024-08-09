using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caching.RedisWorker;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : BaseController
    {
        private readonly IGroupProductRepository _groupRepository;
        private readonly RedisConn _redisService;
        public IConfiguration _Configuration;
        public MenuController(IGroupProductRepository groupProductRepository,
        IConfiguration Configuration, RedisConn redisService)
        {
            _Configuration = Configuration;
            _redisService = redisService;
            _groupRepository = groupProductRepository;
        }

        [HttpPost("get-list-cate-help.json")]
        public async Task<ActionResult> getAllFaq(string token)
        {
            try
            {
                string j_param = "{'parent_id':279}";
                // token = CommonHelper.Encode(j_param, _Configuration["KEY_TOKEN_API"]);
                string _msg = string.Empty;
                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, _Configuration["KEY_TOKEN_API"]))
                {
                    var faq = new List<GroupProduct>();
                    int parent_id = Convert.ToInt32(objParr[0]["parent_id"]);
                    string cache_name = CacheType.HELP_FAQ + parent_id;
                    var j_data = await _redisService.GetAsync(cache_name, Convert.ToInt32(_Configuration["Redis:Database:db_common"]));


                    if (j_data != null)
                    {
                        faq = JsonConvert.DeserializeObject<List<GroupProduct>>(j_data);
                        _msg = "Get cache Successfully !!!";
                    }
                    else
                    {
                        faq = await _groupRepository.getCategoryByParentId(parent_id);
                        if (faq.Count() > 0)
                        {
                            _redisService.Set(cache_name, JsonConvert.SerializeObject(faq), Convert.ToInt32(_Configuration["Redis:Database:db_common"]));
                            _msg = "Get db Successfully !!!";
                        }
                    }

                    var rs = from n in faq
                             select new CategoryViewModel
                             {
                                 name = n.Name,
                                 cate_id = n.Id,
                                 parent_id = n.ParentId,
                                 path = n.Path
                             };
                    return Ok(new
                    {
                        status = (int)ResponseType.SUCCESS,
                        data_list = rs,
                        msg = _msg
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = (int)ResponseType.ERROR,
                        msg = "Key ko hop le"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("menu/faq/{parent_id}: " + ex);
                return Ok(new
                {
                    status = (int)ResponseType.FAILED,
                    msg = "[getAllFaq] = " + ex.ToString()
                });
            }
        }

    }
}