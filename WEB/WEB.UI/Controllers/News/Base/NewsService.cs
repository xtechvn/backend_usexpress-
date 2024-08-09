using Caching.RedisWorker;

using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Utilities;
using Utilities.Contants;
using WEB.UI.Common;
using WEB.UI.ViewModels;
using Entities.ViewModels;

namespace WEB.UI.Controllers.Order
{
    public partial class NewsService
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public NewsService(IConfiguration _configuration, RedisConn _redisService)
        {
            configuration = _configuration;
            redisService = _redisService;
        }
        public async Task<ArticleEntitiesViewModel> getArticleByCategoryId(int category_id, int skip, int take)
        {
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                url_api += "api/Article/get-list-by-categoryid-order.json";
                string j_param = "{'category_id':" + category_id + ",'skip':" + skip + ",'take':" + take + "}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();

                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    var j_data = JsonParent[0]["data_list"].ToString();
                    var news = JsonConvert.DeserializeObject<List<ArticleFeModel>>(j_data);
                    int total_news = Convert.ToInt32(JsonParent[0]["total_item"]);
                    var article = new ArticleEntitiesViewModel
                    {
                        news_list = news,
                        total_news = news.Count == 0 ? 0 : total_news

                    };
                    return article;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getArticleByCategoryId " + ex.Message);
                return null;
            }
        }
        public async Task<ArticleModel> getArticleDetail(long article_id)
        {
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                url_api += "api/Article/get-detail.json";
                string j_param = "{'article_id':" + article_id + "}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();

                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    var j_data = JsonParent[0]["data"].ToString();
                    var detail = JsonConvert.DeserializeObject<ArticleModel>(j_data);
                    return detail;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getArticleDetail[article_id = " + article_id + "] " + ex.Message);
                return null;
            }
        }

        public async Task<List<ArticleFeModel>> getNewsTopPageView()
        {
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                url_api += "api/Article/get-most-viewed-article.json";
                string j_param = "{'status':1}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();

                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    var j_data = JsonParent[0]["data"].ToString();
                    var detail = JsonConvert.DeserializeObject<List<ArticleFeModel>>(j_data);
                    return detail;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getNewsTopPageView()" + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Lưu pageview cho bài viết
        /// </summary>
        /// <returns></returns>
        public async Task<bool> updatePageView(long article_id)
        {
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                url_api += "api/Article/post-article-pageview.json";
                string j_param = "{'articleID':" + article_id + ",'pageview':1}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();

                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    //var j_data = JsonParent[0]["data"].ToString();                    
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getNewsTopPageView()" + ex.Message);
                return false;
            }
        }
    }
}
