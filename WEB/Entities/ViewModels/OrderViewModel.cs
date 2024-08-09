using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    public class OrderViewModel : Order
    {
        public string Voucher { get; set; }
        public string VoucherDescription { get; set; }
    }

    public class OrderApiViewModel : Order
    {
        public string Voucher { get; set; }
        public string VoucherDescription { get; set; }

        public long ClientMapId { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
    }

    public class OrderItemViewModel
    {
        public long OrderItemMapId { get; set; }
        public long ProductId { get; set; }
        public int LabelId { get; set; }
        public long ProductMapId { get; set; } // productId map voi Id product o DB cu 
        public long OrderId { get; set; } // order id cua db moi sau khi insert thanh cong
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string ProductImage { get; set; }
        public string ProductPath { get; set; }
        public double Price { get; set; }
        public double FirstPoundFee { get; set; }
        public double DiscountShippingFirstPound { get; set; } // Chiết khấu khi mua từ 2 sp trở lên
        public double NextPoundFee { get; set; }
        public double LuxuryFee { get; set; }
        public double ShippingFeeUs { get; set; }
        public int Quantity { get; set; }
        public int SpecialLuxuryId { get; set; }
        public double RateCurrent { get; set; }
        public double Weight { get; set; } // trong luong sp tinh bang POUND
    }

    public class OrderGridModel : Order
    {
        public string OrderStatusName { get; set; }
        public string PaymentStatusName { get; set; }
        public string PaymentTypeName { get; set; }
        public string VoucherCode { get; set; }
    }

    public class OrderSearchModel
    {
        public string OrderNo { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string ClientName { get; set; }
        public string Phone { get; set; }
        public string OrderStatus { get; set; }
        public string VoucherNo { get; set; }
        public string UtmSource { get; set; }
        public string UtmMedium { get; set; }
        public int PaymentType { get; set; }
        public int PaymentStatus { get; set; }
        public string PaymentDate { get; set; }
        public int IsErrorOrder { get; set; }
        public string ProductCode { get; set; }
        public int IsAffialiate { get; set; }
        public int LabelId { get; set; }
    }

    public class OrderDetailViewModel
    {
        public OrderViewModel OrderInfo { get; set; }

        public List<OrderItemViewModel> OrderItemList { get; set; }
    }

    public class RevenueMinMax
    {
        public double Min { get; set; }
        public double Max { get; set; }
    }

    public class OrderReportModel
    {
        public long OrderId { get; set; }
        public string OrderNo { get; set; }
        public string Email { get; set; }
        public double TotalDiscount2ndVnd { get; set; }
        public double TotalDiscountVoucherVnd { get; set; }
        public double PaymentAmount { get; set; }
        public double CashbackAmount { get; set; }
        public double OrderAmount { get; set; }
        public double RateCurrent { get; set; }
        public DateTime PaymentDate { get; set; }
        public string StoreName { get; set; }
        public string UtmSource { get; set; }
        public string UtmMedium { get; set; }
        public string OrderStatusName { get; set; }
        public int USExpressAff { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string USExpressAffEmail { get; set; }

    }

    public class OrderItemReportModel
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double Weight { get; set; }
        public double FirstPoundFee { get; set; }
        public double NextPoundFee { get; set; }
        public double LuxuryFee { get; set; }
    }
    public class OrderAppModel
    {
        public object order { get; set; }
        public object order_detail { get; set; }

    }

    public class OrderKerryProgressModel
    {
        public string status_date { get; set; }
        public string warehouse { get; set; }
        public string delivery_man { get; set; }
        public string phone_number { get; set; }
        public int giaohangthanhcong { get; set; }
    }
    public class OrderLastestProgressExportModel
    {
        public string OrderNo { get; set; }
        public int TotalOrderProgressDay { get; set; }
        public int LastestOrderProgressDay { get; set; }
    }
    public class OrderExpectedExportModel : OrderReportModel
    {
        public int OrderStatus { get; set; }
    }
}
