using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class PropertiesProduct
    {
        public int Id { get; set; }
        public long ProductId { get; set; }
        public int PropertiesProductId { get; set; }
        public DateTime? CreateOn { get; set; }
        public string Value { get; set; }

        public virtual Product Product { get; set; }
    }
}
