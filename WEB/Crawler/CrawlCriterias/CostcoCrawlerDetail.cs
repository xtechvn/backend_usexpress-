using Crawler.Interfaces;
using Entities.Models;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Crawler.CrawlCriterias
{
    public class CostcoCrawlerDetail : ICrawlerByProduct
    {
        string _product_code;
        public CostcoCrawlerDetail(string product_code)
        {
            _product_code = product_code;            
        }

        public ProductViewModel CrawlProductDetail()
        {
            return null;
        }
    }
}
