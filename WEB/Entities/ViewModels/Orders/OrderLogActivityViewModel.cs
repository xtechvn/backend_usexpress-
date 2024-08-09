using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace Entities.ViewModels.Orders
{
    public class OrderLogActivityViewModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }

        [BsonElement]
        public string order_no { get; set; } // mã đơn hàng

        [BsonElement]
        public long create_date { get; set; }// ngày tạo log

        [BsonIgnore]
        public DateTime date_create { get; set; }

        [BsonElement]
        public string email_client { get; set; }//email của client log

        [BsonElement]
        public string email_user { get; set; }//email của user tạo log

        [BsonElement]
        public int status { get; set; }//trạng thái đơn hàng

        [BsonElement]
        public int payment_type { get; set; }//loại thanh toán

        [BsonElement]
        public double amount { get; set; }//giá trị thanh toán

        [BsonIgnore]
        public string notify_name { get; set; }
        [BsonIgnore]
        public double day { get; set; }
        [BsonIgnore]
        public double hour { get; set; }
        [BsonIgnore]
        public double minute { get; set; }
        [BsonIgnore]
        public string time { get; set; }
        [BsonIgnore]
        public long orderId { get; set; }
    }
    public class LogOrderActivityModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }

        [BsonElement]
        public string order_no { get; set; } // mã đơn hàng

        [BsonElement]
        public long create_date { get; set; }//ngày tạo log

        [BsonElement]
        public string email_client { get; set; }//email client log
        [BsonElement]
        public string email_user { get; set; }//email user tạo log
        [BsonElement]
        public int status { get; set; }//trạng thái đơn hàng
        [BsonElement]
        public int payment_type { get; set; }//loại thanh toán
        [BsonElement]
        public double amount { get; set; }//giá trị thanh toán
    }
}
