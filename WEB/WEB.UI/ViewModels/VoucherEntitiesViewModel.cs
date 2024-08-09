using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.UI.ViewModels
{
    public class VoucherEntitiesViewModel
    {
        public int status { get; set; }
        public double total_price_sale { get; set; }
        public string msg_response { get; set; }
        public int voucher_id { get; set; }
        public string voucher_name { get; set; }        
        public string desc { get; set; }
        public string discount { get; set; }
        public string unit { get; set; }
        public DateTime from_date { get; set; }
        public DateTime to_date { get; set; }
        public string expire_date { get; set; }
        public int rule_type { get; set; }
    }
}
