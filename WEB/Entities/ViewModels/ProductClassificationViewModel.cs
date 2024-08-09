using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    public class ProductClassificationViewModel : ProductClassification
    {
        public string GroupName { get; set; }
        public string LabelName { get; set; }
        public string CampaignName { get; set; }
        public string ProductName { get; set; }
    }
}
