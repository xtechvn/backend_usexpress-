using System;
using System.Collections.Generic;

namespace WinAppCheckoutAmazon.DBContext
{

    public partial class AmazonCart
    {
        public int Id { get; set; }
        public string Account { get; set; }
        public long OrderId { get; set; }
        public string OrderCode { get; set; }
        public Nullable<long> OrderDetailId { get; set; }
        public string CartId { get; set; }
        public string Hmac { get; set; }
        public string PurchaseURL { get; set; }
        public string URLEncodedHMAC { get; set; }
        public string SellerId { get; set; }
        public string ASIN { get; set; }
        public Nullable<int> Quantity { get; set; }
        public string Amount { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<int> ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public Nullable<bool> IsReceived { get; set; }
        public Nullable<bool> IsAddOnItem { get; set; }
        public string Note { get; set; }
        public string AmazonOrderId { get; set; }
        public string AmazonPurchaseId { get; set; }
        public string AmazonTrackingId { get; set; }
        public string AmazonItemId { get; set; }
        public string AmazonShipmentId { get; set; }
        public string AmazonBuyDoneUrl { get; set; }
        public string AmazonDetailUrl { get; set; }
        public string AmazonTrackPackageUrl { get; set; }
        public Nullable<bool> AmazonOrdered { get; set; }
        public Nullable<bool> AmazonShipped { get; set; }
        public Nullable<bool> AmazonOutForDelivery { get; set; }
        public Nullable<bool> AmazonArriving { get; set; }
        public string AmazonOrderSummary { get; set; }
        public string AmazonEstimatedDelivery { get; set; }
        public string AmazonScreenShot { get; set; }
        public string AmazonCartCreateError { get; set; }

        public virtual Order Order { get; set; }
    }
    public partial class Order
    {
        public Order()
        {
            this.BankingTransfers = new HashSet<BankingTransfer>();
            this.OrderDetails = new HashSet<OrderDetail>();
            this.AmazonCarts = new HashSet<AmazonCart>();
        }

        public long Id { get; set; }
        public long UserId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public double TotalPrice { get; set; }
        public string Address { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public Nullable<int> PaymentType { get; set; }
        public string Code { get; set; }
        public int Status { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
        public Nullable<double> TotalPriceVND { get; set; }
        public string Voucher { get; set; }
        public Nullable<int> PaymentStatus { get; set; }
        public Nullable<double> Totalfee { get; set; }
        public Nullable<double> TotalPriceSales { get; set; }
        public Nullable<int> rate { get; set; }
        public Nullable<int> StoreId { get; set; }
        public Nullable<System.DateTime> payment_date { get; set; }
        public Nullable<double> TotalPriceSaleVnd { get; set; }
        public Nullable<bool> isSendAccessTrade { get; set; }
        public Nullable<int> AdsTracking { get; set; }
        public string BuySuccess { get; set; }
        public string MoveToStore { get; set; }
        public string GoToAirport { get; set; }
        public string SendToVn { get; set; }
        public Nullable<long> ParentId { get; set; }
        public Nullable<byte> order_test { get; set; }
        public string bank_name { get; set; }
        public Nullable<decimal> price_voucher_vnd { get; set; }
        public string SplitOrder { get; set; }
        public Nullable<double> ShippingOrderFee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BankingTransfer> BankingTransfers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AmazonCart> AmazonCarts { get; set; }
    }
    public partial class BankingTransfer
    {
        public int Id { get; set; }
        public Nullable<long> OrderId { get; set; }
        public Nullable<System.DateTime> TransDate { get; set; }
        public string OrderNumber { get; set; }
        public string DebitAccount { get; set; }
        public Nullable<double> DebitAmount { get; set; }
        public string CreditAccount { get; set; }
        public Nullable<double> CreditAmount { get; set; }
        public string BeneficiaryName { get; set; }
        public string DetailsOfPayment { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<byte> Status { get; set; }

        public virtual Order Order { get; set; }
    }
    public partial class OrderDetail
    {
        public long Id { get; set; }
        public Nullable<long> ProductId { get; set; }
        public long OrderId { get; set; }
        public double OriginUnitPrice { get; set; }
        public int Quantity { get; set; }
        public Nullable<double> Discount { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<int> SendmailCounter { get; set; }
        public Nullable<long> DealToDayOrderId { get; set; }
        public string AmazonItemId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public Nullable<double> TotalPriceVND { get; set; }
        public Nullable<double> Rate { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public string OfferListingId { get; set; }
        public Nullable<double> PriceAmazon { get; set; }
        public string seller_id { get; set; }
        public Nullable<decimal> PriceNew { get; set; }
        public Nullable<decimal> PriceUpdate { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public Nullable<decimal> ShippingUs { get; set; }
        public Nullable<decimal> ShippingFirstPound { get; set; }
        public Nullable<decimal> ShippingProcess { get; set; }
        public Nullable<decimal> ShippingPound { get; set; }
        public Nullable<decimal> ShippingLuxury { get; set; }
        public Nullable<decimal> ShippingExtraFee { get; set; }
        public Nullable<int> StatusOrderDetail { get; set; }
        public string Note { get; set; }
        public Nullable<long> ParentOrderID { get; set; }
        public Nullable<double> ItemWeight { get; set; }
        public string NoteProduct { get; set; }

        public virtual Order Order { get; set; }
    }
}
