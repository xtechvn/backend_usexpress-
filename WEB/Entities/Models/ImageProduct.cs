using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class ImageProduct
    {
        public long Id { get; set; }
        public string Image { get; set; }
        public long? ProductId { get; set; }

        public virtual Product Product { get; set; }
    }
}
