using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AppMappingOrder.ViewModels
{
    public class OrderModel
    {
        [Key]
        public int OrderId { get; set; }

        public int UserId { get; set; }
        public string CreatedDate { get; set; }
        public double TotalPrice { get; set; }

        public string Address { get; set; }


        public string ProvinceId { get; set; }


        public string DistrictId { get; set; }




        public int Paymentype { get; set; }



        public string CustomerName { get; set; }


        public string Phone { get; set; }


        public string Email { get; set; }



        public string ward_id { get; set; }


        public string Note { get; set; }

        public int PaymentStatus { get; set; } // da thanh toan haychua
        public string code { get; set; }
        
        public int Status { get; set; }
        public string StatusName { get; set; }

        public string CreateOrder { get; set; }

        //public string Payment_Date { get; set; }
        public bool isCreateAccount { get; set; }//  1: tự động tạo tài khoản, 0: không tạo tài khoản

        public int voucher_id { get; set; }
        public double price_voucher { get; set; }// số tiền từ voucher được giảm. Trường này đc cấu hình trong bảng voucher
        
        public string bank_code { get; set; }
        public double? TotalPriceVND { get; set; }
        public double? TotalPriceSales { get; set; }
        public double? TotalFee { get; set; }

        public double? ShippingOrderFee { get; set; }

        public int? rate { get; set; }

        public int StoreId { get; set; }

        public string payment_date { get; set; }
        public double? TotalPriceSaleVnd { get; set; }
        public bool isSendAccessTrade { get; set; }
        public int AdsTracking { get; set; }

        public string BuySuccess { get; set; }

        public string MoveToStore { get; set; }

        public string GoToAirport { get; set; }
        public string SendToVn { get; set; }
        public int ParentId { get; set; }

        public string bank_name { get; set; }
        public double price_voucher_vnd { get; set; }


        public string SplitOrder { get; set; }

        public double TotalDiscount2ndUsd { get; set; }
        public double TotalShippingFeeUsd { get; set; }

        public double TotalDiscount2ndVnd { get; set; }
        public double TotalShippingFeeVnd { get; set; }
        public double TotalDiscountVoucherVnd { get; set; }

        public string TrackingId { get; set; }
        public string UtmMedium { get; set; }
        public string UtmCampaign { get; set; }
        public string UtmSource { get; set; }
        public string UtmFirstTime { get; set; }
        public string voucherCode { get; set; }

        public int TotalChangePayment { get; set; }
    }
}
