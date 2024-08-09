using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.ServicePublic
{
    public class FunctionSurveryViewModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        [BsonElement]
        public string FunctionName { get; set; }
    }
}
