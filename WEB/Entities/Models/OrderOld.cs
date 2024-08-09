using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class OrderOld
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public double TotalPrice { get; set; }
        public string Address { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public int? PaymentType { get; set; }
        public string Code { get; set; }
        public int Status { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
        public double? TotalPriceVnd { get; set; }
        public string Voucher { get; set; }
        public int? PaymentStatus { get; set; }
        public double? TotalPriceSales { get; set; }
        public double? Totalfee { get; set; }
        public double? ShippingOrderFee { get; set; }
        public int? Rate { get; set; }
        public int? StoreId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public double? TotalPriceSaleVnd { get; set; }
        public bool? IsSendAccessTrade { get; set; }
        public int? AdsTracking { get; set; }
        public string BuySuccess { get; set; }
        public string MoveToStore { get; set; }
        public string GoToAirport { get; set; }
        public string SendToVn { get; set; }
        public long? ParentId { get; set; }
        public byte? OrderTest { get; set; }
        public string BankName { get; set; }
        public decimal? PriceVoucherVnd { get; set; }
        public string SplitOrder { get; set; }
    }
}
