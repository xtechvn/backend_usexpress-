using Caching.RedisWorker;
using Entities.ViewModels.Product;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Contants;

namespace WEB.CMS.Models.Product
{
    public class ProductMongoAccess
    {
        private readonly IConfiguration _configuration;
        private IMongoCollection<ProductBlackList> _productKeywordBlackListCollection;

        public ProductMongoAccess(IConfiguration configuration)
        {
            _configuration = configuration;
            string url = "mongodb://" + configuration["DataBaseConfig:MongoServer:Host"] + "";
            var client = new MongoClient("mongodb://" + configuration["DataBaseConfig:MongoServer:Host"] + "");
            IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog"]);
            _productKeywordBlackListCollection = db.GetCollection<ProductBlackList>("ProductBlackList");
        }
        public async Task<string> AddNewAsync(ProductBlackList model)
        {
            try
            {
                await _productKeywordBlackListCollection.InsertOneAsync(model);
                return model._id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("CMS. AddNewAsync - ProductMongoAccess: \nData: aff_model: " + JsonConvert.SerializeObject(model) + ".\n Error: " + ex);
                return null;
            }
        }
        public async Task<string> UpdateAsync(ProductBlackList model)
        {
            try
            {
                var filter = Builders<ProductBlackList>.Filter;
                var filterDefinition = filter.And(
                    filter.Eq("_id", model._id));
                await _productKeywordBlackListCollection.FindOneAndReplaceAsync(filterDefinition, model);
                return model._id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("CMS. AddNewAsync - ProductMongoAccess: \nData: aff_model: " + JsonConvert.SerializeObject(model) + ".\n Error: " + ex);
                return null;
            }
        }
        public async Task<string> DeleteAsync(ProductBlackList model)
        {
            try
            {
                var filter = Builders<ProductBlackList>.Filter;
                var filterDefinition = filter.And(
                    filter.Eq("_id", model._id)
                    );
                await _productKeywordBlackListCollection.FindOneAndDeleteAsync(filterDefinition);
                return model._id;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("CMS. DeleteAsync - ProductMongoAccess: \nData: aff_model: " + JsonConvert.SerializeObject(model) + ".\n Error: " + ex);
                return null;
            }
        }
        public async Task<List<ProductBlackList>> GetByKeywords(string keywords, int label_id = -1, int keyword_type = -1, int product_status = -2)
        {
            try
            {
                ProductBlackList model = new ProductBlackList();
                var filter = Builders<ProductBlackList>.Filter;
                var filterDefinition = filter.Empty;
                if (keywords != null && keywords.Trim() != "") filterDefinition &= Builders<ProductBlackList>.Filter.Eq(x => x.keywords, keywords); ;
                if (label_id > -1) filterDefinition &= Builders<ProductBlackList>.Filter.Eq(x => x.label_id, label_id) ; ;
                if (keyword_type > -1) filterDefinition &= Builders<ProductBlackList>.Filter.Eq(x => x.keyword_type, keyword_type);
                if (product_status >= -1) filterDefinition &= Builders<ProductBlackList>.Filter.Eq(x => x.product_status, product_status);

                return await _productKeywordBlackListCollection.Find(filterDefinition).ToListAsync();

            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("CMS. GetPagnition - GetByKeywords Error: " + ex);
                return null;
            }
        }
        public async Task CacheData(RedisConn _redis, int db_index)
        {
            try
            {
                var data = await _productKeywordBlackListCollection.Find(_ => true).ToListAsync();
                List<BlackListCacheData> list = new List<BlackListCacheData>();
                var cache_name = CacheType.KEYWORD_BLACK_LIST;
                if(data == null|| data.Count < 1)
                {
                    _redis.Set(cache_name, "", db_index);
                }
                else
                {
                    foreach (var i in data)
                    {
                        list.Add(new BlackListCacheData()
                        {
                            label_id = i.label_id,
                            keyword = i.keywords,
                            keyword_type = i.keyword_type
                        });
                    }
                    _redis.Set(cache_name, JsonConvert.SerializeObject(list), db_index);
                }
            }
            catch (Exception)
            {
            }
        }
        public async Task<ProductBlackList> Find(string keywords, int keyword_type, int label_id)
        {
            try
            {
                var filter = Builders<ProductBlackList>.Filter;
                var filterDefinition = filter.And(
                       filter.Eq("keywords", keywords),
                       filter.Eq("keyword_type", keyword_type),
                       filter.Eq("label_id", label_id)
                    );

                var model_list = await _productKeywordBlackListCollection.Find(filterDefinition).FirstOrDefaultAsync();
                return model_list;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("CMS. GetPagnition - ProductMongoAccess: \nData: keywords: " + keywords + " ,keyword_type:" + keyword_type + " ,label_id:" + label_id + ".\n Error: " + ex);
                return null;
            }
        }
        public async Task<int> GetTotalPage(int keyword_type, int page_size)
        {
            try
            {
                var filter = Builders<ProductBlackList>.Filter;
                var filterDefinition = filter.And(
                   filter.Eq("keyword_type", keyword_type)
                   );
                var total_record = await _productKeywordBlackListCollection.Find(filterDefinition).CountDocumentsAsync();
                int total_page = (int)(total_record / page_size);
                return total_page;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("CMS. GetTotalPage - ProductMongoAccess: \n" + ex);
                return 0;
            }
        }
        public async Task<long> GetTotalRecord(int keyword_type)
        {
            try
            {
                var filter = Builders<ProductBlackList>.Filter;
                var filterDefinition = filter.And(
                    filter.Eq("keyword_type", keyword_type)
                    );
                var total_record = await _productKeywordBlackListCollection.Find(filterDefinition).CountDocumentsAsync();
                return total_record;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("CMS. GetTotalPage - ProductMongoAccess: \n" + ex);
                return 0;
            }
        }
    }
    class BlackListCacheData
    {
       public string keyword { get; set; }
        public int label_id { get; set; }
        public int keyword_type { get; set; }
    }
}
