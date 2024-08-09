using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Product
{
    public class ProductNotFoundViewModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        [BsonElement]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }
        [BsonElement]
        public string ProductCode { get; set; }
        [BsonElement]
        public int LabelId { get; set; }
        [BsonElement]
        public string ExceptionMsg { get; set; }
        [BsonElement]
        public string Ip { get; set; }
        [BsonElement]
        public int Status { get; set; }
    }
}
