using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Voucher
    {
        public Voucher()
        {
            VoucherLogActivity = new HashSet<VoucherLogActivity>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public DateTime? Cdate { get; set; }
        public DateTime? Udate { get; set; }
        public DateTime? EDate { get; set; }
        public int LimitUse { get; set; }
        public decimal? PriceSales { get; set; }
        public string Unit { get; set; }
        public int? RuleType { get; set; }
        public string GroupUserPriority { get; set; }
        public bool? IsPublic { get; set; }
        public string Description { get; set; }
        public bool? IsLimitVoucher { get; set; }
        public double? LimitTotalDiscount { get; set; }
        public string StoreApply { get; set; }
        public bool? IsFreeLuxury { get; set; }
        public bool? IsMaxPriceProduct { get; set; }
        public double? MinTotalAmount { get; set; }
        public bool? IsMaxFee { get; set; }
        public int? CampaignId { get; set; }
        public bool? IsPriceFirstPoundFee { get; set; }

        public virtual ICollection<VoucherLogActivity> VoucherLogActivity { get; set; }
    }
}
