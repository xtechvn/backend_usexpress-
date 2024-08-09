using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.API.Model.Affiliate
{
    public class AccesstradeOrderPostback
    {
        public string conversion_id { get; set; }
        public string conversion_result_id { get; set; }
        public string tracking_id { get; set; }
        public string transaction_id { get; set; }
        public DateTime transaction_time { get; set; }
        public float transaction_value { get; set; }
        public float? transaction_discount { get; set; }
        public int? status { get; set; }
        public Dictionary<string,string>? extra { get; set; }
        public List<AccesstradeOrderItem> items { get; set; }

    }
    public class AccesstradeOrderItem
    {
        public string id { get; set; }

        public string sku { get; set; }

        public string? name { get; set; }

        public float price { get; set; }

        public int? quantity { get; set; }

        public string category { get; set; }

        public string? category_id { get; set; }
        public int status { get; set; }
        public Dictionary<string, string>? extra { get; set; }
    }
    public class AdpiaOrderPostback
    {
        public string order_code { get; set; }
        public DateTime? order_time { get; set; }
        public int order_value { get; set; }
        public string? order_status { get; set; }
        public string? reject_reason { get; set; }
        public string track_id { get; set; }
        public List<AdpiaOrderItem> items { get; set; }
        

    }
    public class AdpiaOrderItem
    {
        /// <summary>
        /// SKU
        /// </summary>
        public string pcd { get; set; }
        /// <summary>
        /// Name
        /// </summary>
        public string pnm { get; set; }
        /// <summary>
        /// Group_product_name
        /// </summary>
        public string ccd { get; set; }

        public float price { get; set; }
        /// <summary>
        /// Number of product
        /// </summary>
        public int cnt { get; set; }
        public string? status { get; set; }
        public string? reject_reason { get; set; }
    }
}
