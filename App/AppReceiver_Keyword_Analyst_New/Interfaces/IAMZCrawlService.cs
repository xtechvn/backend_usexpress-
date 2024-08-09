using AppReceiver_Keyword_Analyst_New.Model;
using AppReceiver_Keyword_Analyst_New.Models;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AppReceiver_Keyword_Analyst_New.Interfaces
{
    public interface IAMZCrawlService
    {
        Task<AMZSearchViewModel> CrawlSearchResult(ChromeDriver driver, SLQueueItem item);
    }
}
