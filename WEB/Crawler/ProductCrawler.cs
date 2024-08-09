using Entities.Models;
using Entities.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crawler.Interfaces;
using Utilities;

namespace Crawler
{
    public class ProductCrawler : ICrawler<ProductViewModel>
    {
        public ProductViewModel Crawler(ICrawlerByProduct product)
        {
            try
            {
                //var product_detail = new Dictionary<string, object>
                //{
                //    {"detail",product_crawler.CrawlProductDetail()}                    
                //};

                return product.CrawlProductDetail();
            }
            catch (Exception)
            {                
                return null;
            }
        }

    }
}

