using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppLandingPage.Models;
using Caching.RedisWorker;
using Entities.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Repositories.Repositories;
using Utilities;

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
                    await ProcessRunAppLandingPage();
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(300000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AppLandingPage - ExecuteAsync: " + ex);
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
            var productClassificationRepository = serviceProvider.GetService<IProductClassificationRepository>();
            var campaignAdsRepository = serviceProvider.GetService<ICampaignAdsRepository>();

            var listCampaign = campaignAdsRepository.GetAll();

            foreach (var item in listCampaign)
            {
                var listProductClassification = await productClassificationRepository.GetByCapgianId(item.Id);
                //lấy ra danh sách các group trong link
                var listGroupIdChoice = listProductClassification.Select(n => n.GroupIdChoice).Distinct().ToList();
                foreach (var groupId in listGroupIdChoice)
                {
                    //lấy các product dc cấu hình với group
                    var listProductClassificationByGroupId = listProductClassification.Where(n => n.GroupIdChoice == groupId).ToList();
                    var listProductViewModel = new List<ProductViewModel>();
                    foreach (var productClass in listProductClassificationByGroupId)
                    {
                        var asin = "";
                        var rs = CommonHelper.CheckAsinByKeyword(productClass.Link, out asin);
                        var productViewModel = await CrawlData(asin.Replace("/", ""), productClass.Link, productClass.GroupIdChoice);
                        if (productViewModel != null)
                        {
                            Console.WriteLine("Da add sp " + productViewModel.product_code + " vao folder " + groupId);
                            listProductViewModel.Add(productViewModel);
                        }
                        Thread.Sleep(2000);
                    }
                    if (listProductViewModel.Any())
                    {
                        _RedisService.Set("CAMPAIGN_" + item.Id + "_FOLDER_" + groupId, "", int.Parse(ReadFile.LoadConfig().db_folder));
                        _RedisService.Set("CAMPAIGN_" + item.Id + "_FOLDER_" + groupId, JsonConvert.SerializeObject(listProductViewModel), int.Parse(ReadFile.LoadConfig().db_folder));
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
            services.AddSingleton<ICampaignAdsRepository, CampaignAdsRepository>();
            services.AddTransient<IProductClassificationRepository, ProductClassificationRepository>();
        }

        async Task BackUpAsync()
        {
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).
                       AddJsonFile("appsettings.json").Build();
            var configuraion = config.GetSection("DataBaseConfig").Get<Entities.ConfigModels.DataBaseConfig>();

            var services = new ServiceCollection();
            var _RedisService = new RedisConn(config);
            _RedisService.Connect();
           // ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            var productClassificationRepository = serviceProvider.GetService<IProductClassificationRepository>();

            var redisConfig = config.GetSection("Redis").Get<IPConfig>();
            string StrRedisConfig = $"{redisConfig.Host}:{redisConfig.Port},connectRetry=5";

            var pRODUCT_GROUP_LIST = ReadFile.LoadConfig().PRODUCT_GROUP_LIST;

            if (!string.IsNullOrEmpty(pRODUCT_GROUP_LIST))
            {
                var listGroup = pRODUCT_GROUP_LIST.Split(",");
                foreach (var item in listGroup)
                {
                    var listItem = await productClassificationRepository.GetByProductGroupId(int.Parse(item));
                    var listProductViewModel = new List<ProductViewModel>();
                    if (listItem.Any())
                    {
                        foreach (var productClass in listItem)
                        {
                            var asin = "";
                            var rs = CommonHelper.CheckAsinByKeyword(productClass.Link, out asin);
                            var productViewModel = await CrawlData(asin.Replace("/", ""), productClass.Link, int.Parse(item));
                            if (productViewModel != null)
                            {
                                Console.WriteLine("Da set cache cho sp " + productViewModel.product_code + " folder " + item);
                                listProductViewModel.Add(productViewModel);
                            }
                            Thread.Sleep(3000);
                        }
                    }
                    if (listProductViewModel.Any())
                    {
                        _RedisService.Set("FOLDER_" + int.Parse(item), "", int.Parse(ReadFile.LoadConfig().db_folder));
                        //_RedisRepository.Set("FOLDER_" + int.Parse(item), JsonConvert.SerializeObject(listProductViewModel));
                        _RedisService.Set("FOLDER_" + int.Parse(item), JsonConvert.SerializeObject(listProductViewModel), int.Parse(ReadFile.LoadConfig().db_folder));
                    }
                }
            }
        }
    }
}
