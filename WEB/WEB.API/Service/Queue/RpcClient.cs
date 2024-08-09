using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WEB.API.ViewModels;
using WEB.API.Common;
public class RpcClient
{
    public static string QUEUE_HOST = ReadFile.LoadConfig().QUEUE_HOST;
    public static string QUEUE_V_HOST = ReadFile.LoadConfig().QUEUE_V_HOST;
    public static string QUEUE_USERNAME = ReadFile.LoadConfig().QUEUE_USERNAME;
    public static string QUEUE_PASSWORD = ReadFile.LoadConfig().QUEUE_PASSWORD;
    public static int QUEUE_PORT = Convert.ToInt32(ReadFile.LoadConfig().QUEUE_PORT.ToString());
    public static string QUEUE_KEY_API = ReadFile.LoadConfig().QUEUE_KEY_API;

    private const string QUEUE_NAME = nameof(QueueRPcViewModel) + "_queue_rpc";

    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly string replyQueueName;
    private readonly EventingBasicConsumer consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper =
                new ConcurrentDictionary<string, TaskCompletionSource<string>>();

    public RpcClient()
    {
        var factory = new ConnectionFactory()
        {
            HostName = QUEUE_HOST,
            UserName = QUEUE_USERNAME,
            Password = QUEUE_PASSWORD,
            VirtualHost = QUEUE_V_HOST,
            Port = Protocols.DefaultProtocol.DefaultPort

        };


        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        replyQueueName = channel.QueueDeclare().QueueName;
        consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<string> tcs))
                return;
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            tcs.TrySetResult(response);
        };
    }

    public Task<string> CallAsync(string message, CancellationToken cancellationToken = default(CancellationToken))
    {
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);

        channel.BasicPublish(
            exchange: "",
            routingKey: QUEUE_NAME,
            basicProperties: props,
            body: messageBytes);

        channel.BasicConsume(
            consumer: consumer,
            queue: replyQueueName,
            autoAck: true);

        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        return tcs.Task;
    }

    public void Close()
    {
        connection.Close();
    }
}

//public class Rpc
//{
//    public static void Main(string[] args)
//    {
//        Console.WriteLine("RPC Client");
//        string n = args.Length > 0 ? args[0] : "30";
//        Task t = InvokeAsync(n);
//        t.Wait();

//        Console.WriteLine(" Press [enter] to exit.");
//        Console.ReadLine();
//    }

//    private static async Task InvokeAsync(string n)
//    {
//        var rpcClient = new RpcClient();

//        Console.WriteLine(" [x] Requesting fib({0})", n);
//        var response = await rpcClient.CallAsync(n.ToString());
//        Console.WriteLine(" [.] Got '{0}'", response);

//        rpcClient.Close();
//    }
//}