using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppReceiverAnalysCrawler_Jomashop.Models
{
    public class CrawlMethodOutput
    {
        public int status { get; set; }
        public string message { get; set; }
        public ProductViewModel product { get; set; }
    }
}
