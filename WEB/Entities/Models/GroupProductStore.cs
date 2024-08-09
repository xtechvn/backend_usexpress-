using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class GroupProductStore
    {
        public long Id { get; set; }
        public int GroupProductId { get; set; }
        public int LabelId { get; set; }
        public string LinkStoreMenu { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
