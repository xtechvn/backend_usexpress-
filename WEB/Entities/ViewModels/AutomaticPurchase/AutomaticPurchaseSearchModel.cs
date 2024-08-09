using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.AutomaticPurchase
{
    public class AutomaticPurchaseSearchModel
    {

        public long Id { get; set; }
        public string OrderNo { get; set; }
        public string ProductCode { get; set; } 
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public int PurchaseStatus { get; set; }
    }
}
