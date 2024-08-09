using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Product
{
    public class ProductMongoViewModel
    {
   
        [BsonElement("_id")]
        public string _id { get; set; }
        public void GenID()
        {
            _id = ObjectId.GenerateNewId().ToString();
        }
        public ProductViewModel product_detail { get; set; }
    }
}
