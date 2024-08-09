using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class ProductOtherLink
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string LinkOrder { get; set; }
        public string Note { get; set; }
        public double? PriceCheckout { get; set; }
        public double? PriceDyamic { get; set; }
        public int? LabelId { get; set; }
        public string ProductOrderId { get; set; }
    }
}
