using App_AutomaticPurchase_AMZ.Model;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_AutomaticPurchase_AMZ.Repositories
{
    public interface IUsExAPI
    {
        public Task<MethodOutput> GetPurchaseItems(string url);
        public Task<MethodOutput> UpdatePurchaseDetail(AutomaticPurchaseAmz new_detail, string url, string log, int user_excution = 64, string key = "1372498309AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k");
        public Task<MethodOutput> CheckIfPurchased(AutomaticPurchaseAmz new_detail, string url, int user_excution = 64, string key = "1372498309AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k");

        public Task<MethodOutput> AddNewItem(string url, AutomaticPurchaseAmz new_detail, int user_excution = 64, string key = "1372498309AAH0fVJfnZQFg5Qaqro47y1o5mIIcwVkR3k");
        public Task<MethodOutput> UploadImage(string file_path, string us_ex_upload_domain = "https://image.usexpress.vn");


    }
}
