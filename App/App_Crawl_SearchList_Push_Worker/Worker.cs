using App_Crawl_SearchList_Push_Worker.Models;
using AppReceiver_Keyword_Analyst.Redis;
using Caching.Elasticsearch;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace App_Crawl_SearchList_Push_Worker
{
    /// <summary>
    /// Bot sẽ được chạy tự động theo lịch trình được set.
    /// Mỗi 1 khoảng time sẽ truy cập vào database get ra những thư mục có trạng thái isAutoCrawl = ON để lấy ra link của sản phẩm theo nhãn
    /// Tiếp đó sẽ push các link đó vào QUEUE để cho BOT tách link xử lý
    /// </summary>
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private static IConfiguration _configuration;
        private static QueueSettingGroupMappingModel queue_setting;
        private static RedisService redis_service;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            ConfigureServices();
            return base.StartAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_configuration["Main_Config:ClearESEnabled"] == "1")
                {
                    UpdateESData(stoppingToken);
                }
                if (_configuration["Main_Config:ClearRedisEnabled"] == "1")
                {
                    UpdateRedisData(stoppingToken);

                }
                int time_delay = 21600;
                try
                {
                    time_delay = Convert.ToInt32(ConfigModel.LoadConfig().delay_time);
                }
                catch (Exception) { }
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                _logger.LogInformation("Time Delay: " + time_delay.ToString() + "s. ");

                try
                {

                    //-- Get Group Product List :
                    var string_output = await GetDataFromAPI(ReadFile.LoadConfig().API_BASE_URL + ReadFile.LoadConfig().API_GET_GROUP_PRODUCT);
                    if (string_output.Contains("Failed"))
                    {
                        Console.WriteLine(ReadFile.LoadConfig().API_BASE_URL + ReadFile.LoadConfig().API_GET_GROUP_PRODUCT + "Cannot access DB or null Output");
                        LogHelper.InsertLogTelegram("App_Crawl_SearchList_Push_Worker - Connect " + ReadFile.LoadConfig().API_BASE_URL + ReadFile.LoadConfig().API_GET_GROUP_PRODUCT
                            + " - Cannot access DB or null Output "+ string_output);
                    }
                    else
                    {

                        var group_product_list = JsonConvert.DeserializeObject<List<GroupProduct>>(string_output);
                        //-- Check config to excute Service:
                        if (_configuration["Main_Config:CrawlGroupProductStoreStatus"] == "1")
                        {
                            await GroupProductStoreExcute(group_product_list);
                        }
                        if (_configuration["Main_Config:CrawlProductClassificationStatus"] == "1")
                        {
                            await ProductClassificationExucte(group_product_list);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string str = "App_Crawl_SearchList_Push_Worker - ExecuteAsync : " + ex.ToString();

                    LogHelper.InsertLogTelegram(str);
                    Console.WriteLine(str);
                }
                Console.Clear();
                Console.WriteLine("Excute Completed. Delay: " + time_delay.ToString() + "s. ");
                await Task.Delay(time_delay * 1000, stoppingToken);

            }
        }
        /// <summary>
        /// Cập nhật thông tin giá cả sản phẩm trên Redis và ES ngoài trang chủ
        /// </summary>
        /// <returns></returns>
        async Task UpdateRedisData(CancellationToken stoppingToken)
        {
            int time_delay = 7200;
            var _es_repository = new ESRepository<object>(_configuration["ElasticSearch"]);
            try
            {
                time_delay = Convert.ToInt32(_configuration["Main_Config:Redis_Clear_Delay"]);
            }
            catch (Exception) { }
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("Cache Clear Delay: "+ time_delay + " s");
                int dbName = Convert.ToInt32(_configuration["ConnectionStrings:db_group_product"]) < 0 ? 2 : Convert.ToInt32(_configuration["ConnectionStrings:db_group_product"]);
                var keys = ConnectionMultiplexer.Connect(_configuration["ConnectionStrings:Redis"]).GetServer(_configuration["ConnectionStrings:Redis"]).Keys(dbName, pattern: "*");
                List<string> key_list = keys.Select(key => (string)key).ToList();
                foreach (var key in key_list)
                {
                    Console.Write("Cache Name: " + key + ".  Push: ");
                    var failed_msg = "Push Failed: ";
                    string value = redis_service.GetCacheRedis(key, dbName, _configuration["ConnectionStrings:Redis"]);
                    if (value == null || value.Trim() == "") continue;
                    List<ProductViewModel> product_list = JsonConvert.DeserializeObject<List<ProductViewModel>>(value);
                    int group_id = product_list[0].group_product_id;
                    try
                    {
                        group_id = Convert.ToInt32(key.Replace(_configuration["Main_Config:cache_name_partten"], ""));
                    }
                    catch (Exception) { }
                    foreach (var product in product_list)
                    {
                        var del_result = _es_repository.DeleteProductByCode("product", product.product_code);
                        Console.Write(" UpdateRedisData - ES Deleted - ");
                        var link = "https://www.amazon.com/dp/" + product.product_code;
                        switch (product.label_id)
                        {
                            case 1: break;
                            case 7: break;
                            default: break;
                        }
                        SLProductItem item = new SLProductItem()
                        {
                            label_id = product.label_id,
                            from_parent_url = "",
                            group_id = group_id,
                            product_code = product.product_code,
                            url = link
                        };
                        //-- Push to Queue API:
                        var result = await PushDataToQueueAPI(ReadFile.LoadConfig().API_LIVE_URL + ReadFile.LoadConfig().API_PUSH_QUEUE, JsonConvert.SerializeObject(item), queue_setting.queue_name_detail);
                        if (result == "Success")
                        {
                            Console.Write(product.product_code + " - Pushed, ");
                        }
                        else
                        {
                            failed_msg += product.product_code + ", ";
                        }
                        await Task.Delay(1000);
                    }
                    if(failed_msg == "Push Failed: ")
                    {
                        failed_msg += "none.";
                    }
                    Console.Write("\n " + failed_msg);
                    ClearCacheData(key, dbName);
                   
                }
                await Task.Delay(time_delay * 1000, stoppingToken);
            }

        }
        async Task UpdateESData(CancellationToken stoppingToken)
        {
            int time_delay = 7200;
            int del_delay = 500;
            List<string> grp_lst = new List<string>();
            List<ProductViewModel> list_product =null;

            var _es_repository = new ESRepository<object>(_configuration["ElasticSearch"]);
            try
            {
                time_delay = Convert.ToInt32(_configuration["Main_Config:ES_Clean_Delay"]);
                del_delay = Convert.ToInt32(_configuration["Main_Config:ES_Delete_Delay_ms"]); 

            }
            catch (Exception) { }
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_configuration["Main_Config:ESCleanMode"].Trim() == "selected")
                {
                    var group_id_remove_duplicate = _configuration["Main_Config:group_id"];
                    if (group_id_remove_duplicate != null && group_id_remove_duplicate.Trim() != "")
                    {
                        Console.WriteLine("Cache Duplicate in group list: " + group_id_remove_duplicate);
                         grp_lst = group_id_remove_duplicate.Trim().Split(",").ToList();
                       
                    }
                }
                else if (_configuration["Main_Config:ESCleanMode"].Trim() == "all") {
                    try
                    {
                        var max_id = Convert.ToInt32(_configuration["Main_Config:HighestGroupID"]);
                        for(int i = 1; i <= max_id; i++)
                        {
                            grp_lst.Add(i.ToString());
                        }
                        Console.WriteLine("Cache Duplicate in group list ALL Count = " + grp_lst.Count);
                    }
                    catch (Exception) { }
                }
                foreach (var str in grp_lst)
                {
                    try
                    {
                        Console.WriteLine("Group- " + str + ": ");
                        var id = Convert.ToInt32(str);
                        list_product = await _es_repository.GetListProductCodeByGroupProduct("product", id);
                        if(list_product!= null && list_product.Count > 0)
                        {
                            Console.WriteLine("Count Distinct Item: " + list_product.Count());
                            foreach (var product in list_product.Skip(20))
                            {
                                if (product.label_id == (int)LabelType.amazon)
                                {
                                    var rs_del = _es_repository.DeleteProductByCodeNew("product", product.product_code);
                                    Console.WriteLine("Delete: " + product.product_code + " - " + rs_del + " - " + DateTime.Now);
                                    if (product.update_last> new DateTime(2021, 06, 01))
                                    {
                                        var item = new SLProductItem()
                                        {
                                            from_parent_url = "",
                                            group_id = product.group_product_id,
                                            label_id = product.label_id,
                                            product_code = product.product_code,
                                            url = "https://www.amazon.com/dp/" + product.product_code.Trim()
                                        };
                                        var queue_push_result = PushDataToQueueAPI(_configuration["API:Queue"], JsonConvert.SerializeObject(item), "group_product_mapping_detail").Result;
                                        Console.Write(queue_push_result+"\n");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine(product.product_code + "-" + product.label_id + " - non AMZ");
                                }
                                await Task.Delay(del_delay, stoppingToken);
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex.ToString());
                        continue;
                    }
                    Console.Clear();
                }
                await Task.Delay(time_delay * 1000, stoppingToken);
            }


        }
        public async Task ClearCacheData(string key, int dbName)
        {
            try
            {
                var delay_time = Convert.ToInt32(_configuration["Main_Config:ClearCacheDelay"]) < 0 ? 1800 : Convert.ToInt32(_configuration["Main_Config:ClearCacheDelay"]);
                await Task.Delay(delay_time);
                redis_service.Remove(key, dbName, _configuration["ConnectionStrings:Redis"]);
                Console.Write(" -- Clear Cache: " + key + " -- ");
            } catch(Exception ex)
            {
                _logger.LogInformation(ex.ToString());
            }
        }
        /// <summary>
        /// Thực thi Push Queue các URL nhóm sản phẩm lưu trong Group_Product_Store
        /// </summary>
        /// <returns></returns>
        async Task GroupProductStoreExcute(List<GroupProduct> group_products)
        {
            try
            {
                var group_product_list = group_products;
                //-- Get Auto Crawl Group Product List:
                // group_product_list = group_product_list.Where(x => x.Status == 0).Where(x => x.IsAutoCrawler == 1).ToList();

                //-- Group Product Store
                string str = await GetDataFromAPI(ReadFile.LoadConfig().API_BASE_URL + ReadFile.LoadConfig().API_GET_GROUP_PRODUCT_STORE);
                if (str == "Failed")
                {
                    Console.WriteLine(ReadFile.LoadConfig().API_BASE_URL + ReadFile.LoadConfig().API_GET_GROUP_PRODUCT_STORE + "Cannot access DB or null Output");
                    LogHelper.InsertLogTelegram("App_Crawl_SearchList_Push_Worker - Connect " + ReadFile.LoadConfig().API_BASE_URL + ReadFile.LoadConfig().API_GET_GROUP_PRODUCT_STORE
                        + " - Cannot access DB or null Output");
                }
                else
                {
                    var group_product_store_list = JsonConvert.DeserializeObject<List<GroupProductStore>>(str);
                    var group_product_store_excute_list = new List<GroupProductStore>();
                    foreach (var group_product in group_product_list)
                    {
                        group_product_store_excute_list.AddRange(group_product_store_list.Where(x => x.GroupProductId == group_product.Id).ToList());
                    }
                    Console.WriteLine("GroupProductStoreURL by CrawlStatus Count=" + group_product_store_excute_list.Count);
                    foreach (var group_product_store_item in group_product_store_excute_list)
                    {
                        switch (group_product_store_item.LabelId)
                        {
                            case 1:
                                {
                                    SLQueueItem item = new SLQueueItem()
                                    {
                                        groupProductid = group_product_store_item.GroupProductId,
                                        labelid = group_product_store_item.LabelId,
                                        linkdetail = group_product_store_item.LinkStoreMenu
                                    };
                                    //-- Push to Queue API:
                                    var result = await PushDataToQueueAPI(ReadFile.LoadConfig().API_LIVE_URL + ReadFile.LoadConfig().API_PUSH_QUEUE, JsonConvert.SerializeObject(item), queue_setting.queue_name);
                                    if (result == "Success")
                                        Console.WriteLine("Pushed: " + group_product_store_item.LinkStoreMenu + " - " + queue_setting.queue_name + " . ");
                                    else Console.WriteLine("Push Failed: " + group_product_store_item.LinkStoreMenu + " - " + queue_setting.queue_name + ". Reason: " + result);
                                    //Thread.Sleep(1000);
                                }
                                break;
                            default: break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string err = "App_Crawl_SearchList_Push_Worker - GroupProductStoreExcute -  Error: " + ex.ToString();
                Console.WriteLine(err);
                LogHelper.InsertLogTelegram(err);
            }

        }
        /// <summary>
        /// Thực thi Push Queue URL sản phẩm set tay trong Product_Classification
        /// </summary>
        /// <returns></returns>
        async Task ProductClassificationExucte(List<GroupProduct> group_products)
        {
            try
            {
                //-- Get Group Product List :
                var group_product_list = group_products;
                //-- Get Auto Crawl Group Product List:
                // group_product_list = group_product_list.Where(x => x.Status == 0).Where(x => x.IsAutoCrawler == 1).ToList();
                //-- Product Classification:
                string str = await GetDataFromAPI(ReadFile.LoadConfig().API_BASE_URL + ReadFile.LoadConfig().API_GET_PRODUCT_CLASSIFICATION);
                if (str .Contains("Failed"))
                {
                    Console.WriteLine(ReadFile.LoadConfig().API_BASE_URL + ReadFile.LoadConfig().API_GET_GROUP_PRODUCT_STORE + "Cannot access DB or null Output");
                    LogHelper.InsertLogTelegram("App_Crawl_SearchList_Push_Worker - Connect " + ReadFile.LoadConfig().API_BASE_URL + ReadFile.LoadConfig().API_GET_GROUP_PRODUCT_STORE
                        + " - Cannot access DB or null Output");
                }
                else
                {
                    var list_product_classification = JsonConvert.DeserializeObject<List<ProductClassification>>(str);
                    List<ProductClassification> non_filter_list = new List<ProductClassification>();
                    //-- Filter, Add Actived-Crawl Product Classification:
                    foreach (var group_product in group_product_list)
                    {
                        non_filter_list.AddRange(list_product_classification.Where(x => x.GroupIdChoice == group_product.Id).ToList());
                    }
                    Console.WriteLine("ProductClassification by CrawlStatus Count=" + non_filter_list.Count);
                    string product_str = String.Join(",", non_filter_list.Cast<ProductClassification>().Select(x => App_Auto_Mapping_Push_Queue.Helper.CommonHelper.GetASINFromURL(x.Link)).ToArray());
                    List<string> excute_product_code_list = await FilterNonExistsProductCode(ReadFile.LoadConfig().API_BASE_URL + ReadFile.LoadConfig().API_FILTER_PCODE_NOT_EXISTS, product_str);
                    excute_product_code_list.RemoveAll(x => x == "");
                    if (excute_product_code_list.Count > 0)
                    {
                        List<ProductClassification> excute_list = new List<ProductClassification>();
                        foreach (var product_code in excute_product_code_list)
                        {
                            excute_list.AddRange(non_filter_list.Where(x => App_Auto_Mapping_Push_Queue.Helper.CommonHelper.GetASINFromURL(x.Link) == product_code).ToList());
                            non_filter_list.RemoveAll(x => App_Auto_Mapping_Push_Queue.Helper.CommonHelper.GetASINFromURL(x.Link) == product_code);
                        }
                        Console.WriteLine("New ProductClassification Count = " + excute_list.Count + " : ");
                        //-- Push Queue
                        foreach (var productClassification in excute_list)
                        {
                            switch (productClassification.LabelId)
                            {
                                case 1:
                                    {
                                        if (_configuration["Main_Config:CrawlProductClassificationWithNoExprire"] == "1")
                                        {
                                            if (productClassification.FromDate != null)
                                            {
                                                //-- URL chưa tới thời điểm cần crawl:
                                                if ((DateTime)productClassification.FromDate > DateTime.Now)
                                                {
                                                    continue;

                                                }
                                            }
                                            if (productClassification.ToDate != null)
                                            {
                                                //-- URL đã quá thời gian xuất hiện:
                                                if (DateTime.Now > (DateTime)productClassification.ToDate)
                                                {
                                                    continue;
                                                }

                                            }
                                        }
                                        string product_code;
                                        if (CommonHelper.CheckAsinByLink(productClassification.Link, out product_code))
                                        {
                                            if (product_code != null && product_code != "")
                                            {
                                                SLProductItem item = new SLProductItem()
                                                {
                                                    url = productClassification.Link,
                                                    group_id = productClassification.GroupIdChoice,
                                                    product_code = product_code,
                                                    from_parent_url = null,
                                                    label_id = (int)productClassification.LabelId
                                                };
                                                //-- Push to Queue API:
                                                var result = await PushDataToQueueAPI(ReadFile.LoadConfig().API_LIVE_URL + ReadFile.LoadConfig().API_PUSH_QUEUE, JsonConvert.SerializeObject(item), queue_setting.queue_name_detail);
                                                if (result == "Success")
                                                    Console.WriteLine("Pushed: " + productClassification.Link + " - " + queue_setting.queue_name_detail + " . ");
                                                else Console.WriteLine("Push Failed: " + productClassification.Link + " - " + queue_setting.queue_name_detail + ". Reason: " + result);
                                                Thread.Sleep(1000);
                                            }
                                            else
                                            {
                                                SLQueueItem item = new SLQueueItem()
                                                {
                                                    groupProductid = productClassification.GroupIdChoice,
                                                    labelid = (int)productClassification.LabelId,
                                                    linkdetail = productClassification.Link
                                                };
                                                //-- Push to Queue API:
                                                var result = await PushDataToQueueAPI(ReadFile.LoadConfig().API_LIVE_URL + ReadFile.LoadConfig().API_PUSH_QUEUE, JsonConvert.SerializeObject(item), queue_setting.queue_name);
                                                if (result == "Success")
                                                    Console.WriteLine("Pushed: " + productClassification.Link + " - " + queue_setting.queue_name + " . ");
                                                else Console.WriteLine("Push Failed: " + productClassification.Link + " - " + queue_setting.queue_name + ". Reason: " + result);

                                                Thread.Sleep(1000);
                                            }

                                        }
                                        else
                                        {
                                            SLQueueItem item = new SLQueueItem()
                                            {
                                                groupProductid = productClassification.GroupIdChoice,
                                                labelid = (int)productClassification.LabelId,
                                                linkdetail = productClassification.Link
                                            };
                                            //-- Push to Queue API:
                                            var result = await PushDataToQueueAPI(ReadFile.LoadConfig().API_LIVE_URL + ReadFile.LoadConfig().API_PUSH_QUEUE, JsonConvert.SerializeObject(item), queue_setting.queue_name);
                                            if (result == "Success")
                                                Console.WriteLine("Pushed: " + productClassification.Link + " - " + queue_setting.queue_name + " . ");
                                            else Console.WriteLine("Push Failed: " + productClassification.Link + " - " + queue_setting.queue_name + ". Reason: " + result);
                                            Thread.Sleep(1000);
                                        }
                                    }
                                    break;
                                case 7:
                                    {

                                    }
                                    break;
                                default: break;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No-new Product in ProductClassification ...");
                    }
                }
            }
            catch (Exception ex)
            {
                string err = "App_Crawl_SearchList_Push_Worker - ProductClassificationExucte -  Error: " + ex.ToString();
                Console.WriteLine(err);
                LogHelper.InsertLogTelegram(err);
            }

        }
        private  void ConfigureServices()
        {
            try
            {
                // build config
                var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).
                         AddJsonFile("appsettings.json").Build();
                var services = new ServiceCollection();
                services.AddOptions();
                // add services:
                services.AddScoped<IConfiguration>(_ => config);
                var serviceProvider = services.BuildServiceProvider();
                // Get Service:
                _configuration = serviceProvider.GetService<IConfiguration>();
                //-- Aditional Service if Config is on:

                //-- RedisService
                var conf = LoadConfiguration();
                var sp = LoadServices(conf);
                redis_service = sp.GetService<RedisService>();

                //-- Queue Setting:
                string select_queue = _configuration["Select_Queue"];
                queue_setting = new QueueSettingGroupMappingModel
                {
                    host = _configuration[select_queue + ":HostName"],
                    v_host = _configuration[select_queue + ":VirtualHost"],
                    port = Convert.ToInt32(_configuration[select_queue + ":Port"])! <= 0 ? Convert.ToInt32(_configuration["RabbitMQ:Port"]) : 5672,
                    username = _configuration[select_queue + ":UserName"],
                    password = _configuration[select_queue + ":Password"],
                    queue_name = _configuration[select_queue + ":QueueName"],
                    queue_name_detail = _configuration[select_queue + ":QueueName_Detail"]
                };
            }
            catch (Exception ex)
            {
                string err = "App_Crawl_SearchList_Push_Worker - ConfigureService -  Error: " + ex.ToString();
                Console.WriteLine(err);
                LogHelper.InsertLogTelegram(err);
            }
        }
        public static async Task<string> PushDataToQueueAPI(string url, string message, string queue_name)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiPrefix = url;

                string j_param = "{'data_push':'" + message + "','type':'" + queue_name + "'}";
                string token = CommonHelper.Encode(j_param, ReadFile.LoadConfig().EncryptApi);
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("token", token),
                });
                var result = await httpClient.PostAsync(apiPrefix, content);
                var rs_content = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Content.ReadAsStringAsync().Result);
                if (rs_content["status"] == ResponseType.SUCCESS.ToString())
                    return "Success";
                else
                    return "Failed - " + rs_content["msg"];
            }
            catch (Exception ex)
            {
                return "Error: " + ex.ToString();
            }
        }
        public async Task<string> GetDataFromAPI(string api_url)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiPrefix = api_url;

                var data = new
                {
                    time = DateTime.Now.ToUniversalTime()
                };
                string token = CommonHelper.Encode(JsonConvert.SerializeObject(data), ReadFile.LoadConfig().EncryptApi);
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("token", token),
                });
                var result = await httpClient.PostAsync(apiPrefix, content);
                var rs_content = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Content.ReadAsStringAsync().Result);
                if (Convert.ToInt32(rs_content["status"]) == (int)ResponseType.SUCCESS)
                    return rs_content["msg"];
                else
                    return "Failed" + result.Content.ReadAsStringAsync().Result;
            }
            catch (Exception ex)
            {
                string err = "App_Crawl_SearchList_Push_Worker - GetDataFromAPI : " + ex.ToString();
                LogHelper.InsertLogTelegram(err);
                Console.WriteLine(err);
                return "Error: " + ex.ToString();
            }
        }
        private async Task<List<string>> FilterNonExistsProductCode(string url, string product_code_string)
        {
            try
            {
                //-- Call API
                HttpClient httpClient = new HttpClient();
                string apiPrefix = url;
                string j_param = "{'product_list_target':'" + product_code_string + "'}";
                string token = Utilities.CommonHelper.Encode(j_param, ReadFile.LoadConfig().API_Key);
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("token", token),
                });
                var result = await httpClient.PostAsync(apiPrefix, content);
                var rs_content = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Content.ReadAsStringAsync().Result);
                //-- If Success:
                if (rs_content["status"] == ((int)ResponseType.SUCCESS).ToString())
                {
                    string pcode = rs_content["data"].Trim().Replace("\"", "");
                    return pcode.Split(",").ToList();
                }
                else
                {
                    //-- status=FAILED,EMPTY,ERROR
                    LogHelper.InsertLogTelegram("App_Auto_Mapping_Push_Queue - FilterNonExistsProductCode: API Return FAILED - Status: " + rs_content["status"]);
                    Console.WriteLine("App_Auto_Mapping_Push_Queue - FilterNonExistsProductCode: API Return FAILED - Status: " + rs_content["status"]);
                    Console.Write("\n Return All Items.");
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("App_Auto_Mapping_Push_Queue - FilterNonExistsProductCode: " + ex.ToString());
                Console.WriteLine("App_Auto_Mapping_Push_Queue - FilterNonExistsProductCode: " + ex.ToString());
                Console.Write("\n Return All Items.");
            }
            //-- Return ALL:
            return null;
        }
        #region connection Redis
        static IServiceProvider LoadServices(IConfiguration configuration)
          => new ServiceCollection()
                  .AddRedis(builder =>
                     builder
                         .SetConfigurationOptions(configuration["ConnectionStrings:Redis"])

                  .SetDBIndex(Convert.ToInt32(configuration["ConnectionStrings:db_product_index"]))
                  )
                  .AddTransient<RedisService>()
                  .BuildServiceProvider();

        static IConfiguration LoadConfiguration()
            => new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")

                    .Build();
        #endregion
    }
}
