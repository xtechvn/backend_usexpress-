using Entities.ViewModels.ServicePublic;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.API.Service.Survery
{
    public class FunctionSurvery
    {
        private readonly IConfiguration configuration;
        //Code khai báo một biến cấp classs của IMongoCollection<Carts>.Interface IMongoCollection biểu diễn một MongoDB collection.
        private IMongoCollection<FunctionSurveryViewModel> FunctionSurveryCollection;
        public FunctionSurvery(IConfiguration _Configuration)
        {
            configuration = _Configuration;
            string url = "mongodb://" + _Configuration["MongoDB:user"] + ":" + _Configuration["MongoDB:pwd"] + "@" + _Configuration["MongoDB:Host"] + ":" + _Configuration["MongoDB:Port"] + "/" + _Configuration["MongoDB:catalog_core"];
            var client = new MongoClient(url);
            IMongoDatabase db = client.GetDatabase(_Configuration["MongoDB:catalog_core"]);
            this.FunctionSurveryCollection = db.GetCollection<FunctionSurveryViewModel>("FunctionSurvery");
        }
        public async Task<FunctionSurveryViewModel> addNew(FunctionSurveryViewModel functionSurveryViewModel)
        {
            try
            {
                await FunctionSurveryCollection.InsertOneAsync(functionSurveryViewModel);
                return functionSurveryViewModel;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("addNew - FunctionSurvery: " + ex);
                return null;
            }
        }

        public async Task<List<FunctionSurveryViewModel>> GetAllFunctionSurvery()
        {
            try
            {
                var builderFunctionSurvery = Builders<FunctionSurveryViewModel>.Filter.Empty;
                var listFunctionSurvery = FunctionSurveryCollection.Find(builderFunctionSurvery).ToList();
                return listFunctionSurvery;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("GetAllFunctionSurvery - FunctionSurvery: " + ex);
                return null;
            }
        }
    }
}
