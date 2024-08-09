using Entities.ViewModels;
using Entities.ViewModels.Carts;
using Entities.ViewModels.Product;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Repositories.MongoRepositories.Product
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
        /// <summary>
        /// lấy % các sp k tìm thấy của ngày hiện tại so với ngày hôm qua
        /// </summary>
        /// <param name="productFavorite"></param>
        /// <returns></returns>
        public RevenueViewModel getPercent()
        {
            try
            {
                var filterProductNotFound = Builders<ProductNotFoundViewModel>.Filter.Empty;
                var listProductNotFound = ProductNotFoundCollection.Find(filterProductNotFound).ToList();
                var totalToday = listProductNotFound.Where(n => n.CreateTime.Date == DateTime.Now.Date).Count();
                var totalYester = listProductNotFound.Where(n => n.CreateTime.Date == DateTime.Now.Date.AddDays(-1)).Count();
                RevenueViewModel revenueViewModel = new RevenueViewModel
                {
                    CrawlPercent = totalYester > 0 ? Math.Round((((float)(float)totalToday / (float)totalYester) * (float)100), 2) : 0,
                    TotalProductNotFound = totalToday
                };
                return revenueViewModel;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("addNew - ProductNotFound: " + ex);
                return new RevenueViewModel();
            }
        }
    }
}
