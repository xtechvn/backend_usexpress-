using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class OrderDetailOld
    {
        public long Id { get; set; }
        public long? ProductId { get; set; }
        public long OrderId { get; set; }
        public double OriginUnitPrice { get; set; }
        public int Quantity { get; set; }
        public double? Discount { get; set; }
        public int? Status { get; set; }
        public int? SendmailCounter { get; set; }
        public long? DealToDayOrderId { get; set; }
        public string AmazonItemId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public double? TotalPriceVnd { get; set; }
        public double? Rate { get; set; }
        public DateTime? CreateDate { get; set; }
        public string OfferListingId { get; set; }
        public double? PriceAmazon { get; set; }
        public string SellerId { get; set; }
        public decimal? PriceNew { get; set; }
        public decimal? PriceUpdate { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public decimal? ShippingUs { get; set; }
        public decimal? ShippingFirstPound { get; set; }
        public decimal? ShippingProcess { get; set; }
        public decimal? ShippingPound { get; set; }
        public decimal? ShippingLuxury { get; set; }
        public decimal? ShippingExtraFee { get; set; }
        public int? StatusOrderDetail { get; set; }
        public string Note { get; set; }
        public long? ParentOrderId { get; set; }
        public double? ItemWeight { get; set; }
        public string NoteProduct { get; set; }
    }
}
