using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace WEB.UI.Common
{
    public class ConnectApi
    {
        public string url_api { get; set; }
        public string token_tele { get; set; }
        public string group_id { get; set; }       
        
        public string token { get; set; }

        public ConnectApi(string _url_api, string _token_tele, string _group_id, string _token)
        {           
            url_api = _url_api;           
            token_tele = _token_tele;
            group_id = _group_id;
            token = _token;
        }

        public async Task<string> CreateHttpRequest()
        {
            try
            {
                string responseFromServer = string.Empty;
                string status = string.Empty;
                var httpClientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
                };

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("token", token),
                    });

                    var response_api = await httpClient.PostAsync(url_api, content);

                    // Nhan ket qua tra ve                            
                    responseFromServer = response_api.Content.ReadAsStringAsync().Result;
                    
                }

                return responseFromServer;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram(token_tele, group_id, "[API NOT CONNECT] CreateHttpRequest error: " + ex.ToString() + " token =" + token + " url_api = " + url_api);
                return string.Empty;
            }
        }
    }
}
