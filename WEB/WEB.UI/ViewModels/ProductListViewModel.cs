using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.UI.ViewModels
{
    public class ProductListViewModel
    {
        public string image_url { get; set; }
        public string url { get; set; }
        public string product_name { get; set; }
        public double star { get; set; }
        public int reviews_count { get; set; }
        public string url_store { get; set; }
        public double amount { get; set; } // giá về tay
    }
}
