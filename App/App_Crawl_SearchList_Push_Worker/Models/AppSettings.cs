using System;
using System.Collections.Generic;
using System.Text;

namespace App_Crawl_SearchList_Push_Worker.Models
{
    public class AppSettings
    {
        public string EncryptApi { get; set; }
        public string API_BASE_URL { get; set; }
        public string API_LIVE_URL { get; set; }
        public string API_PUSH_QUEUE { get; set; }
        public string API_GET_GROUP_PRODUCT { get; set; }
        public string API_GET_GROUP_PRODUCT_STORE { get; set; }
        public string API_GET_PRODUCT_CLASSIFICATION { get; set; }
        public string API_FILTER_PCODE_NOT_EXISTS { get; set; }
        public string API_Key { get; set; }
        public string token_id { get; set; }
        public string Default_url { get; set; }
        public string delay_time { get; set; }
    }
}
