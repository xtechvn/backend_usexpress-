using App_AutoPurchase_TrackingOrders.Model;
using Entities.Models;
using Entities.ViewModels.AutomaticPurchase;
using System.Threading.Tasks;

namespace App_AutoPurchase_TrackingOrders.Repositories
{
    public interface IUsExAPI
    {
        public Task<MethodOutput> GetTrackingList(string url);
        public Task<MethodOutput> UpdateTrackingDetail(AutomaticPurchaseAmz new_detail, string url, string log, int user_excution = 64, string key = "1372498309AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k");
        public Task<MethodOutput> UploadImage(string file_path, string us_ex_upload_domain = "https://image.usexpress.vn");
    }
}
