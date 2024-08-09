using Entities.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using WEB.API.Common;

namespace WEB.API.Service.Queue
{
    public class WorkQueueClient
    {
        //public static string QUEUE_HOST =  ReadFile.LoadConfig().QUEUE_HOST;
        //public static string QUEUE_V_HOST = ReadFile.LoadConfig().QUEUE_V_HOST;
        //public static string QUEUE_USERNAME = ReadFile.LoadConfig().QUEUE_USERNAME;
        //public static string QUEUE_PASSWORD = ReadFile.LoadConfig().QUEUE_PASSWORD;
        //public static int QUEUE_PORT = Convert.ToInt32(ReadFile.LoadConfig().QUEUE_PORT.ToString());
        public bool InsertQueueSimple(QueueSettingViewModel queue_setting,string message, string queueName)
        {            
            var factory = new ConnectionFactory()
            {
                HostName = queue_setting.host,
                UserName = queue_setting.username,
                Password = queue_setting.password,
                VirtualHost = queue_setting.v_host,
                Port = Protocols.DefaultProtocol.DefaultPort
            };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                try
                {
                    channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "",
                                         routingKey: queueName,
                                         basicProperties: null,
                                         body: body);
                    return true;

                }
                catch (Exception ex)
                {
                    LogHelper.InsertLogTelegram("InsertQueueSimple ==> error:  " + ex.Message);
                    return false;
                }
            }
        }

    }
}
