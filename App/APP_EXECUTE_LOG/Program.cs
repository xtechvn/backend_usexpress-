using APP_EXECUTE_LOG.Engines;
using APP_EXECUTE_LOG.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Configuration;
using System.Text;
using System.Threading;
using Utilities;

namespace APP_EXECUTE_LOG
{
    /// <summary>
    /// BOT này sẽ tự động đọc data từ QUEUE chứa json log để gửi lên TELEGRAM
    /// </summary>
    class Program
    {
       
       //-- Push Status:
       private static string STATUS_TELE_PUSH = ConfigurationManager.AppSettings["STATUS_TELE_PUSH"];
       private static string STATUS_ELASHTIC_PUSH = ConfigurationManager.AppSettings["STATUS_ELASHTIC_PUSH"];
       private static string STATUS_LOGDB_PUSH = ConfigurationManager.AppSettings["STATUS_LOGDB_PUSH"];
       //-- RabbitMQ Host:
       public static string QUEUE_HOST = ConfigurationManager.AppSettings["QUEUE_HOST"];
       public static string QUEUE_V_HOST = ConfigurationManager.AppSettings["QUEUE_V_HOST"];
       public static string QUEUE_USERNAME = ConfigurationManager.AppSettings["QUEUE_USERNAME"];
       public static string QUEUE_PASSWORD = ConfigurationManager.AppSettings["QUEUE_PASSWORD"];
       public static string QUEUE_PORT = ConfigurationManager.AppSettings["QUEUE_PORT"];
       public static string QUEUE_KEY_API = ConfigurationManager.AppSettings["QUEUE_KEY_API"];
       private static string QUEUE_NAME = ConfigurationManager.AppSettings["QUEUE_NAME"];
       //-- Elastic Search:
       //-- SQL DB Log:

       static void Main(string[] args)
       {
           PushLogService pushLogService = new PushLogService();
           var factory = new ConnectionFactory()
           {
               UserName = QUEUE_USERNAME,
               Password = QUEUE_PASSWORD,
               VirtualHost = QUEUE_V_HOST,
               HostName = QUEUE_HOST,
               Port = Protocols.DefaultProtocol.DefaultPort
           };

           using (var connection = factory.CreateConnection())
           using (var channel = connection.CreateModel())
           {
               channel.QueueDeclare(queue: QUEUE_NAME, durable: false, exclusive: false, autoDelete: false, arguments: null);
               channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
               Console.WriteLine("Waiting for messages from "+ QUEUE_HOST +" - "+ QUEUE_NAME+" ...");
               var consumer = new EventingBasicConsumer(channel);

               consumer.Received += (model, ea) =>
               {
                   byte[] body = ea.Body.ToArray();
                   var message = Encoding.UTF8.GetString(body);
                   Console.WriteLine("Received: {0} - {1} at {2}", QUEUE_NAME, message, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                   try
                   {
                       LogModel msg_content = JsonConvert.DeserializeObject<LogModel>(message);

                       if (STATUS_TELE_PUSH=="1")
                       {
                           string res_tele = pushLogService.InsertLogTelegram(msg_content);
                           if (res_tele == "")
                           {
                               Console.WriteLine("Telegram - Success: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                           }
                           else
                           {
                               Console.WriteLine("Telegram - Failed: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")+"Error: "+ res_tele);
                           }
                       }
                       if (STATUS_ELASHTIC_PUSH == "1")
                       {
                           string res_elastic = pushLogService.InsertLogElasticSearch(msg_content);
                           if (res_elastic == "")
                           {
                               Console.WriteLine("Elastic - Success: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                           }
                           else
                           {
                               Console.WriteLine("Elastic - Failed: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "Error: " + res_elastic);
                           }
                       }
                       if (STATUS_LOGDB_PUSH == "1")
                       {
                           string res_db = pushLogService.InsertLogDB(msg_content);
                           if (res_db == "")
                           {
                               Console.WriteLine("DB - Success: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                           }
                           else
                           {
                               Console.WriteLine("DB - Failed: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "Error: " + res_db);
                           }
                       }
                       Console.WriteLine("Done - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                   }
                   catch (Exception ex)
                   {
                       Console.WriteLine("Failed: " + ex.Message.ToString());
                       LogHelper.InsertLogTelegram("APP_EXECUTE_LOG - Cannot Excute: " + ex.Message);
                   }
               };
               channel.BasicConsume(queue: QUEUE_NAME, autoAck: true, consumer: consumer);
               Console.ReadLine();
           }
       }
        /*
        static void Main(string[] args)
       {
           LogModel test = new LogModel()
           {
               bot_token= "1372498309:AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k",
               group_id= "-309075192",
               error_content="Test Log Push"
           };
           string encode_key = "1372498309AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k";
           string j_param = JsonConvert.SerializeObject(test);
           string token = CommonHelper.Encode(j_param, encode_key);
           Console.WriteLine(token);
           Thread.Sleep(3000);
           Console.WriteLine("");
       }
        */
    }
}
