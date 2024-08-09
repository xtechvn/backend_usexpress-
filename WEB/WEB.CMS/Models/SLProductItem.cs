using System;
using System.Collections.Generic;
using System.Text;

namespace App_Crawl_SearchList_Receiver.Models
{
    public class SLProductItem
    {
        public int group_id { get; set; }
        public int label_id { get; set; }
        public string url { get; set; }
        public string product_code { get; set; }
        public string from_parent_url { get; set; }
    }
}
