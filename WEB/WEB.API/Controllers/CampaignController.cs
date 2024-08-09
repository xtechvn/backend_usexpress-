using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Caching.RedisWorker;
using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Repositories.Repositories;
using Utilities;
using Utilities.Contants;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampaignController : Controller
    {
        private readonly ICampaignAdsRepository campaignAdsRepository;
        public IConfiguration configuration;
        private readonly RedisConn _RedisService;

        public CampaignController(IConfiguration config, ICampaignAdsRepository _campaignAdsRepository
            , RedisConn redisService)
        {
            configuration = config;
            campaignAdsRepository = _campaignAdsRepository;
            _RedisService = redisService;
        }

        [HttpPost("get-all.json")]
        public async Task<ActionResult> getAllCampaign()
        {
            try
            {
                string cache_name = CacheType.CAMPAIGN;
                int db_index = Convert.ToInt32(configuration["Redis:Database:db_common"]);

                var listCampaign = campaignAdsRepository.GetListAll();
                foreach (var item in listCampaign)
                {
                    //set cache
                    var cacheName = cache_name + item.Id;
                    var j_campaign = await _RedisService.GetAsync(cacheName, db_index);
                    if (string.IsNullOrEmpty(j_campaign))
                    {
                        _RedisService.Set(cacheName, JsonConvert.SerializeObject(item), db_index);
                    }
                }
                return Ok(new { status = ResponseType.SUCCESS.ToString(), listCampaign = JsonConvert.SerializeObject(listCampaign), msg = "Get All Campaign" });
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("getAllCampaign - API/CampaignController " + ex);
                return Ok(new { status = ResponseType.ERROR.ToString(), listCampaign = JsonConvert.SerializeObject(new List<CampaignAds>()), msg = ex.ToString() });
            }
        }
    }
}
