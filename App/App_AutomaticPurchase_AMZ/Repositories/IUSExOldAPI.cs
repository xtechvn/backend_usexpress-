using App_AutomaticPurchase_AMZ.Model;
using Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using WinAppCheckoutAmazon.DBContext;

namespace App_AutomaticPurchase_AMZ.Repositories
{
    public interface IUSExOldAPI
    {
        public Task<MethodOutput> UpdateAmazonCart(AutomaticPurchaseAmz new_detail, USOLDToken api_Token, string url,string key= "U1qYbPRVdnNdKMC7pmJ0Qm96vJCLefzb6TKzPuEFRyZVPz1RwJ7Kbw6oUrXRh14ItgwPB7xFy4r6IrLL");
        public Task<MethodOutput> GetToken(string url,string user_name,string password);
        public Task<MethodOutput> SendEmailAPI(string email_url, USOLDToken api_Token, Dictionary<string, string> emailTemplate);
        public Task<MethodOutput> GetAmazonCart(string update_toNew_URL, string old_db_url, USOLDToken api_Token, string key = "U1qYbPRVdnNdKMC7pmJ0Qm96vJCLefzb6TKzPuEFRyZVPz1RwJ7Kbw6oUrXRh14ItgwPB7xFy4r6IrLL");

    }
}
