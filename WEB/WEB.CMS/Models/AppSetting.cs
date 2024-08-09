using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.CMS.Models
{
    public class AppSettings
    {
        public string GROUP_ID_TELEGRAM { get; set; }
        public string BOT_TOKEN_TELEGRAM { get; set; }
        public string INDEX_ES_PRODUCT { get; set; }
        public string EncryptApi { get; set; }
        public string KEY_ENCODE_TOKEN_PUT_QUEUE { get; set; }
        public string API_ADD_PRODUCT_CLASSIFICATION { get; set; }
        public string API_GET_RATE_USD { get; set; }
        public string API_CREATE_PRODUCT_MANUAL { get; set; }
        public string API_LOCK_PRODUCT_MANUAL { get; set; }
        public string API_SYNC_LOCATION { get; set; }
        public string API_REMOVE_SHIPPING_FEE { get; set; }
        public string API_REMOVE_LUXURY_FEE { get; set; }
        public string API_GET_DETAIL_PRODUCT { get; set; }
        public string API_CRAWL_DETAIL_PRODUCT { get; set; }
        public string API_PUSH_DATA_TO_QUEUE { get; set; }
        public string API_CMS_URL { get; set; }
        public string API_CMS_UPLOAD { get; set; }
        public string KEY_CMS_UPLOAD { get; set; }
        public string API_USEXPRESS { get; set; }
        public string KEY_TOKEN_API { get; set; }
        public string API_RESET_PASSWORD_DEFAULT { get; set; }
        public string API_RATE_CURRENT { get; set; }
        public string API_SYNC_ARTICLE { get; set; }
        public string API_SYNC_CATEGORY { get; set; }
        public string API_SEND_MAIL { get; set; }
        public string API_CLEAR_CACHE { get; set; }
        public string API_CLEAR_CACHE_BY_KEY { get; set; }
        public string API_GET_TOTAL_PRODUCT_CRAWL_TODAY { get; set; }
        public string AT_CREATE_ORDER { get; set; }
        public string AT_UPDATE_ORDER { get; set; }
        public string ADPIA_CREATE_ORDER { get; set; }
        public string ADPIA_UPDATE_ORDER { get; set; }
        public string API_SYNC_ADDRESS_CLIENT { get; set; }
        public string API_LOG_ACTIVITY { get; set; }
        public string API_OLD_KEY { get; set; }
        public string API_OLD_UPDATE_AUTO_PURCHASE { get; set; }

    }
}
