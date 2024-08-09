using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class OrderProgress
    {
        public long Id { get; set; }
        public string OrderNo { get; set; }
        public short OrderStatus { get; set; }
        public DateTime CreateDate { get; set; }
    }


}
