using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppLandingPage.Models;
using Caching.RedisWorker;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Repositories.Repositories;
using Utilities;
using Utilities.Contants;

namespace AppLandingPage
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
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    Console.Clear();
                    int time_delay = 14400;
                    try
                    {
                        time_delay = Convert.ToInt32(ReadFile.LoadConfig().delay_time);
                        Console.WriteLine("Time Delay: " + time_delay.ToString() + "s. ");

                    }
                    catch (Exception)
                    {

                    }
                    await ProcessRunAppLandingPage();
                    Console.WriteLine("Completed. Delay: " + time_delay.ToString() + "s. ");
                    await Task.Delay(time_delay * 1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AppCrawl_Product_Classification - ExecuteAsync: " + ex);
                Console.WriteLine("AppCrawl_Product_Classification - ExecuteAsync: " + ex);
            }
        }
        async Task ProcessRunAppLandingPage()
        {
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).
                    AddJsonFile("appsettings.json").Build();
            var configuraion = config.GetSection("DataBaseConfig").Get<Entities.ConfigModels.DataBaseConfig>();

            var services = new ServiceCollection();
            var _RedisService = new RedisConn(config);
            _RedisService.Connect();
            var redisConfig = config.GetSection("Redis").Get<IPConfig>();
            string StrRedisConfig = $"{redisConfig.Host}:{redisConfig.Port},connectRetry=5";

            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            var campaign_group_list = await campaignGroupProductRepository.GetAll();
            campaign_group_list = campaign_group_list.Where(n => n.Status == 0).ToList();
            campaign_group_list = campaign_group_list.OrderBy(o => o.OrderBox).ToList();
            /* var group_product_list = await group_product_repository.GetAll();
               group_product_list = group_product_list.Where(n => n.Priority != null).ToList();
               group_product_list = group_product_list.OrderBy(o => o.Priority).ToList();*/
            if (campaign_group_list.Count < 1)
            {
                Console.WriteLine("Can't find any group_product config. Crawl All Item...");
                List<ProductClassification> list_product_classification = await productClassificationRepository.GetAll();
                List<int> groupidlist = new List<int>();
                foreach (var item in list_product_classification)
                {
                    groupidlist.Add(item.GroupIdChoice);
                }
                groupidlist = groupidlist.Distinct().ToList();
                foreach (var group_ID in groupidlist)
                {
                    Console.Write("Group ID: " + group_ID + "  ");
                    var list_product_by_group = new List<ProductViewModel>();
                    var list_product_classification_by_group_id = await productClassificationRepository.GetByProductGroupId(group_ID);
                    if (list_product_classification_by_group_id != null)
                    {
                        foreach (var product_item in list_product_classification_by_group_id)
                        {

                            switch (product_item.LabelId)
                            {
                                case 1:
                                    {
                                        var asin = "";
                                        var rs = CommonHelper.CheckAsinByKeyword(product_item.Link, out asin);
                                        var productViewModel = await CrawlData(asin.Replace("/", ""), product_item.Link, product_item.GroupIdChoice);
                                        if (productViewModel != null)
                                        {
                                            Console.Write("Added: " + productViewModel.product_code + " - PNF = " + productViewModel.page_not_found.ToString() + ". ");
                                            list_product_by_group.Add(productViewModel);
                                        }
                                        Thread.Sleep(2000);
                                    }
                                    break;
                                case 7:
                                    {

                                    }
                                    break;
                                default: break;
                            }


                        }
                        if (list_product_by_group.Any())
                        {
                            _RedisService.Set("GROUP_PRODUCT_" + group_ID, "", int.Parse(ReadFile.LoadConfig().db_folder));
                            _RedisService.Set("GROUP_PRODUCT_" + group_ID, JsonConvert.SerializeObject(list_product_by_group), int.Parse(ReadFile.LoadConfig().db_folder));
                            Console.WriteLine("\n Cached. GROUP_PRODUCT_" + group_ID);
                        }
                    }
                    else
                    {
                        Console.Write("Can't finds any item.");
                    }
                }
            }
            else
            {
                Console.WriteLine("Group product List Count = " + campaign_group_list.Count);
                foreach (var groupID in campaign_group_list)
                {
                    Console.Write("Campaign ID: " + groupID.CampaignId + "  ");
                    if (groupID.Status == 0)
                    {
                        var list_product_by_group = new List<ProductViewModel>();
                        var list_product_classification_by_group_id = await productClassificationRepository.GetByProductGroupId(groupID.CampaignId);
                        if (list_product_classification_by_group_id != null && list_product_classification_by_group_id.Count > 0)
                        {
                            foreach (var product_item in list_product_classification_by_group_id)
                            {

                                switch (product_item.LabelId)
                                {
                                    case 1:
                                        {
                                            var asin = "";
                                            var rs = CommonHelper.CheckAsinByKeyword(product_item.Link, out asin);
                                            var productViewModel = await CrawlData(asin.Replace("/", ""), product_item.Link, product_item.GroupIdChoice);
                                            if (productViewModel != null)
                                            {
                                                Console.Write("Added: " + productViewModel.product_code + " - PNF = " + productViewModel.page_not_found.ToString() + ". ");
                                                list_product_by_group.Add(productViewModel);
                                            }
                                            Thread.Sleep(2000);
                                        }
                                        break;
                                    case 7:
                                        {

                                        }
                                        break;
                                    default: break;
                                }


                            }
                            if (list_product_by_group.Any())
                            {
                                _RedisService.Set("GROUP_PRODUCT_" + groupID.CampaignId, "", int.Parse(ReadFile.LoadConfig().db_folder));
                                _RedisService.Set("GROUP_PRODUCT_" + groupID.CampaignId, JsonConvert.SerializeObject(list_product_by_group), int.Parse(ReadFile.LoadConfig().db_folder));
                                Console.WriteLine("\n Cached. GROUP_PRODUCT_" + groupID.CampaignId);
                            }
                        }
                        else
                        {
                            Console.Write("Can't finds any item.");
                        }
                    }
                    else
                    {
                        Console.Write("Deactived. \n");
                    }
                }
            }

        }

        public static async Task<ProductViewModel> CrawlData(string ASIN, string link, int groupId)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiPrefix = ReadFile.LoadConfig().API_CMS_LIVE_URL + ReadFile.LoadConfig().API_CRAWL_DETAIL_PRODUCT;

                string j_param = "{'product_code':'" + ASIN + "','url':'" + link + "','shop_id':'app_crawl_page'}";
                string token = CommonHelper.Encode(j_param, ReadFile.LoadConfig().KEY_TOKEN_API);
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("token", token),
                });
                var result = await httpClient.PostAsync(apiPrefix, content);
                var product_detail = JsonConvert.DeserializeObject<ProductViewModel>
                    (result.Content.ReadAsStringAsync().Result);
                return product_detail;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // build config
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            services.AddOptions();
            services.Configure<Entities.ConfigModels.DataBaseConfig>(configuration.GetSection("DataBaseConfig"));

            // add services:
        }
        public static async Task<string> GetDataFromAPI(string api_url)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var apiPrefix = api_url;

                string j_param = "{'time':'" + DateTime.Now.ToUniversalTime() + "'}";
                string token = CommonHelper.Encode(j_param, ReadFile.LoadConfig().EncryptApi);
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("token", token),
                });
                var result = await httpClient.PostAsync(apiPrefix, content);
                var rs_content = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Content.ReadAsStringAsync().Result);
                if (Convert.ToInt32(rs_content["status"]) == (int)ResponseType.SUCCESS)
                    return rs_content["msg"];
                else
                    return "Failed";
            }
            catch (Exception ex)
            {
                string err = "App_Crawl_SearchList_Push_Worker - GetDataFromAPI : " + ex.ToString();
                LogHelper.InsertLogTelegram(err);
                Console.WriteLine(err);
                return "Error: " + ex.ToString();
            }
        }

    }
}
