using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class ProductHistory
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public int UserId { get; set; }
        public short? UserType { get; set; }
        public int? Price { get; set; }
        public int? Rate { get; set; }
        public DateTime? CreateOn { get; set; }

        public virtual Product Product { get; set; }
    }
}
