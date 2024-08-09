using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    public class ProductFeeViewModel
    {
        public string label_name { get; set; }
        public double price { get; set; }
       public double total_fee { get; set; }
        public double amount_vnd { get; set; }
        public double shiping_fee { get; set; }
        public Dictionary<string,double> list_product_fee { get; set; }
    }
}
