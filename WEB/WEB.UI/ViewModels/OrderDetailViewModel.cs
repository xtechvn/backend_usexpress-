using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.UI.ViewModels
{
    
    public class OrderDetailViewModel
    {
        public long id { get; set; }
        public string orderNo { get; set; }
        public long orderId { get; set; }
        public string createdOn { get; set; }
        public int orderStatus { get; set; }
        public string priceVnd { get; set; }
        public string amountVnd { get; set; }
        public string orderStatusName { get; set; }
        
        public string totalDiscount { get; set; }
        public int paymentType { get; set; }
        public string paymentTypeName { get; set; }
        public string clientName { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string storeName { get; set; }
       // public double rateCurrent { get; set; }
        public string receiver_name { get; set; }
        public int paymentStatus { get; set; }
        public int labelId { get; set; }
        public string note { get; set; }
        public List<ProductItemHistoryViewModel> productList { get; set; }
        public int addressId { get; set; }
    }
    
}
