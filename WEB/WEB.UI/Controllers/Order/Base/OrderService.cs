
using Entities.ViewModels.ServicePublic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Utilities;
using Utilities.Contants;
using WEB.UI.Common;

namespace WEB.UI.Controllers.Order
{
    public partial class OrderService
    {
        private readonly IConfiguration configuration;

        public OrderService(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public async Task<string> getListingOrder(long client_id, int order_status, string input_search, int current_page, int page_size)
        {            
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                url_api += "api/Order/get-fe-order-list.json";
                string j_param = "{'client_id':" + client_id + ",'order_status':" + order_status + ",'input_search':'" + input_search + "','current_page':" + current_page + ",'page_size':" + page_size + "}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string data = JsonParent[0]["data_list"].ToString();
               // int totalOrder =Convert.ToInt32(JsonParent[0]["totalOrder"]);
               
                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    return response_api;
                }
                else
                {
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getListingOrder " + ex.Message);
                return string.Empty;
            }
        }
        public async Task<string> getOrderDetail(long order_id,long client_id)
        {
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                url_api += "api/Order/get-fe-order-detail.json";
                string j_param = "{'order_id':" + order_id + ",'client_id':" + client_id + "}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();                

                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    return JsonParent[0]["order_detail"].ToString();                    
                }
                else
                {
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getOrderDetail " + ex.Message);
                return string.Empty;
            }
        }
        public async Task<string> getListTabOrder(long client_id)
        {
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                url_api += "api/Order/get-fe-order-count.json";
                string j_param = "{'client_id':" + client_id + "}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string data = JsonParent[0]["order_count"].ToString();

                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    return data;
                }
                else
                {
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getListTabOrder status =" + status.ToString() + " token =" + token);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getListTabOrder " + ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Lấy ra đơn hàng gần nhất chưa thanh toán của User
        /// </summary>
        /// <param name="client_id"></param>
        /// <returns></returns>
        public async Task<string> getOrderLastByClientId(long client_id)
        {
            try
            {               
                string url_api = configuration["url_api_usexpress_new"];
                url_api += "api/Order/get-fe-lastest-order.json";
                string j_param = "{'client_id':" + client_id + "}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
              

                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    string data = JsonParent[0]["order_detail"].ToString();
                    return data;
                }
                else
                {
                  //  Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getOrderLastByClientId status =" + status.ToString() + " token =" + token);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getListTabOrder " + ex.Message);
                return string.Empty;
            }
        }


        public async Task<string> getOrderProgress(string order_no)
        {
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                url_api += "api/OrderProgress/get-order-progress.json";
                string j_param = "{'OrderNo':'" + order_no + "'}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();                

                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    string data = JsonParent[0]["order_progress"].ToString();
                    return data;
                }
                else
                {
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getOrderProgress " + ex.Message);
                return string.Empty;
            }
        }
    }
}
