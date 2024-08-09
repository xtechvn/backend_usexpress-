using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Product
{
    public class ProductBlackList
    {
        [BsonElement("_id")]
        public string _id { get; set; }
        [BsonElement("keywords")]
        public string keywords { get; set; } // Là chuỗi chặn của sản phẩm
        [BsonElement("keyword_type")]
        public int keyword_type { get; set; } // 0: hiểu là chặn cho mã sản phẩm | 1: hiểu là chặn theo từ khóa tên của sản phẩm
        [BsonElement("label_id")]
        public int label_id { get; set; } // nhãn hàng
        [BsonElement("product_status")]
        public int product_status { get; set; } //Khóa | Chặn | Mở khóa | Bỏ chặn là status khóa chặn của sản phẩm. Trường này sẽ điều hướng việc hiển thị sản phẩm ngoài FR
        [BsonElement("create_date")]
        public DateTime create_date { get; set; }
        public void GenID()
        {
            _id = ObjectId.GenerateNewId().ToString();
        }
    }
}
