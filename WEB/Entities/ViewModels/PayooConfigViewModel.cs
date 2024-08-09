using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    public class PayooConfigViewModel
    {
        public string OrderNo { get; set; }
        public string ApiPayooCheckout { get; set; }
        public string ShopID { get; set; }
        public string ShopDomain { get; set; }
        public string BusinessUsername { get; set; }        
        public string ShippingDays { get; set; }
        public string ShopBackUrl { get; set; }
        public string ShopTitle { get; set; }

        public string NotifyUrl { get; set; }
        public string ChecksumKey { get; set; }
        public double TotalAmountLast { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string EmailUsexpress  { get; set; }
        public int CardType { get; set; }  // atm ha visa
        public string BankCode { get; set; } // ngan hang client chọn
        public string OrderDescription { get; set; }
    }
}
