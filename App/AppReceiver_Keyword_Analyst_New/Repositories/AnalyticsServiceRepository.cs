using AppReceiver_Keyword_Analyst_New.Interfaces;
using AppReceiver_Keyword_Analyst_New.Model;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace AppReceiver_Keyword_Analyst_New.Repositories
{
    public class AnalyticsServiceRepository : IAnalyticsService
    {
        private IConfiguration _configuration;
        private IAMZCrawlService _aMZCrawlService;
        public AnalyticsServiceRepository(IConfiguration configuration, IAMZCrawlService aMZCrawlService)
        {
            _configuration = configuration;
            _aMZCrawlService = aMZCrawlService;
        }
        public async Task<AMZSearchViewModel> CrawlData(ChromeDriver driver, SLQueueItem queue_item)
        {
            AMZSearchViewModel list = null;
            try
            {
                //AMZ
                switch (queue_item.label_Id)
                {
                    case 1:
                        {
                            list = await _aMZCrawlService.CrawlSearchResult(driver, queue_item);
                        }
                        break;
                    default: break;
                }


            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AppReceiver_Keyword_Analyst_New -   Crawl Keyword:  " + queue_item.keyword + " - Failed. Error: " + ex.ToString());
                Console.WriteLine("Crawl Keyword:  " + queue_item.keyword + " - Failed. Error: " + ex.ToString());
            }
            return list;
        }
    }
}
