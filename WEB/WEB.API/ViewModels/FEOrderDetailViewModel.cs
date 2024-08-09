using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.API.ViewModels
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class FEOrderDetailViewModel
    {
        public long id { get; set; }
        public long ClientId { get; set; }
        public string orderNo { get; set; }
        public dynamic? createdOn { get; set; }
        public int orderStatus { get; set; }
        public int PaymentStatus { get; set; }
        public string orderStatusName { get; set; }
        public dynamic? priceVnd { get; set; }
        public dynamic? amountVnd { get; set; }
        public double? totalDiscount2ndVnd { get; set; }
        public double? totalDiscountVoucherVnd { get; set; }
        public string? TotalDiscount { get; set; }
        public int paymentType { get; set; }
        public string paymentTypeName { get; set; }
        public string clientName { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string storeName { get; set; }
        public double rateCurrent { get; set; }
        public string note { get; set; }
        public int addressId { get; set; }
        public List<FEOrderProductList> productList { get; set; }
    }
    public class FEOrderProductList
    {
        public string imageThumb { get; set; }
        public string productCode { get; set; }
        public string title { get; set; }
        public dynamic price { get; set; }
        public string? amoutVnd { get; set; }
        public int quantity { get; set; }
        public double? firstPoundFee { get; set; }
        public double? nextPoundFee { get; set; }
        public double? luxuryFee { get; set; }
        public double? weight { get; set; }
        public string Path { get; set; }
        public string SellerName { get; set; }
        public double Cost { get; set; }
        public string LinkSource { get; set; }
    }


}
