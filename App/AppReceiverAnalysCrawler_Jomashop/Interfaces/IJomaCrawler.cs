using AppReceiverAnalysCrawler_Jomashop.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AppReceiverAnalysCrawler_Jomashop.Interfaces
{
    public interface IJomaCrawler
    {
        public Task<ProductViewModel> CrawlDetail(ChromeDriver driver, IConfiguration _configuration, QueueMessage record);
        public Task<CrawlMethodOutput> CrawlDetailV2(ChromeDriver driver, IConfiguration _configuration, QueueMessage record);

    }
}
