using Entities.ViewModels.Log;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Contants;

namespace WEB.API.Service.Log
{ 
    public static class UsersLoggingService
    {
        public static async Task<string> InsertLog(IConfiguration _Configuration, LogUsersActivityModel log, string document_name)
        {
            try
            {
                string url = "mongodb://" + _Configuration["MongoDB:user"] + ":" + _Configuration["MongoDB:pwd"] + "@" + _Configuration["MongoDB:Host"] + ":" + _Configuration["MongoDB:Port"] + "/" + _Configuration["MongoDB:catalog_core"];
                var client = new MongoClient(url);
                IMongoDatabase db = client.GetDatabase(_Configuration["MongoDB:catalog_core"]);
                IMongoCollection<LogUsersActivityModel> affCollection = db.GetCollection<LogUsersActivityModel>(document_name);
                var filter = Builders<LogUsersActivityModel>.Filter.Where(x => x.id == log.id);
                var result_document = affCollection.Find(filter).ToList();
                if (result_document != null && result_document.Count > 0)
                {
                    await affCollection.ReplaceOneAsync(filter, log);
                }
                else
                {
                    await affCollection.InsertOneAsync(log);
                }
                return "";
            } catch(Exception ex)
            {
                return ex.ToString();
            }
        }
        public static async Task<string> InsertLogFromAPI(IConfiguration configuration,string j_data_log)
        {
            try
            {
                var client = new MongoClient("mongodb://" + configuration["DataBaseConfig:MongoServer:Host"] + "");
                IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog"]);
                LogUsersActivityModel log = new LogUsersActivityModel()
                {
                    user_type = 0,
                    user_id = -1,
                    user_name = "Kerry",
                    log_type = (int)LogActivityType.CHANGE_ORDER_BY_KERRRY,
                    log_date = DateTime.Now,
                    j_data_log = j_data_log,
                };
                IMongoCollection<LogUsersActivityModel> affCollection = db.GetCollection<LogUsersActivityModel>(LogActivityBSONDocuments.API);
                var filter = Builders<LogUsersActivityModel>.Filter.Where(x => x.id == log.id);
                var result_document = affCollection.Find(filter).ToList();
                if (result_document != null && result_document.Count > 0)
                {
                    await affCollection.ReplaceOneAsync(filter, log);
                }
                else
                {
                    await affCollection.InsertOneAsync(log);
                }
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
