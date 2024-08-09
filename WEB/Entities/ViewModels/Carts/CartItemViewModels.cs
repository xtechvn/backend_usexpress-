using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Entities.ViewModels.Carts
{
    public class CartItemViewModels
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }

        [BsonElement]
        public string cart_id { get; set; } // định danh user

        public int label_id { get; set; } // nhãn sp

        [BsonElement]
        public string seller_id { get; set; } // sale choice
        [BsonElement]
        public string product_code { get; set; } //id san pham tren moi store

        [BsonElement]
        public double rate_current { get; set; } // tỷ gia chạy theo vcb
        public double price_discount_first_pound { get; set; } //$ số tiền  giảm khi mua sản phẩm với số lượng từ 2 trở lên hoặc khác. Tùy theo chính sách từng nhãn

        [BsonElement]
        public int quantity { get; set; } // số lượng mua sp
        

        [BsonElement]
        public double amount_last { get; set; } //$ đơn giá sản phẩm sau khi được giảm các loại chi phí
        [BsonElement]
        public double amount_last_vnd { get; set; } //vnd đơn giá sản phẩm sau khi được giảm các loại chi phí da dc nhan voi ty gia

        [BsonElement]
        public string voucher_name { get; set; } // ten voucher giảm giá

        [BsonElement]
        public ProductViewModel product_detail { get; set; } // Thông tin sản phẩm mua

        [BsonElement]
        public string create_date { get; set; }
        [BsonElement]
        public string update_last { get; set; }
        //[BsonElement]
        public int cart_status { get; set; } // 0:binh thuong, 1: xoa

        public bool is_selected { get; set; } // trạng thái được chọn ngoài giỏ hàng

    }
}
