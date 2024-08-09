using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Crawler.Interfaces
{
    // SOLID: QUY TAC OPEN & CLOSED
    public interface ICrawler<T> where T : class
    {
        T Crawler(ICrawlerByProduct product_crawler);
    }
}
