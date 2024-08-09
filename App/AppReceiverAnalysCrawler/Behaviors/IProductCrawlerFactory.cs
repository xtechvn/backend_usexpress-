using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Chrome;
namespace AppReceiverAnalysCrawler.Behaviors
{
   public interface IProductCrawlerFactory
    {
        void DoSomeRealWork(string page_type, string product_code, int label_Id, string url, ChromeDriver browers,int group_product_id,int bot_type );
        void SyncElasticsearch(string product_manual_key_id, string group_id);
    }
}
