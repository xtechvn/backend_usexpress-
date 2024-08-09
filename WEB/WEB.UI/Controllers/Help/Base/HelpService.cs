using Entities.ViewModels;
using Entities.ViewModels.ServicePublic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.UI.Common;

namespace WEB.UI.Controllers.Client
{
    public partial class HelpService
    {
        private readonly IConfiguration configuration;

        public HelpService(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        /// <summary>
        /// Lấy ra danh mục con của HELP
        /// </summary>
        /// <param name="parent_id"></param>
        /// <returns></returns>
        public async Task<List<CategoryViewModel>> getListMenuHelp(int parent_id)
        {
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                url_api += "api/Menu/get-list-cate-help.json";
                string j_param = "{'parent_id':" + parent_id + "}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                int status =Convert.ToInt32(JsonParent[0]["status"]);           

                if (status == ((int)ResponseType.SUCCESS))
                {
                    string data = JsonParent[0]["data_list"].ToString();
                    return JsonConvert.DeserializeObject<List<CategoryViewModel>>(data);
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getListMenuHelp " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Lấy ra danh mục con của HELP
        /// </summary>
        /// <param name="parent_id"></param>
        /// <returns></returns>
        public async Task<List<ArticleViewModel>> getArticleListByCateId(int cate_id)
        {
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                url_api += "api/Article/get-list-by-categoryid.json";
                string j_param = "{'category_id':" + cate_id + "}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string data = JsonParent[0]["data_list"].ToString();

                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    return JsonConvert.DeserializeObject<List<ArticleViewModel>>(data);
                }
                else
                {
                    string msg = JsonParent[0]["msg"].ToString();
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getArticleListByCateId: get  api that bai msg =  " + msg);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getArticleListByCateId " + ex.Message);
                return null;
            }
        }
        /// <summary>
        /// Tìm ra những câu trả lời theo title
        /// </summary>
        /// <param name="keyword"></param>
        /// cate_id: cate cha cua help: gioi han tim kiem trong khoang do
        /// <returns></returns>
        public async Task<List<ArticleViewModel>> FindAnserByTitle(string keyword,int cate_id)
        {
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                url_api += "api/Article/find-article.json";
                string j_param = "{'title':'" + keyword + "','parent_cate_faq_id':" + cate_id + "}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string data = JsonParent[0]["data_list"].ToString();

                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    return JsonConvert.DeserializeObject<List<ArticleViewModel>>(data);
                }
                else
                {                 
                    return null;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "FindAnserByTitle " + ex.Message);
                return null;
            }
        }

    }
}
