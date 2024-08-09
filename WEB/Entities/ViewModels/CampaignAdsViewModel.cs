using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    public class CampaignAdsViewModel : ProductClassification
    {
        public string GroupName { get; set; }
        public string LabelName { get; set; }
        public string CampaignName { get; set; }
    }
}
