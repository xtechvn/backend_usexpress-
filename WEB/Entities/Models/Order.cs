using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderItem = new HashSet<OrderItem>();
        }

        public long Id { get; set; }
        public long? ClientId { get; set; }
        public int? UserId { get; set; }
        public short? LabelId { get; set; }
        public string OrderNo { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime? CreatedOn { get; set; }
        public double? RateCurrent { get; set; }
        public double? PriceVnd { get; set; }
        public double? AmountVnd { get; set; }
        public double? TotalDiscount2ndVnd { get; set; }
        public double? TotalShippingFeeVnd { get; set; }
        public double? TotalDiscountVoucherVnd { get; set; }
        public long? VoucherId { get; set; }
        public double? Discount { get; set; }
        public double? PriceUsd { get; set; }
        public double? AmountUsd { get; set; }
        public double? TotalDiscount2ndUsd { get; set; }
        public double? TotalShippingFeeUsd { get; set; }
        public double? TotalDiscountVoucherUsd { get; set; }
        public string Note { get; set; }
        public short? PaymentType { get; set; }
        public short? PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
        public int? OrderStatus { get; set; }
        public string TrackingId { get; set; }
        public string UtmMedium { get; set; }
        public string UtmCampaign { get; set; }
        public string UtmFirstTime { get; set; }
        public string UtmSource { get; set; }
        public DateTime? UpdateLast { get; set; }
        public long? OrderMapId { get; set; }
        public short? Version { get; set; }
        public string VoucherName { get; set; }
        public long? AddressId { get; set; }
        public int? IsDelete { get; set; }

        public virtual Client Client { get; set; }
        public virtual ICollection<OrderItem> OrderItem { get; set; }
    }
}
