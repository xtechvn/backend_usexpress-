using Entities.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace WEB.UI.Common
{
    public class RequestData
    {
        public string url_api { get; set; }
        public string token_tele { get; set; }
        public string group_id { get; set; }
        public string product_code { get; set; }
        public string page_type { get; set; }
        public string KEY_TOKEN_API { get; set; }
        public string url_crawl { get; set; }
        public int label_id { get; set; }
        public RequestData(string _url_api, string _token_tele, string _group_id, string _product_code, string _page_type, string _KEY_TOKEN_API, string _url_crawl, int _label_id)
        {

            label_id = _label_id;
            url_api = _url_api;
            url_crawl = _url_crawl;
            token_tele = _token_tele;
            group_id = _group_id;
            product_code = _product_code;
            page_type = _page_type;
            KEY_TOKEN_API = _KEY_TOKEN_API;
        }
        // Push tin hiệu tới Queue crawl chi tiết sản phẩm
        public async Task<bool> CrawlDetailProduct()
        {
            string token = string.Empty;
            try
            {

                var j_queue_param = new Dictionary<string, string>
                                {
                                    {"product_code",product_code},
                                    {"page_type", page_type},
                                    {"url",url_crawl},
                                    {"label_id",label_id.ToString()}
                                };

                token = CommonHelper.Encode(JsonConvert.SerializeObject(j_queue_param), KEY_TOKEN_API);

                // Kết nối tới API  để lấy dữ liệu crawl về                        
                // var connect_api_us = new ConnectApi(url_api, token_tele, group_id, token);
                //  var response_api = await connect_api_us.CreateHttpRequest();

                using (var httpClient = new HttpClient())
                {                   
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("token", token),
                    });

                    var response_api = await httpClient.PostAsync(url_api, content);

                    // Nhan ket qua tra ve                            
                    var JsonParent = JArray.Parse("[" + response_api.Content.ReadAsStringAsync().Result + "]");
                    string status = JsonParent[0]["status"].ToString();

                    if (status == ResponseType.SUCCESS.ToString())
                    {
                        return true; //Push queue thanh cong
                    }
                    else
                    {
                        string msg = JsonParent[0]["msg"].ToString();
                        LogHelper.InsertLogTelegram(token_tele, group_id, "getDetailProduct error from api: " + msg.ToString() + " token =" + token);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(token_tele, group_id, "getDetailProduct error: " + ex.ToString() + " token =" + token);
                return false;
            }
        }

        // Push tin hiệu tới Queue crawl Search
        public async Task<bool> CrawlSearchProduct(string keyword, string cache_name)
        {
            string token = string.Empty;
            try
            {
                var j_param_search = new Dictionary<string, string>
                                {
                                    {"label_id",label_id.ToString()},
                                    {"keyword", keyword},
                                    {"cache_name",cache_name}
                                };
                var j_param = new Dictionary<string, string>
                {
                    {"data_push",(JsonConvert.SerializeObject(j_param_search))},
                    {"type",TaskQueueName.keyword_crawl_queue},
                };
                token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), KEY_TOKEN_API);

                // Kết nối tới API  để lấy dữ liệu crawl về                        
                var connect_api_us = new ConnectApi(url_api, token_tele, group_id, token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();

                if (status == ResponseType.SUCCESS.ToString())
                {
                    return true; //Push queue thanh cong
                }
                else
                {
                    string msg = JsonParent[0]["msg"].ToString();
                    LogHelper.InsertLogTelegram(token_tele, group_id, "CrawlSearchProduct error from api: " + msg.ToString() + " token =" + token);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(token_tele, group_id, "CrawlSearchProduct error: " + ex.ToString() + " token =" + token);
                return false;
            }
        }

    }
}
