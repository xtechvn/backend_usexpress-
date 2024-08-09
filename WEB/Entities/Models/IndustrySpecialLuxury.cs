using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class IndustrySpecialLuxury
    {
        public int Id { get; set; }
        public int SpecialType { get; set; }
        public double Price { get; set; }
        public int Status { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ExpireDate { get; set; }
        public DateTime? UpdateLast { get; set; }
        public string GroupLabelId { get; set; }
        public bool? IsAllowDiscountCompare { get; set; }
    }
}
