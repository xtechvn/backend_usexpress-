using Entities.ViewModels;
using Entities.ViewModels.Product;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.API.Models.Product
{
    public class ProductDetailMongoAccess
    {
        private readonly IConfiguration _configuration;
        private IMongoCollection<ProductMongoViewModel> _productDetailCollection;

        public ProductDetailMongoAccess(IConfiguration configuration)
        {
            _configuration = configuration;

            string url = "mongodb://" + _configuration["MongoDB:user"] + ":" + _configuration["MongoDB:pwd"] + "@" + _configuration["MongoDB:Host"] + ":" + _configuration["MongoDB:Port"] + "/" + _configuration["MongoDB:catalog_core"];
            var client = new MongoClient(url);
            IMongoDatabase db = client.GetDatabase(_configuration["MongoDB:catalog_core"]);
            _productDetailCollection = db.GetCollection<ProductMongoViewModel>("ProductDetail");
        }
        public async Task<string> AddNewAsync(ProductMongoViewModel model)
        {
            try
            {
                await _productDetailCollection.InsertOneAsync(model);
                return model._id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - AddNewAsync: \nData: aff_model: " + JsonConvert.SerializeObject(model) + ".\n Error: " + ex);
                return null;
            }
        }
        public async Task<string> UpdateAsync(ProductMongoViewModel model)
        {
            try
            {
                var filter = Builders<ProductMongoViewModel>.Filter;
                var filterDefinition = filter.And(
                    filter.Eq("_id", model._id));
                await _productDetailCollection.FindOneAndReplaceAsync(filterDefinition, model);
                return model._id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - UpdateAsync: \nData: aff_model: " + JsonConvert.SerializeObject(model) + ".\n Error: " + ex);
                return null;
            }
        }
        public async Task<string> DeleteAsync(ProductBlackList model)
        {
            try
            {
                var filter = Builders<ProductMongoViewModel>.Filter;
                var filterDefinition = filter.And(
                    filter.Eq("_id", model._id)
                    );
                await _productDetailCollection.FindOneAndDeleteAsync(filterDefinition);
                return model._id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - DeleteAsync: \nData: aff_model: " + JsonConvert.SerializeObject(model) + ".\n Error: " + ex);
                return null;
            }
        }
        public async Task<string> FindIDByProductCode(string product_code)
        {
            try
            {
                var filter = Builders<ProductMongoViewModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoViewModel>.Filter.Eq(x => x.product_detail.product_code, product_code); ;
                var model = await _productDetailCollection.Find(filterDefinition).FirstOrDefaultAsync();
                return model._id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - FindIDByProductCode Error: " + ex);
                return null;
            }
        }
        public async Task<ProductMongoViewModel> FindByID(string id)
        {
            try
            {
                var filter = Builders<ProductMongoViewModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoViewModel>.Filter.Eq(x => x._id, id); ;
                var model = await _productDetailCollection.Find(filterDefinition).FirstOrDefaultAsync();
                return model;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - FindByID Error: " + ex);
                return null;
            }
        }
        public async Task<ProductMongoViewModel> FindDetailByProductCode(string product_code)
        {
            try
            {
                var filter = Builders<ProductMongoViewModel>.Filter;
                var filterDefinition = filter.Empty;
                filterDefinition &= Builders<ProductMongoViewModel>.Filter.Eq(x => x.product_detail.product_code, product_code); ;
                var model = await _productDetailCollection.Find(filterDefinition).FirstOrDefaultAsync();
                return model;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("ProductDetailMongoAccess - FindDetailByProductCode Error: " + ex);
                return null;
            }
        }
    }
}
