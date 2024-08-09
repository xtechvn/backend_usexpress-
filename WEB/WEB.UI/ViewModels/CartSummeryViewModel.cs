using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.UI.ViewModels
{
    public class CartSummeryViewModel
    {
        public double total_amount_cart { get; set; }
        public double total_discount_amount { get; set; }
        public double total_amount_last { get; set; }
        public int label_id { get; set; }

    }
}
