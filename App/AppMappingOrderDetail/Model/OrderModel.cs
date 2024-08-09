using System;
using System.Collections.Generic;
using System.Text;

namespace AppMappingOrderDetail.Model
{
    public class OrderModel
    {
        public int id { get; set; }
        public int order_id { get; set; }
        public int payment_type { get; set; }
        public int payment_status { get; set; }
        public string order_no { get; set; }
        public string create_date { get; set; }
        public string order_status { get; set; }

    }
}
