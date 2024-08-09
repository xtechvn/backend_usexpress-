using App_Crawl_TrackingFlightPackages.Model;
using System.Threading.Tasks;

namespace App_AutomaticPurchase_AMZ.Repositories
{
    public interface IUsExAPI
    {
        public Task<MethodOutput> UploadImage(string file_path, string us_ex_upload_domain = "https://image.usexpress.vn");

        public Task<MethodOutput> GetTrackingFlightList(string url);

    }
}
