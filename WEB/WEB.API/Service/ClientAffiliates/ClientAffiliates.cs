using Entities.ViewModels.Affiliate;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.API.Service.ClientAffiliates
{
    public class ClientAffiliates
    {
        private readonly IConfiguration configuration;
        //Code khai báo một biến cấp classs của IMongoCollection<Carts>.Interface IMongoCollection biểu diễn một MongoDB collection.
        private IMongoCollection<MyAffiliateLinkViewModel> _affURLCollection;
        public ClientAffiliates(IConfiguration _Configuration)
        {
            configuration = _Configuration;
            string url = "mongodb://" + configuration["DataBaseConfig:MongoServer:Host"] + "";
            var client = new MongoClient("mongodb://" + configuration["DataBaseConfig:MongoServer:Host"] + "");
            IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog"]);
            _affURLCollection = db.GetCollection<MyAffiliateLinkViewModel>("ClientAffiliateURL");
        }
        public List<MyAffiliateLinkViewModel> GetAffliateURLByClient(long client_id)
        {
            try
            {
                var listData = _affURLCollection.AsQueryable()
                  .Where(p => p.client_id == client_id)
                  .OrderByDescending(p => p.update_time)
                  .ToList();
                return listData;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("API. ClientAffiliates -  GetAffliateURLByClient: \nData: client_id: " + client_id + ".\n Error: " + ex);
                return null;
            }
        }
        public async Task<MyAffiliateLinkViewModel> GetAffliateURLByIDAsync(int aff_id)
        {
            try
            {
                var filter = Builders<MyAffiliateLinkViewModel>.Filter;
                var filterDefinition = filter.And(
                    filter.Eq("id", aff_id)
                );
                var listData = await _affURLCollection.Find(filterDefinition).Limit(1).SingleAsync();
                return listData;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("API. ClientAffiliates -  GetAffliateURLByID: \nData: _id: " + aff_id + ".\n Error: " + ex);
                return null;
            }
        }
        public async Task<MyAffiliateLinkViewModel> CheckAffiliateURLExists(MyAffiliateLinkViewModel item)
        {
            try
            {
                var filter = Builders<MyAffiliateLinkViewModel>.Filter;
                var filterDefinition = filter.And(
                    filter.Eq("link_aff", item.link_aff),
                    filter.Eq("client_id", item.client_id)
                );
                var result =  await  _affURLCollection.FindAsync(filterDefinition).Result.FirstOrDefaultAsync();

                return result != null ? result : null;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("API. CheckAffiliateURLExists -  GetAffliateURLByID: \nData: " + JsonConvert.SerializeObject(item) + ".\n Error: " + ex);
                return new MyAffiliateLinkViewModel() { 
                    _id="Error on Excution"
                };
            }
        }
        public async Task<MyAffiliateLinkViewModel> PushAffliateURLAsync(MyAffiliateLinkViewModel aff_model)
        {
            try
            {
                await _affURLCollection.InsertOneAsync(aff_model);
                return aff_model;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("API. ClientAffiliates - GetAffliateURLAsync: \nData: aff_model: " + JsonConvert.SerializeObject(aff_model) + ".\n Error: " + ex);
                return null;
            }
        }
        public async Task<MyAffiliateLinkViewModel> UpdateAffliateURLAsync(MyAffiliateLinkViewModel aff_model)
        {
            try
            {
                var filter = Builders<MyAffiliateLinkViewModel>.Filter;
                var filterDefinition = filter.And(
                    filter.Eq("_id", aff_model._id),
                    filter.Eq("link_aff", aff_model.link_aff),
                    filter.Eq("client_id", aff_model.client_id)
                );
                await _affURLCollection.FindOneAndReplaceAsync(filterDefinition, aff_model);
                return aff_model;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("API. ClientAffiliates - UpdateAffliateURLAsync: \nData: aff_model: " + JsonConvert.SerializeObject(aff_model) + ".\n Error: " + ex);
                return null;
            }
        }
    }

}
