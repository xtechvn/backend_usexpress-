using App_Crawl_Mapping_Receiver_Service_v2.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace App_Crawl_Mapping_Receiver_Service.Models
{
   public class LocalResultModel
   {
        public List<SLProductItem> list_product { get; set; }
        public List<SLQueueItem> list_url { get; set; }
   }
}
