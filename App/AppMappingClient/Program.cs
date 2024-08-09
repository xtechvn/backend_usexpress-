
using Entities.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using Utilities;
using Utilities.Contants;

namespace AppMappingClient
{
    class Program
    {
        public static string QUEUE_HOST = ConfigurationManager.AppSettings["QUEUE_HOST"];
        public static string QUEUE_V_HOST = ConfigurationManager.AppSettings["QUEUE_V_HOST"];
        public static string QUEUE_USERNAME = ConfigurationManager.AppSettings["QUEUE_USERNAME"];
        public static string QUEUE_PASSWORD = ConfigurationManager.AppSettings["QUEUE_PASSWORD"];
        public static string QUEUE_PORT = ConfigurationManager.AppSettings["QUEUE_PORT"];
        public static string QUEUE_KEY_API = ConfigurationManager.AppSettings["QUEUE_KEY_API"];
        public static string EncryptApi = ConfigurationManager.AppSettings["EncryptApi"];
        public static string url_usexpress_old = ConfigurationManager.AppSettings["domain_us_old"];
        public static string domain_us_api_new = ConfigurationManager.AppSettings["domain_us_api_new"];
        private static string task_queue = TaskQueueName.client_old_convert_queue;

        static void Main(string[] args)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = QUEUE_HOST,
                    UserName = QUEUE_USERNAME,
                    Password = QUEUE_PASSWORD,
                    VirtualHost = QUEUE_V_HOST,
                    Port = Protocols.DefaultProtocol.DefaultPort
                };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: task_queue,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    Console.WriteLine(" [*] Waiting for messages.");

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (sender, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);

                        #region Get data from Queue & Analys

                        var client = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                        if (client.Count > 0)
                        {

                            string email = client["email"].Trim().ToString();
                            // get detail order: order,orderitem
                            mappingClient(email);

                        }
                        else
                        {
                            // writelog tele
                        }
                        #endregion

                        Console.WriteLine("[" + DateTime.Now.ToString() + "] - [x] Process Success - Data Received {0}", message);
                    };
                    channel.BasicConsume(queue: task_queue,
                                         autoAck: true,
                                         consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AppMappingClient: " + ex.ToString());
                Console.WriteLine("execute Queue error: " + ex.ToString());
                Console.ReadLine();
            }
        }


        private static void mappingClient(string email)
        {
            string client_model = string.Empty;
            string address_client_model = string.Empty;
            try
            {
                #region get order detail by us old
                string url_api_us_old = url_usexpress_old + "ApiUsexpress/getClientDetail";
                string j_param = "{'email':'" + email + "'}";
                string token = CommonHelper.Encode(j_param, EncryptApi);

                string response_api_order = connectApi(url_api_us_old, token);

                if (response_api_order == string.Empty)
                {
                    LogHelper.InsertLogTelegram(email + " empty !!!");

                }

                JArray JsonParent = JArray.Parse("[" + response_api_order + "]");
                string order_status = JsonParent[0]["status"].ToString().Replace("\"", "");

                if (order_status.ToLower() != "success")
                {
                    string msg = JsonParent[0]["msg"].ToString().Replace("\"", "");
                    LogHelper.InsertLogTelegram("email: " + email + " xay ra loi khi get tu US OLD: " + msg + " ==> token =  " + token + "=> enpoint: " + url_api_us_old);

                }
                else
                {
                    client_model = JsonParent[0]["client_model"].ToString();
                    address_client_model = JsonParent[0]["address_client_model"].ToString();
                    var response_push_client = PushClientOldDetail(client_model, address_client_model);
                }

                #endregion
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("mapping client: " + email + "that bai: " + ex.ToString());
            }
        }

        private static bool PushClientOldDetail(string j_data_client, string j_data_address_client)
        {
            string token = string.Empty;
            try
            {
                var clientModel = JsonConvert.DeserializeObject<ClientViewModel>(j_data_client);
                //var addressClientModel = JsonConvert.DeserializeObject<AddressClientViewModel>(j_data_address_client);

                string j_param = "{'client_info': '" + j_data_client + "','address_info': '" + j_data_address_client + "'}";
                token = CommonHelper.Encode(j_param, EncryptApi);

                string url_api_push_order = domain_us_api_new + "api/Client/addnew.json";
                string response_api_order = connectApi(url_api_push_order, token);
                JArray JsonParent = JArray.Parse("[" + response_api_order + "]");
                string order_status = JsonParent[0]["status"].ToString().Replace("\"", "");

                if (order_status.ToLower() == "success")
                {
                   // LogHelper.InsertLogTelegram("email " + clientModel.Email  + " da duoc xu ly thanh cong");
                    return true;
                }
                else
                {
                    string msg = JsonParent[0]["msg"].ToString().Replace("\"", "");
                    LogHelper.InsertLogTelegram(" ==>> push that bai qua endpoint: " + url_api_push_order + ". Msg =" + msg + ". Token = " + token);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("PushOrderOldDetail: " + ex.ToString() + " --> order_data = " + j_data_client);
                return false;
            }
        }


        private static string connectApi(string url, string token)
        {
            try
            {
                string _post = "token=" + token;
                byte[] byteArray = Encoding.UTF8.GetBytes(_post);
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                request.ContentLength = byteArray.Length;
                request.ContentType = "application/x-www-form-urlencoded";
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();

                StreamReader reader = new StreamReader(dataStream);
                string response_api_order = reader.ReadToEnd();

                return response_api_order;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

    }
}
