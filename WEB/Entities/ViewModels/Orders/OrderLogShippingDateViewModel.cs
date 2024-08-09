using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Orders
{
    public class OrderLogShippingDateViewModel
    {
        public long Id { get; set; }
        public string OrderNo { get; set; }
        public int LabelId { get; set; }
        public int OrderStatus { get; set; }
        public string OrderStatusName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime PaymentDate { get; set; }
        public int LastestOrderProgressDay { get; set; }
        public int TotalOrderProgressDay { get; set; }
        public int exprire_day_count { get; set; }

    }
}
