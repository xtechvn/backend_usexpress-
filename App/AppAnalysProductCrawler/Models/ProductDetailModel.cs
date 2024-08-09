using System;
using System.Collections.Generic;
using System.Text;

namespace AppAnalysProductCrawler.Models
{
    public class ProductDetailModel
    {
        public string product_code { get; set; }
        public string url { get; set; }
        public string shop_id { get; set; }
        public string cache_name { get; set; }
        public int label_id { get; set; }
    }
}
