using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    public class ProductBuyerViewModel
    {
        public int LabelId { get; set; }
        public double Price { get; set; } // đơn giá sp bao gồm shipping Fee
        public double Pound { get; set; } // cân nặng sp 
        public int Unit { get; set; } // đơn vị tính. Ví dụ: Pound,inch,...
        public int IndustrySpecialType { get; set; } // ngành hàng. Dựa vào đây để phân loại Luxry
        public double RateCurrent { get; set; }
        public double ShippingUSFee { get; set; }
    }

    public class ProductSyncModel
    {
        public int LABEL_ID { get; set; }
        public string PRODUCT_CODE { get; set; }
        public double ITEM_WEIGHT { get; set; }
        public string ORIGINAL_WEIGHT { get; set; }
        public double FIRST_POUND_FEE { get; set; }
        public double SHIPPING_US_FEE { get; set; }
        public double LUXURY_FEE { get; set; }
        public double NEXT_POUND_FEE { get; set; }
        public double PRODUCT_PRICE { get; set; }
        public double PRICE_LAST { get; set; }
        public double RATE_CURRENT { get; set; }
        public double TOTAL_FEE { get; set; }
        public double TOTAL_SHIPPING_FEE { get; set; }
    }
}
