using System;
using System.Collections.Generic;
using System.Text;

namespace App_Crawl_SearchList_Push_Worker.Models
{
    public class AppSettings
    {
        public string ExcuteFromBegin { get; set; }
        public string EncryptApi { get; set; }
        public string API_LIVE_URL { get; set; }
        public string API_Key { get; set; }
        public string API_GET_AFF_ORDERLIST { get; set; }
        public string API_PUSH_ORDER_TO_ACCESSTRADE { get; set; }
        public string API_PUSH_ORDER_TO_ADPIA { get; set; }

        public string token_id { get; set; }
        public string Default_url { get; set; }
        public string delay_time { get; set; }
        public string MongoServer_Host { get; set; }
        public string MongoServer_catalog { get; set; }
    }
}
