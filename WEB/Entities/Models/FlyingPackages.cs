using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Entities.Models
{
    public partial class FlyingPackages
    {
        public long Id { get; set; }
        public string Awbcode { get; set; }
        public string AirlinesId { get; set; }
        public string OrderList { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdateLast { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public int? TotalPieces { get; set; }
        public double? TotalWeight { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string Note { get; set; }
    }
}
