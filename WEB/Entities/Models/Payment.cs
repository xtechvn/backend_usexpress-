using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Payment
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long? ProductId { get; set; }
        public int UserId { get; set; }
        public double Amount { get; set; }
        public int PaymentType { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Note { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
