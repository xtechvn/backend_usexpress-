using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class CampaignGroupProduct
    {
        public int Id { get; set; }
        public int GroupProductId { get; set; }
        public int CampaignId { get; set; }
        public int Status { get; set; }
        public int? OrderBox { get; set; }
    }
}
