using System;
using System.Collections.Generic;
using System.Text;

namespace ConsumerPushDataToOldVersion
{
    public class RabbitMQConfig
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }
        public string QueueName { get; set; }
    }

    public class ConsumerConfig
    {
        /// <summary>
        /// 0 : client
        /// 1 : order 
        /// </summary>
        public int Type { get; set; }
        public string ApiUrl { get; set; }
    }
}
