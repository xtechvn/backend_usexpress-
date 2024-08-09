using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class UtmTrackers
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public int UtmSourceId { get; set; }
        public string UtmMedium { get; set; }
        public string UtmCampaign { get; set; }
        public DateTime? CreateOn { get; set; }
    }
}
