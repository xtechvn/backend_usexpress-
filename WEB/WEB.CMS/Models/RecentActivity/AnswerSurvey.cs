using Entities.ViewModels.Orders;
using Entities.ViewModels.ServicePublic;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.CMS.Models.RecentActivity
{
    public class AnswerSurvey
    {
        private readonly IConfiguration configuration;
        //Code khai báo một biến cấp classs của IMongoCollection<Carts>.Interface IMongoCollection biểu diễn một MongoDB collection.
        private IMongoCollection<AnswerSurveryViewModel> AnswerSurveryCollection;
        public AnswerSurvey(IConfiguration _Configuration)
        {
            configuration = _Configuration;
            string url = "mongodb://" + configuration["DataBaseConfig:MongoServer:Host"] + "";
            var client = new MongoClient("mongodb://" + configuration["DataBaseConfig:MongoServer:Host"] + "");
            IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog"]);
            AnswerSurveryCollection = db.GetCollection<AnswerSurveryViewModel>("AnswerSurvery");
        }

        public List<AnswerSurveryViewModel> GetAnswerSurveryPagnition (int pageIndex = 1, int pageSize = 6)
        {
            try
            {
                var listData = AnswerSurveryCollection.AsQueryable()
                    .Select(p => p)
                    .OrderByDescending(p=>p.CreateOn)
                    .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                    .ToList();
                return listData;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("CMS. GetAnswerSurveryPagnition - RecentActivityLog: " + ex);
                return new List<AnswerSurveryViewModel>();
            }
        }
        public List<AnswerSurveryViewModel> GetAnswerSurveryToday()
        {
            try
            {
                var filter = Builders<AnswerSurveryViewModel>.Filter.Empty;
                var rs = AnswerSurveryCollection.Find(filter).ToList();
                var listData = AnswerSurveryCollection.Find(filter).SortByDescending(n => n.id).ToList();
                return listData;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("CMS. GetAnswerSurveryToday - RecentActivityLog: " + ex);
                return new List<AnswerSurveryViewModel>();
            }
        }
    }
}
