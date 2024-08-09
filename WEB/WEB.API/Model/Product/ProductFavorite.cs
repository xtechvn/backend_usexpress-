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
    public class ProductFavorite
    {
        private readonly IConfiguration configuration;
        //Code khai báo một biến cấp classs của IMongoCollection<Carts>.Interface IMongoCollection biểu diễn một MongoDB collection.
        private IMongoCollection<ProductFavoriteViewModel> ProductFavoriteCollection;
        public ProductFavorite(IConfiguration _Configuration)
        {
            configuration = _Configuration;

            var client = new MongoClient("mongodb://" + configuration["DataBaseConfig:MongoServer:Host"] + "");
            IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog"]);
            this.ProductFavoriteCollection = db.GetCollection<ProductFavoriteViewModel>("ProductFavorite");

        }
        public async Task<ProductFavoriteViewModel> addNew(ProductFavoriteViewModel productFavorite)
        {
            try
            {
                // Kiểm tra sp có trong db đã có tồn tại sp tương ứng với client này chưa
                //FilterDefinition chỉ định một điều kiện tìm kiếm được sử dụng trong khi cập nhật một document
                var filter = Builders<ProductFavoriteViewModel>.Filter.
                    Where(x => x.ProductCode == productFavorite.ProductCode
                    && x.ClientId == productFavorite.ClientId && productFavorite.LabelId == x.LabelId);
                var orgProductFavorite = ProductFavoriteCollection.Find(filter).FirstOrDefault();
                if (orgProductFavorite != null)
                {
                    orgProductFavorite.ClientId = productFavorite.ClientId;
                    orgProductFavorite.ProductCode = productFavorite.ProductCode;
                    orgProductFavorite.LabelId = productFavorite.LabelId;
                    orgProductFavorite.IsFavorite = productFavorite.IsFavorite;
                    await ProductFavoriteCollection.ReplaceOneAsync(filter, orgProductFavorite);
                }
                else
                {
                    await ProductFavoriteCollection.InsertOneAsync(productFavorite);
                }
                return productFavorite;
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("addNew - ProductFavorite: " + ex);
                return null;
            }
        }
    }
}
