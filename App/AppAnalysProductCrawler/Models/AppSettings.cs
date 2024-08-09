using System;
using System.Collections.Generic;
using System.Text;

namespace AppAnalysProductCrawler.Models
{
    public class AppSettings
    {
        public string GROUP_ID_TELEGRAM { get; set; }
        public string BOT_TOKEN_TELEGRAM { get; set; }
        public string INDEX_ES_PRODUCT { get; set; }
        public string EncryptApi { get; set; }
        public string KEY_ENCODE_TOKEN_PUT_QUEUE { get; set; }
        public string API_RATE_CURRENT { get; set; }
        public string PRODUCT_GROUP_LIST { get; set; }
        public string API_CRAWL_DETAIL_PRODUCT { get; set; }
        public string API_CMS_URL { get; set; }
        public string API_CMS_LIVE_URL { get; set; }
        public string KEY_TOKEN_API { get; set; }
    }
}
