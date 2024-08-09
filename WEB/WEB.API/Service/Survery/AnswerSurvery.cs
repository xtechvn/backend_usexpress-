using Entities.ViewModels.ServicePublic;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.API.Service.Survery
{
    public class AnswerSurvery
    {
        private readonly IConfiguration configuration;
        //Code khai báo một biến cấp classs của IMongoCollection<Carts>.Interface IMongoCollection biểu diễn một MongoDB collection.
        private IMongoCollection<AnswerSurveryViewModel> AnswerSurveryCollection;
        public AnswerSurvery(IConfiguration _Configuration)
        {
            configuration = _Configuration;

            string url = "mongodb://" + _Configuration["MongoDB:user"] + ":" + _Configuration["MongoDB:pwd"] + "@" + _Configuration["MongoDB:Host"] + ":" + _Configuration["MongoDB:Port"] + "/" + _Configuration["MongoDB:catalog_core"];
            var client = new MongoClient(url);
            IMongoDatabase db = client.GetDatabase(_Configuration["MongoDB:catalog_core"]);
            this.AnswerSurveryCollection = db.GetCollection<AnswerSurveryViewModel>("AnswerSurvery");

        }
        public async Task<AnswerSurveryViewModel> addNew(AnswerSurveryViewModel answerSurvery)
        {
            try
            {
                await AnswerSurveryCollection.InsertOneAsync(answerSurvery);
                return answerSurvery;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("addNew - AnswerSurvery: " + ex);
                return null;
            }
        }
    }
}
