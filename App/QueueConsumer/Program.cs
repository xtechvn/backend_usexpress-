using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Caching.Elasticsearch;
using Caching.RedisWorker;
using Entities.ViewModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using Utilities.Contants;

namespace AppConsummerPushProductToES
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = builder.Build();

            var serilogConfig = config.GetSection("Serilog").Get<SerilogConfig>();
            var rabbitMQConfig = config.GetSection("RabbitMQ").Get<RabbitMQConfig>();
            var elasticConfig = config.GetSection("Elastic").Get<IPConfig>();
            var redisConfig = config.GetSection("Redis").Get<IPConfig>();

            Log.Logger = new LoggerConfiguration().WriteTo
               .File(path: serilogConfig.Path,
                     outputTemplate: serilogConfig.OutputTemplate,
                     rollingInterval: RollingInterval.Day,
                     retainedFileCountLimit: serilogConfig.RetainedFileCountLimit).CreateLogger();

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
                channel.QueueDeclare(queue: TaskQueueName.product_es_queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                Console.WriteLine(" [*] Waiting for messages.");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    byte[] body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received a message from queue: {0}", TaskQueueName.product_es_queue);
                    try
                    {

                        #region push data
                        int result = 0;
                        string StrRedisConfig = $"{redisConfig.Host}:{redisConfig.Port},connectRetry=5";
                        string StrEsConfig = $"{elasticConfig.Host}:{elasticConfig.Port}";
                        IRedisRepository _RedisRepository = new RedisRepository(StrRedisConfig);
                        IESRepository<object> _ESRepository = new ESRepository<object>(StrEsConfig);

                        var json = _RedisRepository.Get(message);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var objModel = JsonConvert.DeserializeObject<EsProductViewModel>(json);
                            if (objModel != null && !objModel.page_not_found)
                            {
                                objModel.product_code = objModel.product_code + "_" + objModel.label_id;
                                result = _ESRepository.UpSert(objModel, "product");
                            }
                        }

                        if (result > 0)
                        {
                            Console.WriteLine(" [x] Push Data Success");
                        }
                        else
                        {
                            Console.WriteLine(" [x] Push Data Error");
                        }
                        #endregion

                        Thread.Sleep(1000);
                        Console.WriteLine(" [x] Done");
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(" [x] Failed: " + ex.Message.ToString());
                        Console.WriteLine(" [x] The message is still keeping");
                        Log.Error(ex.Message.ToString());
                    }
                };
                channel.BasicConsume(queue: TaskQueueName.product_es_queue, autoAck: false, consumer: consumer);
                Console.ReadLine();
            }
            #endregion
        }
    }
}
