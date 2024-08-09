using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.CMS.Models
{
    public class RecentActivityLog
    {
        public string user_name { get; set; }
        public string log_type { get; set; }
        public string log { get; set; }
        public string time_from_today { get; set; }
        public string log_date { get; set; }

    }
}
