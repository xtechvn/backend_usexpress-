using Entities.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Repositories.IRepositories;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using Utilities;
using Utilities.Contants;

namespace ConsumerPushDataToOldVersion
{
    class Program
    {
        private const int CLIENT_TYPE = 0;
        private const int ORDER_TYPE = 1;

        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder().SetBasePath(
                    Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            var _ClientRepository = serviceProvider.GetService<IClientRepository>();
            var _OrderRepository = serviceProvider.GetService<IOrderRepository>();

            var rabbitMQConfig = config.GetSection("RabbitMQ").Get<RabbitMQConfig>();
            var consumerConfig = config.GetSection("ConsumerConfig").Get<ConsumerConfig>();
            var keyTokenApi = config.GetSection("KEY_TOKEN_API").Get<string>();
            var API_US_NEW = config.GetSection("API_US_NEW").Get<string>();
            var DOMAIN_CMS_NEW = config.GetSection("DOMAIN_CMS_NEW").Get<string>();
            var EncryptApi = config.GetSection("EncryptApi").Get<string>();

            switch (consumerConfig.Type)
            {
                case CLIENT_TYPE:
                    rabbitMQConfig.QueueName = TaskQueueName.client_new_convert_queue;
                    break;
                case ORDER_TYPE:
                    rabbitMQConfig.QueueName = TaskQueueName.order_new_convert_queue;
                    break;
                default:
                    break;
            }

            #region Test API
            //var ClientId = Convert.ToInt64(41958);
            //var _ClientDetail = _ClientRepository.getClientDetail(ClientId).Result;
            //var _AddressClientList = _ClientRepository.GetClientAddressList(ClientId).Result.Where(s => s.IsActive);
            //if (_AddressClientList != null && _AddressClientList.Count() > 0)
            //{
            //    _ClientDetail.DateOfBirth = DateTime.Now;
            //    _ClientDetail.CreateDateIdentity = DateTime.Now;
            //    _ClientDetail.Phone = _AddressClientList.FirstOrDefault().Phone;
            //    _ClientDetail.ClientName = _AddressClientList.FirstOrDefault().ReceiverName;
            //    _ClientDetail.Note = "Đăng ký từ FE New";
            //}
            //var objModel = new
            //{
            //    client_detail = _ClientDetail,
            //    address_client_list = _AddressClientList
            //};
            //Console.WriteLine(" [x] Get Client Succeed at : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            //var token = CommonHelper.Encode(JsonConvert.SerializeObject(objModel), keyTokenApi);

            #endregion

            #region Main
            var factory = new ConnectionFactory()
            {
                UserName = rabbitMQConfig.UserName,
                Password = rabbitMQConfig.Password,
                VirtualHost = rabbitMQConfig.VirtualHost,
                HostName = rabbitMQConfig.HostName,
                Port = rabbitMQConfig.Port != 0 ? rabbitMQConfig.Port : Protocols.DefaultProtocol.DefaultPort
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: rabbitMQConfig.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                Console.WriteLine(" [*] Waiting for messages.");
                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += (model, ea) =>
                {
                    byte[] body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received a message from queue: {0} - {1} at {2}", rabbitMQConfig.QueueName, message, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    try
                    {
                        #region handle message

                        if (consumerConfig.Type == CLIENT_TYPE)
                        {
                            var data_queue = message.Trim();

                            Console.WriteLine(" [x] Message Queue : " + data_queue);

                            var client = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                            long address_id = Convert.ToInt64(client["address_id"].Trim().ToString());
                            long client_id = Convert.ToInt64(client["client_id"].Trim().ToString());

                            var _ClientDetail = _ClientRepository.getClientDetail(client_id).Result;
                            var _AddressClientList = address_id > 0 ? _ClientRepository.GetClientAddressList(client_id).Result.Where(s => s.Id == address_id) : _ClientRepository.GetClientAddressList(client_id).Result.Where(s => s.IsActive);

                            if (_AddressClientList != null && _AddressClientList.Count() > 0)
                            {
                                _ClientDetail.DateOfBirth = DateTime.Now;
                                _ClientDetail.CreateDateIdentity = DateTime.Now;
                                _ClientDetail.ProvinceId = _AddressClientList.FirstOrDefault().ProvinceId;
                                _ClientDetail.DistrictId = _AddressClientList.FirstOrDefault().DistrictId;
                                _ClientDetail.Phone = _AddressClientList.FirstOrDefault().Phone;
                                _ClientDetail.ClientName = _AddressClientList.FirstOrDefault().ReceiverName;
                                _ClientDetail.Note = "Đăng ký từ FE New";
                            }

                            var objModel = new
                            {
                                client_detail = _ClientDetail,
                                address_client_list = _AddressClientList
                            };

                            Console.WriteLine(" [x] Get Client Succeed at : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                            var token = CommonHelper.Encode(JsonConvert.SerializeObject(objModel), keyTokenApi);
                            var result = ApiPostRequest(consumerConfig.ApiUrl, token);

                            Console.WriteLine(" [x] Called Api Client at : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                            if (result.status == ApiStatusType.SUCCESS)
                            {
                                var _ClientMapId = Convert.ToInt64(result.client_id);
                                Console.WriteLine(" [x] return ClientMapId : " + _ClientMapId + " at: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                                if (_ClientMapId > 0)
                                {
                                    _ClientRepository.UpdateClientMapId(client_id, _ClientMapId);
                                }
                                #region sync địa chỉ đơn hàng
                                if (message.IndexOf("order_id") >= 0)
                                {
                                    long order_id = Convert.ToInt64(client["order_id"].Trim().ToString());
                                    // sync order: Thực hiện push thông tin order_id vào để tiến hành sync địa chỉ vừa update
                                    if (order_id > 0)
                                    {
                                        pushOrderIdToQueue(order_id, EncryptApi, API_US_NEW);
                                    }
                                }
                                #endregion

                                Console.WriteLine(" [x] Push data succeed at: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                            }
                            else
                            {
                                Console.WriteLine(" [x] Push data failed : " + result.msg);
                                LogHelper.InsertLogTelegram("JOB Push Client To Old Version Failed: " + token);
                            }
                        }

                        if (consumerConfig.Type == ORDER_TYPE)
                        {
                            // handle order
                            var OrderId = Convert.ToInt64(message.Trim());

                            var objModel = new
                            {
                                order = getOrderDetail(OrderId, keyTokenApi, API_US_NEW), // _OrderRepository.GetOrderDetailForApi(OrderId).Result,
                                order_item = getOrderItem(OrderId, keyTokenApi, API_US_NEW) //_OrderRepository.GetOrderItemList(OrderId).Result,
                            };

                            var token = CommonHelper.Encode(JsonConvert.SerializeObject(objModel), keyTokenApi);
                            var result = ApiPostRequest(consumerConfig.ApiUrl, token);

                            if (result.status == ApiStatusType.SUCCESS)
                            {
                                var _OrderMapId = result.order_map_id == null ? Convert.ToInt64(result.order_id_old) : Convert.ToInt64(result.order_map_id);

                                Console.WriteLine(" [x] return OrderMapId : " + _OrderMapId + " - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                                if (_OrderMapId > 0)
                                {
                                    mappingOrderOldId(OrderId, _OrderMapId, keyTokenApi, API_US_NEW, DOMAIN_CMS_NEW, objModel.order.OrderNo);
                                }
                                Console.WriteLine(" [x] Push data succeed" + " - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                            }
                            else
                            {
                                Console.WriteLine(" [x] Push data failed : " + result.msg);
                                LogHelper.InsertLogTelegram("JOB Push Order To Old Version Failed: " + token);
                            }
                        }

                        #endregion
                        Console.WriteLine(" [x] Done" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(" [x] Failed: " + ex.Message.ToString());
                        LogHelper.InsertLogTelegram("JOB PUSH DATA" + (consumerConfig.Type == CLIENT_TYPE ? "CLIENT" : "ORDER") + " TO OLD VERSION: " + ex.Message);
                    }
                };
                channel.BasicConsume(queue: rabbitMQConfig.QueueName, autoAck: true, consumer: consumer);
                Console.ReadLine();
            }
            #endregion
        }


        public static OrderApiViewModel getOrderDetail(long OrderId, string keyTokenApi, string API_US_NEW)
        {
            try
            {
                string j_param = "{'order_id':" + OrderId + "}";
                var token = CommonHelper.Encode(j_param, keyTokenApi);
                var response_api = ApiPostRequest(API_US_NEW + "/api/order/get-order-detail.json", token);
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    string result = JsonParent[0]["data"].ToString();
                    var data = JsonConvert.DeserializeObject<OrderApiViewModel>(result);

                    return data;
                }
                else
                {
                    string msg = JsonParent[0]["msg"].ToString();
                    LogHelper.InsertLogTelegram("[BOT PUSH DATA TO OLD VERISON] getOrderDetail in OrderRepository" + msg);
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[BOT PUSH DATA TO OLD VERISON] getOrderDetail in OrderRepository " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Lấy ra list sp trong đơn
        /// </summary>
        /// <param name="OrderId"></param>
        /// <param name="keyTokenApi"></param>
        /// <param name="API_US_NEW"></param>
        /// <returns></returns>
        public static List<OrderItemViewModel> getOrderItem(long OrderId, string keyTokenApi, string API_US_NEW)
        {
            try
            {
                string j_param = "{'order_id':" + OrderId + "}";
                var token = CommonHelper.Encode(j_param, keyTokenApi);
                var response_api = ApiPostRequest(API_US_NEW + "/api/order/get-order-item-detail.json", token);
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    string result = JsonParent[0]["data"].ToString();
                    var data = JsonConvert.DeserializeObject<List<OrderItemViewModel>>(result);

                    return data;
                }
                else
                {
                    string msg = JsonParent[0]["msg"].ToString();
                    LogHelper.InsertLogTelegram("[BOT PUSH DATA TO OLD VERISON] getOrderItem in OrderRepository" + msg);
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[BOT PUSH DATA TO OLD VERISON] getOrderItem in OrderRepository " + ex.ToString());
                return null;
            }
        }
        /// <summary>
        /// lưu mã orderid old vào order new
        /// </summary>
        /// <param name="OrderId"></param>
        /// <param name="keyTokenApi"></param>
        /// <param name="API_US_NEW"></param>
        /// <returns></returns>
        public static void mappingOrderOldId(long OrderId, long _OrderMapId, string keyTokenApi, string API_US_NEW, string DOMAIN_CMS_NEW, string order_no)
        {
            try
            {
                string j_param = "{'order_id_new':" + OrderId + ",'order_id_old':" + _OrderMapId + "}";
                var token = CommonHelper.Encode(j_param, keyTokenApi);
                var response_api = ApiPostRequest(API_US_NEW + "/api/order/mapping-order-id-old.json", token);
                var JsonParent = JArray.Parse("[" + response_api + "]");
                string status = JsonParent[0]["status"].ToString();
                if (status == ((int)ResponseType.SUCCESS).ToString())
                {
                    LogHelper.InsertLogTelegram("Đơn " + order_no + " đã được đẩy sang  hệ thống cũ. order id map: " + _OrderMapId + ". Kiểm tra link: " + DOMAIN_CMS_NEW + "/order/detail/" + OrderId);
                }
                else
                {
                    string msg = JsonParent[0]["msg"].ToString();
                    LogHelper.InsertLogTelegram("[BOT PUSH DATA TO OLD VERISON] ERROR API mappingOrderOldId in OrderId = " + OrderId + " - _OrderMapId = " + _OrderMapId + ". Kiểm tra link: " + DOMAIN_CMS_NEW + "/order/detail/" + OrderId);
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[BOT PUSH DATA TO OLD VERISON] getOrderDetail in OrderRepository " + ex.ToString());

            }
        }

        public static dynamic ApiPostRequest(string apiPrefix, string token)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var content = new FormUrlEncodedContent(new[]{
                     new KeyValuePair<string, string>("token", token)
                });
                var rs = httpClient.PostAsync(apiPrefix, content).Result;
                dynamic result = JObject.Parse(rs.Content.ReadAsStringAsync().Result);
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[BOT PUSH DATA TO OLD VERISON] ApiPostRequest " + apiPrefix + " error: " + ex.ToString() + "-- token = " + token);
                return null;
            }

        }


        public static void pushOrderIdToQueue(long order_id, string keyTokenApi, string API_US_NEW)
        {
            try
            {
                //Console.WriteLine(" [x] Push data succeed at: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                var j_param = new Dictionary<string, string>
                {
                    {"data_push",order_id.ToString()},
                    {"type",TaskQueueName.order_new_convert_queue},
                };
                string token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), keyTokenApi);
                var result = ApiPostRequest(API_US_NEW + "/api/QueueService/data-push.json", token);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[BOT PUSH DATA TO OLD VERISON] pushOrderIdToQueue " + order_id + " error: " + ex.ToString());
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
            services.Configure<Entities.ConfigModels.MailConfig>(configuration.GetSection("MailConfig"));

            // add services:
            services.AddTransient<IClientRepository, ClientRepository>();
            services.AddTransient<IOrderRepository, OrderRepository>();
        }

    }
}
