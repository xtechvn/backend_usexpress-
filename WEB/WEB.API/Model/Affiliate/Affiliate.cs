using Entities.ViewModels.Affiliate;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Contants;

namespace WEB.API.Model.Affiliate
{
    public class Affiliate
    {
        private readonly IConfiguration configuration;
        private IMongoCollection<AffiliateViewModel> affCollection;
        public Affiliate(IConfiguration _Configuration)
        {
            configuration = _Configuration;

            var client = new MongoClient("mongodb://" + configuration["DataBaseConfig:MongoServer:Host"] + "");
            //Gọi phương thức GetDatabase() trên MongoClient và chỉ định tên cơ sở dữ liệu để kết nối(FirstDatabase trong trường hợp này)
            //GetDatabase trả về đối tượng .IMongoDatabase.Tiếp theo CartsViewModels collection được truy suất sử dụng phương thức GetCollection của IMongoDatabase.
            IMongoDatabase db = client.GetDatabase(configuration["DataBaseConfig:MongoServer:catalog"]);
            this.affCollection = db.GetCollection<AffiliateViewModel>("Affiliate");

        }
        public async Task<AffiliateViewModel> Add(AffiliateViewModel aff_item)
        {
            try
            {
                // Kiểm tra sp có trong giỏ hàng chưa
                var filter = Builders<AffiliateViewModel>.Filter.Where(x => x.client_id == aff_item.client_id); //FilterDefinition chỉ định một điều kiện tìm kiếm được sử dụng trong khi cập nhật một document
                var result_document = affCollection.Find(filter).ToList();

                if (result_document != null && result_document.Count > 0)
                {
                    return aff_item;
                }
                else
                {
                    await affCollection.InsertOneAsync(aff_item);
                    return aff_item;
                }
            }
            catch (Exception ex)
            {
                Utilities.LogHelper.InsertLogTelegram("Affiliate-- addNew - Update with id: " + aff_item.client_id + " error " + ex.ToString());
                return null;
            }
        }
       
        public async Task<int> IsAffiliateOrder(double client_id)
        {
            int result = -1;
            try
            {
                var builder = Builders<AffiliateViewModel>.Filter;
                var filter = builder.Eq("client_id", client_id);
                var result_document = await affCollection.FindAsync(filter).Result.ToListAsync();
                var latest = result_document.OrderByDescending(x => x.update_time).First();
                if (latest.utm_source != null)
                    switch (latest.utm_source)
                    {
                        case null: result = -1; break;
                        case "accesstrade": result = AffiliateType.accesstrade; break;
                    }

            }
            catch (Exception ex)
            {

                Utilities.LogHelper.InsertLogTelegram("Affiliate-- IsAffiliateOrder with " + client_id + " error " + ex.ToString());
            }
            return result;
        }
       

    }
}
