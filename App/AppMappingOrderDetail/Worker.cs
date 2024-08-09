using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppMappingOrderDetail.Model;
using AppMappingOrderDetail.Models;
using Entities.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Repositories.Repositories;
using Utilities;
using Utilities.Contants;

namespace AppMappingOrderDetail
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string KEY_TOKEN_API = ReadFile.LoadConfig().KEY_TOKEN_API;
        private readonly string EncryptApi = ReadFile.LoadConfig().EncryptApi;
        // queue này chứa đơn hàng bên hệ thống cũ. Mục đích để push về hệ thống mới
        public const string order_old_convert_queue = "order_old_convert_queue";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessOrderChange();
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(30000, stoppingToken);
            }
        }

        public async Task ProcessOrderChange()
        {
            try
            {
                var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).
                        AddJsonFile("appsettings.json").Build();
                var configuraion = config.GetSection("DataBaseConfig").Get<Entities.ConfigModels.DataBaseConfig>();

                var services = new ServiceCollection();
                ConfigureServices(services);
                var serviceProvider = services.BuildServiceProvider();
                var commonRepository = serviceProvider.GetService<ICommonRepository>();

                var apiGetOrderChange = ReadFile.LoadConfig().API_GET_ORDER_CHANGE;
                var apiPushOrderNoToQueue = ReadFile.LoadConfig().API_PUSH_TO_QUEUE;
                var apiRemoveOrderChangeStatus = ReadFile.LoadConfig().API_REMOVE_ORDER_CHANGE_STATUS;
                var httpClient = new HttpClient();
                //var listOrderStatus = await commonRepository.GetAllCodeByType(AllCodeType.ORDER_STATUS);

                //step 1  Get All những đơn hàng đang chờ xử lý theo api này
                var contentGetAll = new FormUrlEncodedContent(new[]
                  {
                     new KeyValuePair<string, string>("token", ""),
                 });
                var rsgetAllOrder = await httpClient.PostAsync(apiGetOrderChange, contentGetAll);
                var rsData = rsgetAllOrder.Content.ReadAsStringAsync().Result;
                var responseData = JsonConvert.DeserializeObject<ResponseData>(rsData);
                if (responseData.data != null && responseData.data.Any())
                {
                    foreach (var item in responseData.data)
                    {

                        string order_no = item.order_no.Split("_").First(); // order_no lấy ra sẽ là 1 chuỗi phân tách bởi các cột. Ví dụ: *_*                        
                        
                        var j_param = new Dictionary<string, string>
                            {
                                {"data_push",order_no}, // Chuyển đổi sang chuỗi json chứa order info !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                                {"type",order_old_convert_queue},
                            };
                        string token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), EncryptApi);
                        var content = new FormUrlEncodedContent(new[]
                            {
                                new KeyValuePair<string, string>("token", token),
                            });

                        //step 2 Push order_no vào Queue "Đồng Bộ".
                        var result = await httpClient.PostAsync(apiPushOrderNoToQueue, content);
                        var responsePushQueue = JsonConvert.DeserializeObject<ResponseData>
                            (result.Content.ReadAsStringAsync().Result);
                        if (responsePushQueue.status != "SUCCESS")
                        {
                            LogHelper.InsertLogTelegram("push queue fail. Msg = " + responsePushQueue.msg
                                + ". Token = " + responsePushQueue.token);
                        }

                        //step 3 Push QUeue thành công thì push lần lượt
                        //ngược lai các order_no đó vào API 2 để giải phóng
                        string j_param_remove = "{'order_no':'" + item.order_no + "'}";
                        string token_remove = CommonHelper.Encode(j_param_remove, KEY_TOKEN_API);
                        var content_remove = new FormUrlEncodedContent(new[]
                           {
                                new KeyValuePair<string, string>("token", token_remove),
                            });

                        var resultRemoveOrderChange = await httpClient.PostAsync(apiRemoveOrderChangeStatus, content_remove);
                        var rsDetail = resultRemoveOrderChange.Content.ReadAsStringAsync().Result;
                        var resultRemove = JsonConvert.DeserializeObject<ResponseDataRemove>(
                            resultRemoveOrderChange.Content.ReadAsStringAsync().Result);
                        if (!resultRemove.data)
                        {
                            LogHelper.InsertLogTelegram("remove order change fail." + ". Token = " + token_remove);
                        }
                    }
                    // step 4: Lưu history lịch sử trạng thái đơn hàng
                    PushOrderChange(responseData);

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Loi khi chay ham ProcessOrderChange. Ex=" + ex);
            }
        }

        /// <summary>
        /// cuonglv
        /// Lưu lịch sử đơn hàng. Tiến trình của đơn
        /// Khi có sự thay đổi trong bảng Order hệ thống cũ. Sẽ trigger lưu vào bảng OrderChangeStatus
        /// </summary>
        /// <param name="data_order_change"></param>
        private async void PushOrderChange(ResponseData data_order_change)
        {
            try
            {
                var httpClient = new HttpClient();
                var endpoint_api_core = ReadFile.LoadConfig().API_CORE_URL;
                foreach (var item in data_order_change.data)
                {
                    string order_no = item.order_no.Split("_").First(); // order_no lấy ra sẽ là 1 chuỗi phân tách bởi các cột. Ví dụ: *_*       
                    string order_status = item.order_no.Split("_")[1]; // order_status lấy ra sẽ là 1 chuỗi phân tách bởi các cột. Ví dụ: UAM-1D13131_3   
                    string status_update_time = item.order_no.Split("_")[2]; // Thời gian thay đổi status
                    var j_param = new Dictionary<string, string>
                    {
                        {"OrderNo",order_no},
                        {"OrderStatus",order_status},
                        {"CreateDate",status_update_time}
                    };
                    string token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), ReadFile.LoadConfig().KEY_TOKEN_API);
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("token", token),
                    });
                    var result = await httpClient.PostAsync(endpoint_api_core + "/api/orderprogress/push-order-progress.json", content);

                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Loi khi chay ham PushOrderChange. Ex=" + ex + " data =" + JsonConvert.SerializeObject(data_order_change));
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
            services.AddTransient<ICommonRepository, CommonRepository>();
        }
    }
}
