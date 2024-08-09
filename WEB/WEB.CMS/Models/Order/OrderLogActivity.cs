using Entities.ViewModels.Orders;
using Entities.ViewModels.Payment;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace WEB.CMS.Models.Order
{
    public class OrderLogActivity
    {
        private readonly IConfiguration configuration;
        //Code khai báo một biến cấp classs của IMongoCollection<Carts>.Interface IMongoCollection biểu diễn một MongoDB collection.
        private IMongoCollection<OrderLogActivityViewModel> OrderLogActivityCollection;
        public OrderLogActivity(IConfiguration _Configuration)
        {
            configuration = _Configuration;

            var client = new MongoClient("mongodb://" + configuration["DataBaseConfig:MongoServer:Host"] + "");
            IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog"]);
            this.OrderLogActivityCollection = db.GetCollection<OrderLogActivityViewModel>("OrderLogActivity");
        }

        public async Task<OrderLogActivityViewModel> addNew(OrderLogActivityViewModel orderLogActivityViewModel)
        {
            try
            {
                await OrderLogActivityCollection.InsertOneAsync(orderLogActivityViewModel);
                return orderLogActivityViewModel;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("CMS. addNew - OrderLogActivity: " + ex);
                return null;
            }
        }

        public List<OrderLogActivityViewModel> getOrderLogActivity(int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                double percent = 0;
                var filter = Builders<OrderLogActivityViewModel>.Filter.Empty;
                var listData = OrderLogActivityCollection.Find(filter).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToList();
                return listData;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("CMS. getOrderLogActivityViewModel - OrderLogActivity: " + ex);
                return new List<OrderLogActivityViewModel>();
            }
        }

        public List<OrderLogActivityViewModel> getOrderLogActivityToday()
        {
            try
            {
                var filter = Builders<OrderLogActivityViewModel>.Filter.Empty;
                var rs = OrderLogActivityCollection.Find(filter).ToList();
                var listData = OrderLogActivityCollection.Find(filter).SortByDescending(n => n.id).ToList();
                return listData;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("CMS. getOrderLogActivityViewModel - OrderLogActivity: " + ex);
                return new List<OrderLogActivityViewModel>();
            }
        }
    }
}
