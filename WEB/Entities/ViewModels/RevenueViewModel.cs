using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    public class RevenueViewModel
    {
        public int TotalOrder { get; set; }
        public double? Revenue { get; set; }
        public string RevenueStr { get; set; }
        public double CrawlPercent { get; set; }
        public int TotalProductNotFound { get; set; }
    }
}
