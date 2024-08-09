
using Entities.ViewModels;
using Entities.ViewModels.Affiliate;
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

namespace WEB.UI.Controllers.Client
{
    public partial class ClientService
    {
        private readonly IConfiguration configuration;

        public ClientService(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        // We're using HttpContextBase to allow access to cookies.
        public async Task<string> getLocationData(int location_type, string param)
        {
            try
            {
                string url_api = configuration["url_api_usexpress_new"];
                switch (location_type)
                {
                    case (int)LocationType.Type.PROVINCE:
                        url_api += "api/ServicePublic/province.json";
                        break;
                    case (int)LocationType.Type.DISTRICT:
                        url_api += "api/ServicePublic/district.json";
                        break;
                    case (int)LocationType.Type.WARD:
                        url_api += "api/ServicePublic/ward.json";
                        break;
                    default:
                        return string.Empty;
                }

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], param == "-1" ? "" : param);
                var response_api = await connect_api_us.CreateHttpRequest();

                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string data = JsonParent[0]["data_list"].ToString();

                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    return data;//.Replace("provinceId", "id").Replace("districtId", "id").Replace("wardId","id");
                }
                else
                {
                    return string.Empty;
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "getProvince " + ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Push client id to queue để Job thực hiện mapping sang us old
        /// </summary>
        public async void pushClientToQueue(long client_id, long address_id)
        {
            try
            {
                var j_param_input = new Dictionary<string, string>
                {
                    {"client_id",client_id.ToString()},
                    {"address_id", address_id.ToString()}                    
                };

                var j_param = new Dictionary<string, string>
                {
                    {"data_push",JsonConvert.SerializeObject(j_param_input)},
                    {"type",TaskQueueName.client_new_convert_queue},
                };
                string token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), configuration["KEY_TOKEN_API_2"]);

                string url_api = configuration["url_api_usexpress_new"] + "api/QueueService/data-push.json";

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status != ResponseType.SUCCESS.ToString())
                {
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[client_id = " + client_id + "] pushClientToQueue: reponse push api:" + _msg);
                }
                //else
                //{
                //    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[client_id = " + client_id + "]: Push client new sang queue thành công !" );
                //}
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[client_id = " + client_id + "] pushClientToQueue " + ex.Message);
            }
        }
        public async Task<Boolean> addNewFeedBackClient(int function_id, string feedback, string email_or_username)
        {
            string url_api = configuration["url_api_usexpress_new"] + "api/ServicePublic/add-answer-survery.json";

            try
            {

                var answerSurveryViewModel = new AnswerSurveryViewModel()
                {
                    FuntionId = function_id.ToString(),
                    Answer = feedback,
                    Email = email_or_username,
                    CreateOn = DateTime.Now
                };

                string j_param = "{'answer_survery':'" + Newtonsoft.Json.JsonConvert.SerializeObject(answerSurveryViewModel) + "'}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status != (ResponseType.SUCCESS).ToString())
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[url_api = " + url_api + "] pushClientToQueue " + ex.Message);
                return false;
            }
        }
        public async Task<Boolean> sendMail(string receive_email, string email_title, string email_body, string cc_email, string bcc_email)
        {
            string url_api = configuration["url_api_usexpress_new"] + "api/Mail/send-email";
            try
            {
                var j_param = new Dictionary<string, string>
                {
                    {"receive_email",receive_email },
                    {"email_title",email_title },
                    {"email_body",email_body },
                    {"cc_email",cc_email },
                    {"bcc_email",bcc_email }
                };
                string token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status != ((Int32)ResponseType.SUCCESS).ToString())
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[url_api = " + url_api + "] sendMail " + ex.Message);
                return false;
            }
        }
        public async Task<Boolean> UpdateClientChangePassword(ClientChangePasswordViewModel model)
        {
            string url_api = configuration["url_api_usexpress_new"] + "api/Client/update-client-change-pass.json";
            try
            {
                string j_param = "{'client_info':'" + Newtonsoft.Json.JsonConvert.SerializeObject(model) + "'}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status != ((Int32)ResponseType.SUCCESS).ToString())
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[url_api = " + url_api + "] UpdateClientChangePassword " + ex.Message);
                return false;
            }
        }

        public async Task<string> registerAffiliate(long client_id)
        {
            string url_api = configuration["url_api_usexpress_new"] + "api/Client/register-aff.json";
            try
            {

                string referral_id = client_id.ToString().Substring(client_id.ToString().Length - 1, 1) + ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();
                string j_param = "{'client_id':" + client_id + ", 'referral_id':'" + referral_id + "'}";
                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status != ((Int32)ResponseType.SUCCESS).ToString())
                {
                    return "";
                }
                return referral_id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[url_api = " + url_api + "] registerAffiliate " + ex.Message);
                return "";
            }
        }

        public async Task<ClientViewModel> getClientDetail(long client_id)
        {
            string url_api = configuration["url_api_usexpress_new"] + "api/Client/detail.json";
            try
            {
                string j_param = "{'clientId':" + client_id + "}";

                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                string _msg = JsonParent[0]["msg"].ToString();

                if (status != (ResponseType.SUCCESS).ToString())
                {
                    return null;
                }
                else
                {
                    var client_info = JsonConvert.DeserializeObject<ClientViewModel>(JsonParent[0]["client_detail"].ToString());
                    return client_info;
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[url_api = " + url_api + "] getClientDetail " + ex.Message);
                return null;
            }
        }


        public async Task<List<MyAffiliateLinkViewModel>> getLinkAffByClientId(long client_id)
        {
            string url_api = configuration["url_api_usexpress_new"] + "api/Affilliate/get-aff-by-client.json";
            try
            {
                string j_param = "{'client_id':" + client_id + "}";

                string token = CommonHelper.Encode(j_param, configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();


                if (status != (ResponseType.SUCCESS).ToString())
                {
                    string _msg = JsonParent[0]["msg"].ToString();
                    Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[url_api = " + url_api + "] getLinkAffByClientId (client_id =" + client_id + ") " + _msg);
                    return null;
                }
                else
                {
                    var client_link = JsonConvert.DeserializeObject<List<MyAffiliateLinkViewModel>>(JsonParent[0]["data"].ToString());
                    return client_link;
                }

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[url_api = " + url_api + "] getLinkAffByClientId " + ex.Message);
                return null;
            }
        }


        public async Task<int> addNewAffiliate(MyAffiliateLinkViewModel param)
        {
            string url_api = configuration["url_api_usexpress_new"] + "api/Affilliate/set-aff-by-client.json";
            try
            {

                string token = CommonHelper.Encode(JsonConvert.SerializeObject(param), configuration["KEY_TOKEN_API"]);

                var connect_api_us = new ConnectApi(url_api, configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], token);

                var response_api = await connect_api_us.CreateHttpRequest();
                // Nhan ket qua tra ve                            
                var JsonParent = JArray.Parse("[" + response_api + "]");
                int status = Convert.ToInt32(JsonParent[0]["status"]);

                return status;

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegramByUrl(configuration["telegram_log_error_fe:Token"], configuration["telegram_log_error_fe:GroupId"], "[url_api = " + url_api + "] addNewAffiliate " + ex.Message);
                return -1;
            }
        }
    }
}
