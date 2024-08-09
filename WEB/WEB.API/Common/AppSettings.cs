using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.API.Common
{
    public class AppSettings
    {
        public string GROUP_ID_TELEGRAM { get; set; }
        public string BOT_TOKEN_TELEGRAM { get; set; }
        public string INDEX_ES_PRODUCT { get; set; }
        public string EncryptApi { get; set; }

        public string QUEUE_HOST { get; set; }
        public string QUEUE_USERNAME { get; set; }
        public string QUEUE_PASSWORD { get; set; }
        public string QUEUE_V_HOST { get; set; }
        public string QUEUE_PORT { get; set; }
        public string QUEUE_KEY_API { get; set; }
        public string API_IMG_UPLOAD { get; set; }
        public string Interested_Product_Cache_Name { get; set; }
        public string Kerry_Order_API_Key { get; set; }
        public string USExpressAppKey { get; set; }

    }
}
