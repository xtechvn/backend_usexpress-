using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caching.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;

namespace WEB.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController : Controller
    {
        private readonly ICampaignAdsRepository campaignAdsRepository;
        public IConfiguration configuration;
        private readonly RedisConn _redisService;

        public CacheController(IConfiguration config, RedisConn redisService, ICampaignAdsRepository _campaignAdsRepository)
        {
            configuration = config;
            _redisService = redisService;
            campaignAdsRepository = _campaignAdsRepository;
        }

        /// <summary>
        /// Clear cache bài viết
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("sync-article.json")]
        public async Task<ActionResult> clearCacheArticle(string token)
        {
            try
            {
                string j_param = "{'article_id':'35','category_id':'35'}";
                //  token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {

                    long article_id = Convert.ToInt64(objParr[0]["article_id"]);
                    var category_list_id = objParr[0]["category_id"].ToString().Split(",");
                    _redisService.clear(CacheType.ARTICLE_ID + article_id, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    for (int i = 0; i <= category_list_id.Length - 1; i++)
                    {
                        int category_id = Convert.ToInt32(category_list_id[i]);
                        _redisService.clear(CacheType.ARTICLE_CATEGORY_ID + category_id, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                        _redisService.clear(CacheType.CATEGORY_NEWS + category_id, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    }

                    return Ok(new { status = (int)ResponseType.SUCCESS, _token = token, msg = "Sync Successfully !!!", article_id = article_id, category_list_id = category_list_id });
                }
                else
                {
                    return Ok(new { status = (int)ResponseType.FAILED, _token = token, msg = "Token Error !!!" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("sync-article.json - clearCacheArticle " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = (int)ResponseType.ERROR, _token = token, msg = "Sync error !!!" });
            }
        }

        /// Clear cache Chuyên mục
        /// Clear cache các chuyên mục trong 1 chiến dịch
        [HttpPost("sync-category.json")]
        public async Task<ActionResult> clearCacheCategory(string token)
        {
            try
            {
                //string j_param = "{'category_id':'308'}";
                //  token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    int category_id = Convert.ToInt32(objParr[0]["category_id"]);
                    _redisService.clear(CacheType.ARTICLE_CATEGORY_ID + category_id, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    _redisService.clear(CacheType.HELP_FAQ + configuration["News:cate_id_help"], Convert.ToInt32(configuration["Redis:Database:db_common"]));

                    #region Clear Campaign
                    var campaign_id = await campaignAdsRepository.getCampaignIdByCategoryId(category_id);

                    string cache_key = CacheType.CAMPAIGN_ID_ + campaign_id;
                    _redisService.clear(cache_key, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    #endregion
                    return Ok(new { status = (int)ResponseType.SUCCESS, _token = token, msg = "Sync Successfully !!!", _category_id = category_id });
                }
                else
                {
                    return Ok(new { status = (int)ResponseType.FAILED, _token = token, msg = "Token Error !!!" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("sync-category.json - clearCacheArticle " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = (int)ResponseType.ERROR, _token = token, msg = "Sync error !!!" });
            }
        }

        [HttpPost("clear.json")]
        public async Task<ActionResult> clearCache(string token)
        {
            try
            {
                // string j_param = "{'value':'22','cache_type':'" + CacheType.CAMPAIGN_ID_ + "'}";
                //  token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    string value = (objParr[0]["value"]).ToString();
                    string cache_type = (objParr[0]["cache_type"]).ToString();

                    #region Clear Campaign
                    string cache_key = cache_type + value;
                    _redisService.clear(cache_key, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    #endregion
                    return Ok(new { status = (int)ResponseType.SUCCESS, _token = token, msg = "Clear Successfully !!!", cache_key = cache_key });
                }
                else
                {
                    return Ok(new { status = (int)ResponseType.FAILED, _token = token, msg = "Token Error !!!" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api-clear.json - clearCache " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = (int)ResponseType.ERROR, _token = token, msg = "Sync error !!!" });
            }
        }


        [HttpPost("clear_cache_by_key.json")]
        public async Task<ActionResult> clearCacheByKey(string token)
        {
            try
            {
                //  string j_param = "{'cache_key':'abcdef'}";
                //  token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                JArray objParr = null;
                if (CommonHelper.GetParamWithKey(token, out objParr, configuration["KEY_TOKEN_API"]))
                {
                    string cache_key = objParr[0]["cache_key"].ToString();

                    #region Clear a cache
                    _redisService.clear(cache_key, Convert.ToInt32(configuration["Redis:Database:db_common"]));
                    #endregion
                    return Ok(new { status = (int)ResponseType.SUCCESS, _token = token, msg = "Clear Successfully !!!", cache_key = cache_key });
                }
                else
                {
                    return Ok(new { status = (int)ResponseType.FAILED, _token = token, msg = "Token Error !!!" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("api-clear.json - clearCacheByKey " + ex.Message + " token=" + token.ToString());
                return Ok(new { status = (int)ResponseType.ERROR, _token = token, msg = "Sync error !!!" });
            }
        }
    }
}