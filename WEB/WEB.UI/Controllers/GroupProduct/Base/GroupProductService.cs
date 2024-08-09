
using Caching.RedisWorker;
using Entities.ViewModels.GroupProducts;
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

namespace WEB.UI.Controllers.Order
{
    public partial class GroupProductService
    {
        private readonly IConfiguration configuration;
        private readonly RedisConn redisService;
        public GroupProductService(IConfiguration _configuration, RedisConn _redisService)
        {
            configuration = _configuration;
            redisService = _redisService;
        }
        public async Task<List<GroupProductViewModel>> getGroupProductDetail(int campaign_id, int skip, int take)
        {
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                url_api += "api/GroupProduct/get-detail-by-campaign-id.json";
                string j_param = "{'campaign_id':" + campaign_id + ",'skip':" + skip + ",'take':" + take + "}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();


                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    var j_data = JsonParent[0]["data"].ToString();
                    var group_list = JsonConvert.DeserializeObject<List<GroupProductViewModel>>(j_data);
                    var model = group_list.Select(o => new GroupProductViewModel
                    {
                        id = o.id,
                        name = o.name,
                        link = o.link + "-cat",
                        image_thumb = o.image_thumb,
                        desc = o.desc
                    }).ToList();
                    return model;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getGroupProductDetail " + ex.Message);
                return null;
            }
        }
        public async Task<List<GroupProductViewModel>> getAllGroupProduct()
        {
            try
            {
                string cache_key = CacheType.MENU_GROUP_PRODUCT;
                int db_index = Convert.ToInt32(configuration["Redis:Database:db_common"]);
                var gr = new List<GroupProductViewModel>();

                var j_product_detail = await redisService.GetAsync(cache_key, db_index);
                if (!string.IsNullOrEmpty(j_product_detail) && j_product_detail != "null")
                {
                    gr = JsonConvert.DeserializeObject<List<GroupProductViewModel>>(j_product_detail);
                    return gr;
                }
                else
                {
                    string url_api = configuration["url_api_usexpress_new"];
                    url_api += "api/GroupProduct/get-all.json";
                    string j_param = "{'status':'0'}";
                    string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                    var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                    var response_api = await connect_api_us.CreateHttpRequest();

                    // Nhan ket qua tra ve                            
                    var JsonParent = JArray.Parse("[" + response_api + "]");
                    string status = JsonParent[0]["status"].ToString();


                    if (status == ((int)ResponseType.SUCCESS).ToString())
                    {
                        string data_gr = JsonParent[0]["data"].ToString();
                        gr = JsonConvert.DeserializeObject<List<GroupProductViewModel>>(data_gr);
                        return gr;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getGroupProductDetail " + ex.Message);
                return null;
            }
        }
        public async Task<List<MenuNewsViewModel>> getMenuNews()
        {
            try
            {
                string cache_key = CacheType.MENU_GROUP_PRODUCT;
                int db_index = Convert.ToInt32(configuration["Redis:Database:db_common"]);
                int cate_id_blog = Convert.ToInt32(configuration["News:cate_id_blog"]);
                var gr = new List<GroupProductViewModel>();

                var j_product_detail = await redisService.GetAsync(cache_key, db_index);
                if (!string.IsNullOrEmpty(j_product_detail) && j_product_detail != "null")
                {
                    var JsonParent = JArray.Parse("[" + j_product_detail + "]");
                    string status = JsonParent[0]["status"].ToString();
                    if (status == ((int)ResponseType.SUCCESS).ToString())
                    {
                        var j_data = JsonParent[0]["data"].ToString();
                        gr = JsonConvert.DeserializeObject<List<GroupProductViewModel>>(j_data);
                    }
                    else
                    {
                        Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "Ko tim thay menu trong cache" + cache_key);
                        return null;
                    }                  
                }
                else
                {
                    string url_api = configuration["url_api_usexpress_new"];
                    url_api += "api/GroupProduct/get-all.json";
                    string j_param = "{'status':'0'}";
                    string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                    var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                    var response_api = await connect_api_us.CreateHttpRequest();

                    // Nhan ket qua tra ve                            
                    var JsonParent = JArray.Parse("[" + response_api + "]");
                    string status = JsonParent[0]["status"].ToString();


                    if (status == ((int)ResponseType.SUCCESS).ToString())
                    {
                        string data_gr = JsonParent[0]["data"].ToString();
                        gr = JsonConvert.DeserializeObject<List<GroupProductViewModel>>(data_gr);
                    }
                    else
                    {
                        return null;
                    }
                }

                // filter
                var menu = new List<MenuNewsViewModel>();
                var menu_news = gr.Where(x => x.parent_id == cate_id_blog).ToList(); // lv 1

                foreach (var item in menu_news)
                {
                    int parent_id = item.id;
                    var child = gr.Where(x => x.parent_id == parent_id).ToList(); // lv2
                    var item_m = new MenuNewsViewModel
                    {
                        id = item.id,
                        parent_id = item.parent_id,
                        link = item.link,
                        name = item.name,
                        has_child = child.Count > 0 ? true : false,
                        menu_child = child
                    };
                    menu.Add(item_m);
                }
                return menu;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getMenuNews " + ex.Message);
                return null;
            }
        }
        public async Task<GroupProductViewModel> getGroupProductDetailByPath(string path)
        {
            try
            {
                string cache_key = CacheType.GROUP_PRODUCT_DETAIL + path;
                int db_index = Convert.ToInt32(configuration["Redis:Database:db_common"]);
                var gr = new GroupProductViewModel();
                string response_api = string.Empty;
                string token = string.Empty;

                var j_product_detail = await redisService.GetAsync(cache_key, db_index);

                if (string.IsNullOrEmpty(j_product_detail))
                {
                    string url_api = configuration["url_api_usexpress_new"];
                    url_api += "api/GroupProduct/get-detail-by-path.json";
                    string j_param = "{'path':'" + path + "'}";
                    token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                    var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                    response_api = await connect_api_us.CreateHttpRequest();
                }
                else
                {
                    response_api = j_product_detail;
                }

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();


                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    string data_gr = JsonParent[0]["data"].ToString();
                    gr = JsonConvert.DeserializeObject<GroupProductViewModel>(data_gr);
                    return gr;
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "fe-getGroupProductDetailByPath token = " + token);
                    return null;
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getGroupProductDetail " + ex.Message);
                return null;
            }
        }
        public async Task<List<GroupProductFeaturedViewModel>> GetFeaturedGroupProduct()
        {
            try
            {
                string cache_key = CacheType.GROUP_MENU;
                int db_index = Convert.ToInt32(configuration["Redis:Database:db_common"]);
                var gr = new List<GroupProductFeaturedViewModel>();
                string response_api = string.Empty;
                string token = string.Empty;

                var j_data = await redisService.GetAsync(cache_key, db_index);
                // j_data = "";
                if (string.IsNullOrEmpty(j_data))
                {
                    string url_api = configuration["url_api_usexpress_new"];
                    url_api += "api/GroupProduct/get-group-product-featured.json";
                    string j_param = "{'status':'" + (Int16)Status.HOAT_DONG + "'}";
                    token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                    var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                    response_api = await connect_api_us.CreateHttpRequest();

                    var j_data_api = JArray.Parse("[" + response_api + "]");
                    string status_api = j_data_api[0]["status"].ToString();
                    if (status_api == ((int)ResponseType.SUCCESS).ToString())
                    {
                        redisService.Set(cache_key, response_api, db_index);
                    }
                }
                else
                {
                    response_api = j_data;
                }

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();

                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    string data_gr = JsonParent[0]["data"].ToString();
                    gr = JsonConvert.DeserializeObject<List<GroupProductFeaturedViewModel>>(data_gr);
                    return gr;
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "fe-GetFeaturedGroupProduct token = " + token);
                    return null;
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "GetFeaturedGroupProduct " + ex.Message);
                return null;
            }
        }

        public async Task<List<LocationProductViewModel>> getProductCodeByGroupId(int group_product_id)
        {
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                url_api += "api/GroupProduct/get-product-code-by-group-id.json";
                string j_param = "{'group_product_id':" + group_product_id + "}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();


                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    var j_data = JsonParent[0]["data"].ToString();
                    var group_list = JsonConvert.DeserializeObject<List<LocationProductViewModel>>(j_data);
                    var model = group_list.Select(o => new LocationProductViewModel
                    {
                        product_code = o.product_code
                    }).ToList();

                    // get trong ES lay detail list








                    return model;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getProductCodeByGroupId " + ex.Message);
                return null;
            }
        }

    }
}
