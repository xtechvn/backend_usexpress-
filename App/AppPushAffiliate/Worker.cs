using App_Crawl_SearchList_Push_Worker.Models;
using AppPushAffiliate.Models;
using Entities.ViewModels.Affiliate;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace AppPushAffiliate
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int time_delay = 86400;
            var appsetting = ReadFile.LoadConfig();
            var utm_source = new List<string>() { "accesstrade", "adpia" };
            var aff_db = new AffiliateOrderMongoAccess(appsetting.MongoServer_Host, appsetting.MongoServer_catalog);
            try
            {
                time_delay = Convert.ToInt32(ReadFile.LoadConfig().delay_time);
                if (appsetting.ExcuteFromBegin == "1")
                {
                    var data_onces = await GetOrderListFromBegin(appsetting.API_LIVE_URL + appsetting.API_GET_AFF_ORDERLIST);
                    var text = JsonConvert.SerializeObject(data_onces);
                    var list = JsonConvert.DeserializeObject<List<AffOrder>>(text);
                    if (list != null && list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            AffiliateOrderItem exists_item = await aff_db.GetSuccessAffOrderByID(item.order_id);
                            if (exists_item !=null && exists_item.order_id==item.order_id)
                            {
                                continue;
                            }
                            string url = "";
                            switch (item.aff_name.Trim())
                            {
                                case "adpia":
                                    {
                                        url = appsetting.API_LIVE_URL + appsetting.API_PUSH_ORDER_TO_ADPIA;
                                    }
                                    break;
                                case "accesstrade":
                                    {
                                        url = appsetting.API_LIVE_URL + appsetting.API_PUSH_ORDER_TO_ACCESSTRADE;
                                    }
                                    break;
                                default: break;
                            }
                            if (url == "") continue;
                            var rs = await PushOrderToAff(url, item.order_id);
                            if (rs["Status"] == ((int)ResponseType.SUCCESS).ToString())
                            {
                                var mongo_item = new AffiliateOrderItem
                                {
                                    aff_name = item.aff_name,
                                    order_id = item.order_id,
                                    msg = rs["Msg"],
                                    status = Convert.ToInt32(rs["Status"]),
                                    time_push = DateTime.Now,

                                };
                                mongo_item.GenID();
                                var id= await aff_db.AddNewAsync(mongo_item);
                                if(id!=null) _logger.LogInformation("Success at"+ DateTimeOffset.Now);

                            }
                        }
                    }
                }
            }
            catch (Exception ex) { _logger.LogInformation("Worker Error: " + ex.ToString()); }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var data = await GetOrderList(appsetting.API_LIVE_URL + appsetting.API_GET_AFF_ORDERLIST, DateTime.Today, DateTime.Now, utm_source);

                    var text = JsonConvert.SerializeObject(data);
                    var list = JsonConvert.DeserializeObject<List<AffOrder>>(text);
                    if (list!=null && list.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            AffiliateOrderItem exists_item = await aff_db.GetSuccessAffOrderByID(item.order_id);
                            if (exists_item != null && exists_item.order_id == item.order_id)
                            {
                                continue;
                            }
                            string url = "";
                            switch (item.aff_name.Trim())
                            {
                                case "adpia":
                                    {
                                        url = appsetting.API_LIVE_URL + appsetting.API_PUSH_ORDER_TO_ADPIA;
                                    }
                                    break;
                                case "accesstrade":
                                    {
                                        url = appsetting.API_LIVE_URL + appsetting.API_PUSH_ORDER_TO_ACCESSTRADE;
                                    }
                                    break;
                                default: break;
                            }
                            if (url == "") continue;
                            var rs = await PushOrderToAff(url, item.order_id);
                            if (rs["Status"] == ((int)ResponseType.SUCCESS).ToString())
                            {
                                var mongo_item = new AffiliateOrderItem
                                {
                                    aff_name = item.aff_name,
                                    order_id = item.order_id,
                                    msg = rs["Msg"],
                                    status = Convert.ToInt32(rs["Status"]),
                                    time_push = DateTime.Now,

                                };
                                mongo_item.GenID();
                                var id = await aff_db.AddNewAsync(mongo_item);
                                if (id != null) _logger.LogInformation("Success at" + DateTimeOffset.Now);
                            }
                        }
                    }
                }
                catch (Exception ex) { _logger.LogInformation("Worker Error: " + ex.ToString()); }
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(time_delay * 1000, stoppingToken);
            }
        }
        public async Task<dynamic> GetOrderList(string api_url, DateTime time_start, DateTime time_end, List<string> utm_sources)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiPrefix = api_url;
                var data = new
                {
                    time_start = time_start,
                    time_end = time_end,
                    utm_sources = JsonConvert.SerializeObject(utm_sources)
                };
                string token = CommonHelper.Encode(JsonConvert.SerializeObject(data), ReadFile.LoadConfig().API_Key);
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("token", token),
                });
                var result = await httpClient.PostAsync(apiPrefix, content);
                dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
                var status = (string)resultContent.status;
                if (Convert.ToInt32(status) == (int)ResponseType.SUCCESS)
                    return (dynamic)resultContent.data;

            }
            catch (Exception ex)
            {
                string err = "AppPushAffiliate - GetOrderList : " + ex.ToString();
                LogHelper.InsertLogTelegram(err);
                _logger.LogInformation(err);
            }
            return null;
        }
        public async Task<object> GetOrderListFromBegin(string api_url)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiPrefix = api_url;
                var utm_source = new List<string>() { "accesstrade", "adpia" };
                var data = new
                {
                    time_start = new DateTime(2021, 9, 1, 0, 0, 0, 0),
                    time_end = DateTime.Today,
                    utm_sources = JsonConvert.SerializeObject(utm_source)
                };
                string token = CommonHelper.Encode(JsonConvert.SerializeObject(data), ReadFile.LoadConfig().API_Key);
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("token", token),
                });
                var result = await httpClient.PostAsync(apiPrefix, content);
                dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
                var status = (string)resultContent.status;
                if (Convert.ToInt32(status) == (int)ResponseType.SUCCESS)
                    return (object)resultContent.data;

            }
            catch (Exception ex)
            {
                string err = "AppPushAffiliate - GetOrderListFromBegin : " + ex.ToString();
                LogHelper.InsertLogTelegram(err);
                _logger.LogInformation(err);
            }
            return null;
        }
        public async Task<Dictionary<string, string>> PushOrderToAff(string api_url, long order_id)
        {
            var results = new Dictionary<string, string>();
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiPrefix = api_url;
                var data = new
                {
                    order_id = order_id
                };
                string token = CommonHelper.Encode(JsonConvert.SerializeObject(data), ReadFile.LoadConfig().API_Key);
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("token", token),
                });
                var result = await httpClient.PostAsync(apiPrefix, content);
                dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
                results.Add("Status", (string)resultContent.status);
                results.Add("Msg", (string)resultContent.msg);
                _logger.LogInformation((string)resultContent.msg);
            }
            catch (Exception ex)
            {
                string err = "AppPushAffiliate - PushOrderToAT : " + ex.ToString();
                LogHelper.InsertLogTelegram(err);
                _logger.LogInformation(err);
                results.Add("Status", "2");
                results.Add("Msg", "Error on Excution");
            }
            return results;
        }
    }
}
