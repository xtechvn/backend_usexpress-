using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.UI.ViewModels
{
    public class CartPriceListViewModel
    {
        public string key_cart_id { get; set; }
        public string product_code { get; set; }
        public int label_id { get; set; }
        public double amount_last_vnd { get; set; } // đơn giá của 1 sp
    }
}
