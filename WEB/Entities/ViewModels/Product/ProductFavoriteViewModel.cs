using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Product
{
    public class ProductFavoriteViewModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        [BsonElement]
        public long ClientId { get; set; }
        [BsonElement]
        public string ProductCode { get; set; }
        [BsonElement]
        public int LabelId { get; set; }
        [BsonElement]
        public bool IsFavorite { get; set; }
    }
}
