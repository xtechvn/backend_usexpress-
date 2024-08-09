using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Entities.ViewModels.Carts
{
    public class CartsViewModels
    {
        //public int id { get; set; }// khoa chinh
        public int label_id { get; set; }
        public string label_name { get; set; }
        public List<CartItemViewModels> cart_item { get; set; } // chi tiet gio hang
        public string voucher_name { get; set; }// voucher giam gia

        public double total_amount_cart { get; set; } // Tiền hàng
        public double total_discount_amount { get; set; } //Giam gia theo rule của usexpress
        //public double total_discound_voucher { get; set; } // Số tiền được giảm từ Voucher
        public double total_amount_last { get; set; } // Tổng tiền khách hàng phải trả. Sau khi trừ hết chi phí

        public int total_product { get; set; } // tong so san pham
        public int total_selected_carts { get; set; } // toosng so sản phẩm được tick trong giỏ hàng
        public double total_shipping_fee_us { get; set; } // tong phi mua ho
    }
}
