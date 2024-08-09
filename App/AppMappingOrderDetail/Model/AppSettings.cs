using System;
using System.Collections.Generic;
using System.Text;

namespace AppMappingOrderDetail.Models
{
    public class AppSettings
    {
        public string GROUP_ID_TELEGRAM { get; set; }
        public string BOT_TOKEN_TELEGRAM { get; set; }
        public string EncryptApi { get; set; }
        public string KEY_TOKEN_API { get; set; }
        public string API_GET_ORDER_CHANGE { get; set; }
        public string API_PUSH_TO_QUEUE { get; set; }
        public string API_REMOVE_ORDER_CHANGE_STATUS { get; set; }
        public string API_CORE_URL { get; set; }
    }
}
