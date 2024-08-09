using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AppMappingOrder.ViewModels
{
   public class OrderItemModel
    {
        [Key]
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public double OriginUnitPrice { get; set; }
        public int Quantity { get; set; }
        public int Discount { get; set; }
        public int Status { get; set; }
        public int SendMailCounter { get; set; }
        public int DealTodayOrderid { get; set; }
        public string AmazoneItemId { get; set; }
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public string Link { get; set; }
        public double? PriceAmazon { get; set; }
        public double? Rate { get; set; }
        public double? TotalPriceVND { get; set; }
        public DateTime? CreateDate { get; set; }
        public string OfferListingId { get; set; }
        public string SellerId { get; set; }
        public string SellerName { get; set; }
        public double PriceNew { get; set; }
        public double PriceUpdate { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }


        public double ShippingUs { get; set; }
        public double ShippingFirstPound { get; set; }
        public double DiscountShippingFirstPound { get; set; }
        
        public double ShippingProcess { get; set; }
        public double ShippingPound { get; set; }

        public double ShippingLuxury { get; set; }
        public double ShippingExtraFee { get; set; }
        public int StatusOrderDetail { get; set; }

        public string Note { get; set; }
        public int ParentOrderID { get; set; }
        public int ItemWeight { get; set; }
        public string NoteProduct { get; set; }
        public string JProductData { get; set; } // thong tin san pham lay ve duoc duoi dang json
    }
}
