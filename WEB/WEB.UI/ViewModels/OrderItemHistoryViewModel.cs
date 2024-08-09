using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.UI.ViewModels
{
    
    public class OrderItemHistoryViewModel
    {
        public long id { get; set; }
        public string orderNo { get; set; }
        public string createdOn { get; set; }
        public int orderStatus { get; set; }
        public double amountVnd { get; set; }
        public int paymentStatus { get; set; }
        public string orderStatusName { get; set; }
        public List<ProductItemHistoryViewModel> product { get; set; }
    }
    public class ProductItemHistoryViewModel
    {
        public string productCode { get; set; }
        
        public string imageThumb { get; set; }
        public string title { get; set; }
        public int quantity { get; set; }
        public string sellerName  { get; set; } // cung cấp bởi
        public string price { get; set; }
        public string LinkSource { get; set; }
    }
}
