using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.ServicePublic
{
    public class AnswerSurveryViewModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        [BsonElement]
        public string FuntionId { get; set; }
        [BsonElement]
        public string Answer { get; set; }
        [BsonElement]
        public string Email { get; set; }
        [BsonElement]
        public DateTime CreateOn { get; set; }
    }
}
