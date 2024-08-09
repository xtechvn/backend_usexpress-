using Entities.ViewModels.Carts;
using Entities.ViewModels.Product;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.API.Model.Product
{
    public class ProductNotFound
    {
        private readonly IConfiguration configuration;
        //Code khai báo một biến cấp classs của IMongoCollection<Carts>.Interface IMongoCollection biểu diễn một MongoDB collection.
        private IMongoCollection<ProductNotFoundViewModel> ProductNotFoundCollection;
        public ProductNotFound(IConfiguration _Configuration)
        {
            configuration = _Configuration;

            var client = new MongoClient("mongodb://" + configuration["DataBaseConfig:MongoServer:Host"] + "");
            IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog"]);
            this.ProductNotFoundCollection = db.GetCollection<ProductNotFoundViewModel>("ProductNotFound");

        }
        public async Task<ProductNotFoundViewModel> addNew(ProductNotFoundViewModel productFavorite)
        {
            try
            {
                await ProductNotFoundCollection.InsertOneAsync(productFavorite);
                return productFavorite;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("addNew - ProductNotFound: " + ex);
                return null;
            }
        }
        /// <summary>
        /// lấy % các sp k tìm thấy của ngày hiện tại so với ngày hôm qua
        /// </summary>
        /// <param name="productFavorite"></param>
        /// <returns></returns>
        public double getPercent(ProductNotFoundViewModel productFavorite)
        {
            try
            {
                double percent = 0;
                var filterToday = Builders<ProductNotFoundViewModel>.Filter.
                  Where(x => x.CreateTime.Date == DateTime.Now.Date);
                var resultToday = ProductNotFoundCollection.Find(filterToday).ToList();

                var filterYesterday = Builders<ProductNotFoundViewModel>.Filter.
                 Where(x => x.CreateTime.Date == DateTime.Now.Date.AddDays(-1));
                var resultYesterday = ProductNotFoundCollection.Find(filterToday).ToList();

                percent = (resultToday.Count / resultYesterday.Count) * 100;

                return (double)Math.Round(percent, 2);
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("addNew - ProductNotFound: " + ex);
                return 0;
            }
        }
    }
}
