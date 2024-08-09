using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crawler.Interfaces
{
    public interface ICrawlerByProduct
    {
        ProductViewModel CrawlProductDetail();        
    }
}
