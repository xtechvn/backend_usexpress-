using Entities.ViewModels;
using Entities.ViewModels.Affiliate;
using Entities.ViewModels.Product;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Contants;

namespace AppPushAffiliate.Models
{
    public class AffiliateOrderMongoAccess
    {
        private IMongoCollection<AffiliateOrderItem> _affiliateOrderCollection;

        public AffiliateOrderMongoAccess(string host,string catalog)
        {
            string url = "mongodb://" + host + "";
            var client = new MongoClient("mongodb://" + host + "");
            IMongoDatabase db = client.GetDatabase(catalog);
            _affiliateOrderCollection = db.GetCollection<AffiliateOrderItem>("AffiliateOrders");
        }
       public async Task<AffiliateOrderItem> GetSuccessAffOrderByID(long order_id)
       {
            try
            {
                var filter = Builders<AffiliateOrderItem>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<AffiliateOrderItem>.Filter.Eq(x => x.order_id, order_id);
                filterDefinition &= Builders<AffiliateOrderItem>.Filter.Eq(x => x.status, (int)ResponseType.SUCCESS);

                var model = await _affiliateOrderCollection.Find(filterDefinition).FirstOrDefaultAsync();
                if(model!=null && model.status== (int)ResponseType.SUCCESS)
                return model;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("AffiliateOrderMongoAccess - IsOrderPushed Error: " + ex);
            }
            return null;

       }
        public async Task<string> AddNewAsync(AffiliateOrderItem model)
        {
            try
            {
                await _affiliateOrderCollection.InsertOneAsync(model);
                return model._id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("AffiliateOrderMongoAccess - AddNewAsync: \nData: aff_model: " + JsonConvert.SerializeObject(model) + ".\n Error: " + ex);
                return null;
            }
        }
        public async Task<string> UpdateAsync(AffiliateOrderItem model)
        {
            try
            {
                var filter = Builders<AffiliateOrderItem>.Filter;
                var filterDefinition = filter.And(
                    filter.Eq("_id", model._id));
                await _affiliateOrderCollection.FindOneAndReplaceAsync(filterDefinition, model);
                return model._id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("AffiliateOrderMongoAccess - UpdateAsync: \nData: aff_model: " + JsonConvert.SerializeObject(model) + ".\n Error: " + ex);
                return null;
            }
        }
    }
}
