using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class OrderItem
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public double Price { get; set; }
        public int? Quantity { get; set; }
        public double? Weight { get; set; }
        public double? FirstPoundFee { get; set; }
        public double? DiscountShippingFirstPound { get; set; }
        public double? NextPoundFee { get; set; }
        public double? ShippingFeeUs { get; set; }
        public double? LuxuryFee { get; set; }
        public DateTime? CreateOn { get; set; }
        public DateTime? UpdateLast { get; set; }
        public string ImageThumb { get; set; }
        public long? OrderItempMapId { get; set; }
        public int? SpecialLuxuryId { get; set; }
        public int? ProductType { get; set; }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
