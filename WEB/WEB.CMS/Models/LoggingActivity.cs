using Entities.ViewModels.Log;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Models;

namespace WEB.CMS.Common
{
    public static class LoggingActivity
    {
        public static async Task AddLog(int user_id,string user_name, int log_type, string j_data_log, string additional_keyword="")
        {
            try
            {
                if (user_id<0||user_name==null||user_name.Trim()==""||log_type<0)
                {
                }
                else
                {
                    var data = new
                    {
                        user_type = 0,
                        user_id = user_id, 
                        user_name = user_name,
                        j_data_log = j_data_log, 
                        log_type = log_type, 
                        key_word_search = additional_keyword
                    };
                    HttpClient httpClient = new HttpClient();
                    var apiPrefix = ReadFile.LoadConfig().API_CMS_URL+ReadFile.LoadConfig().API_LOG_ACTIVITY;
                    var KEY_TOKEN_API = ReadFile.LoadConfig().KEY_TOKEN_API;
                    string j_param = JsonConvert.SerializeObject(data);
                    string token = CommonHelper.Encode(j_param, KEY_TOKEN_API);
                    var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
                    var result = await httpClient.PostAsync(apiPrefix, content);
                }
            } catch(Exception ex)
            {
                LogHelper.InsertLogTelegram("AddLog - LogActivity " + ex.ToString());
            }
        }
    }
}
