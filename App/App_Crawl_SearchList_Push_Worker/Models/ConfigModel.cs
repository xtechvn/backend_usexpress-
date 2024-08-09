using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace App_Crawl_SearchList_Push_Worker.Models
{
    public class ConfigModel
    {
        public static AppSettings LoadConfig()
        {
            using (StreamReader r = new StreamReader("config.json"))
            {
                string json = r.ReadToEnd();
                var result = JsonConvert.DeserializeObject<AppSettings>(json);
                return result;
            }
        }

    }
    public class SerilogConfig
    {
        public string Path { get; set; }
        public string OutputTemplate { get; set; }
        public string RollingInterval { get; set; }
        public int RetainedFileCountLimit { get; set; }
    }

    public class RabbitMQConfig
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }
        public string QueueName { get; set; }
    }

    public class IPConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
