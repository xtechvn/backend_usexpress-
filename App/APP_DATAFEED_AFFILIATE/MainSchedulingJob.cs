using APP_DATAFEED_AFFILIATE.Common;
using APP_DATAFEED_AFFILIATE.ViewModel;
using Chsword;
using FastMember;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using Utilities;

namespace APP_DATAFEED_AFFILIATE
{
    public class MainSchedulingJob : IJob
    {
        /// <summary>
        /// SimpleJOb is just a class that implements IJOB interface. It implements just one method, Execute method
        /// </summary>
        /// 
        private static string keyTokenApi = System.Configuration.ConfigurationManager.AppSettings["keyTokenApi"];
        private static string API_US_NEW = System.Configuration.ConfigurationManager.AppSettings["API_US_NEW"];
        private static string path_file_export = System.Configuration.ConfigurationManager.AppSettings["path_file_export"];
        void IJob.Execute(IJobExecutionContext context)
        {
            try
            {
                Console.WriteLine("In the process of analysis ...");

                // get list datafeed
                var aff_list = getDataFeedAffiliate();

                if (aff_list == null)
                {
                    Console.WriteLine("aff_list null. Hay vao CMS set trong qly nhom hang");
                    Console.ReadLine();
                }

                // group by
                var gr_aff = from m in aff_list
                             group m by m.affType into g
                             select new
                             {
                                 aff_type = g.Key,
                                 group_product = g.ToList()
                             };
                // switch by Affiliate Type
                string s_gr_product_id = string.Empty;
                var dt_feed = new System.Data.DataTable();
                
                
                int total_feed = 0;
                string aff_name = string.Empty;

                foreach (var item in gr_aff) // truy xuat toi từng đối tác
                {
                    string path_name = string.Empty;
                    switch (item.aff_type)
                    {
                        case (int)AffiliateType.accesstrade:
                            aff_name = AffiliateType.accesstrade.ToString();
                            s_gr_product_id = string.Join(",", item.group_product.Select(x => x.groupProductId.ToString()).ToArray());
                            var j_datafeed = getDataFeed(s_gr_product_id, item.aff_type);


                            if (j_datafeed == string.Empty)
                            {
                                Console.WriteLine("Hien tai chua tong hop duoc datafeed trong ES theo nhom hang " + s_gr_product_id + ". Kiem tra lai job Crawler Offline");
                                Console.ReadLine();
                            }
                            var at_list = JsonConvert.DeserializeObject<List<AccesstradeDataFeedViewModel>>(j_datafeed);
                            total_feed = at_list.Count();
                            // gen file csv
                            using (var reader = ObjectReader.Create(at_list))
                            {
                                dt_feed.Load(reader);
                            }

                            //Tạo thư mục nếu chưa có
                            string folder_name = createFolder(path_file_export + @"\" + "ACCESSTRADE");

                            path_name = folder_name + @"\" + "datafeed.csv";
                            

                            break;
                        case (int)AffiliateType.apia:
                            aff_name = AffiliateType.apia.ToString();
                            
                            break;

                        default:
                            CommonHelper.InsertLogTelegram("[BOT APP_DATAFEED_AFFILIATE] MAIN AFF TYPE KHONG THOA MAN ");
                            Console.WriteLine("AFF TYPE KHONG THOA MAN");
                            Console.ReadLine();
                            break;
                    }
                    if (!string.IsNullOrEmpty(path_name))
                    {
                        dt_feed.ToCSV(path_name);
                        Console.WriteLine("[" + DateTime.Now.ToString() + "] Tong hop thanh cong: " + total_feed + " san pham cho nhan hang " + aff_name );
                    }
                }


                Console.ReadLine();
            }
            catch (Exception ex)
            {
                CommonHelper.InsertLogTelegram("[BOT APP_DATAFEED_AFFILIATE] MAIN" + ex.ToString());
                Console.ReadLine();
            }
        }


        private static string createFolder(string folder_name)
        {
            if (!Directory.Exists(folder_name))
            {
                Directory.CreateDirectory(folder_name);
            }
            return folder_name;
        }

        /// <summary>
        /// Lấy ra nhóm hàng được push đi quảng cáo
        /// </summary>
        /// <returns></returns>
        public static List<AffiliateGroupProductViewModel> getDataFeedAffiliate()
        {
            try
            {
                string j_param = "{'id':-1}";
                var token = CommonHelper.Encode(j_param, keyTokenApi);
                var response_api = ApiPostRequest(API_US_NEW + "/api/Affilliate/get-all-affiliate-group-product.json", token);
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                if (status == "0")
                {
                    string result = JsonParent[0]["data"].ToString();
                    var data = JsonConvert.DeserializeObject<List<AffiliateGroupProductViewModel>>(result);

                    return data;
                }
                else
                {
                    string msg = JsonParent[0]["msg"].ToString();
                    CommonHelper.InsertLogTelegram("[BOT APP_DATAFEED_AFFILIATE] getDataFeedAffiliate" + msg);
                }
                return null;
            }
            catch (Exception ex)
            {
                CommonHelper.InsertLogTelegram("[BOT APP_DATAFEED_AFFILIATE] getDataFeedAffiliate " + ex.ToString());
                return null;
            }
        }


        public static string getDataFeed(string s_gr_product_id, int aff_type)
        {
            try
            {
                string j_param = "{'group_id':'" + s_gr_product_id + "','aff_type':" + aff_type + "}";
                var token = CommonHelper.Encode(j_param, keyTokenApi);

                var response_api = ApiPostRequest(API_US_NEW + "/api/Affilliate/get-data-feed.json", token);
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();

                if (status == "0")
                {
                    string result = JsonParent[0]["data"].ToString();
                    //var data = JsonConvert.DeserializeObject(result,)
                    //dynamic json = new JDynamic(result);
                    return result;
                }
                else
                {
                    string msg = JsonParent[0]["msg"].ToString();
                    CommonHelper.InsertLogTelegram("[BOT APP_DATAFEED_AFFILIATE] getDataFeed " + msg);
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                CommonHelper.InsertLogTelegram("[BOT APP_DATAFEED_AFFILIATE] getDataFeed  " + ex.ToString());
                return string.Empty;
            }
        }

        public static dynamic ApiPostRequest(string apiPrefix, string token)
        {
            try
            {
                var httpClient = new HttpClient();
                var content = new FormUrlEncodedContent(new[]{
                     new KeyValuePair<string, string>("token", token)
                });
                var rs = httpClient.PostAsync(apiPrefix, content).Result;
                dynamic result = JObject.Parse(rs.Content.ReadAsStringAsync().Result);
                return result;
            }
            catch (Exception ex)
            {
                CommonHelper.InsertLogTelegram("[BOT APP_DATAFEED_AFFILIATE] ApiPostRequest in OrderRepository " + ex.ToString());
                return null;
            }
        }

    }
}
