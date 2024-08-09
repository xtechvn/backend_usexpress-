using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caching.RedisWorker;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabelController : BaseController
    {
        private readonly ILabelRepository _LabelRepository;
        public IConfiguration configuration;
        private readonly RedisConn _RedisService;

        public LabelController(IConfiguration config, ILabelRepository labelRepository
            , RedisConn redisService)
        {
            configuration = config;
            _LabelRepository = labelRepository;
            _RedisService = redisService;
        }

        [HttpPost("get-all")]
        public async Task<ActionResult> getLabelActive()
        {
            try
            {
                string cache_name = CacheType.LABEL_PRODUCT;
                var labelActive = new List<Label>();
                int db_index = Convert.ToInt32(configuration["Redis:Database:db_common"]);
                //Kiểm tra trong cache có không
                var j_label = await _RedisService.GetAsync(cache_name, db_index);
                if (j_label == null)
                {
                    labelActive = await _LabelRepository.GetLabelActive();
                    var labelActiveNew = labelActive.Select(n => new
                    {
                        id = n.Id,
                        storeName = n.StoreName,
                        icon = n.Icon,
                        prefixOrderCode = n.PrefixOrderCode,
                        domain = n.Domain,
                        status = n.Status,
                    }).ToList();

                    _RedisService.Set(cache_name, JsonConvert.SerializeObject(labelActiveNew), db_index);

                    return Ok(new { status = ResponseType.SUCCESS.ToString(), data = labelActiveNew });
                }
                else
                {
                    labelActive = JsonConvert.DeserializeObject<List<Label>>(j_label);
                    return Ok(new { status = ResponseType.SUCCESS.ToString(), data = labelActive });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getLabelActive - DashBoardController " + ex);
                return Ok(new
                {
                    status = ResponseType.ERROR.ToString(),
                    msg = "Error !!!"
                });
            }
        }
    }
}
