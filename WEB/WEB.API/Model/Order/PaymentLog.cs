using Entities.ViewModels.Payment;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.API.Model.Order
{
    public class PaymentLog
    {
        private readonly IConfiguration configuration;
        //Code khai báo một biến cấp classs của IMongoCollection<Carts>.Interface IMongoCollection biểu diễn một MongoDB collection.
        private IMongoCollection<PaymentLogViewModel> ProductFavoriteCollection;
        public PaymentLog(IConfiguration _Configuration)
        {
            configuration = _Configuration;

            var client = new MongoClient("mongodb://" + configuration["DataBaseConfig:MongoServer:Host"] + "");
            IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog"]);
            this.ProductFavoriteCollection = db.GetCollection<PaymentLogViewModel>("ProductFavorite");
        }
        public async Task<PaymentLogViewModel> addNew(PaymentLogViewModel paymentLog)
        {
            try
            {
                await ProductFavoriteCollection.InsertOneAsync(paymentLog);
                return paymentLog;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("addNew - PaymentLog - API: " + ex);
                return null;
            }
        }
    }
}
