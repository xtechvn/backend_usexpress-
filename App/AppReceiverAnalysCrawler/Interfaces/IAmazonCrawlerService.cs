using Entities.ViewModels;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppReceiverAnalysCrawler.Interfaces
{
    public interface IAmazonCrawlerService 
    {
        ProductViewModel crawlerProductAmazon(ChromeDriver browers,string url,string product_code,int group_product_id);
        ProductViewModel crawlProductMoreAmazon(ChromeDriver browers, ProductViewModel product_detail_result, string page_source);
        List<SellerListViewModel> getPriceBySellers(ChromeDriver browers, string page_source_html);
        //Dictionary<string, double> getFeeForProduct(double price, string weight, string asin);
        //double calculatorAmountLast(double rate, Dictionary<string, double> product_fee, string asin);
    }
}
