using System;
using System.Collections.Generic;
using System.Text;

namespace AppMappingOrderDetail.Model
{
    public class ResponseData
    {
        public string status { get; set; }
        public string msg { get; set; }
        public string token { get; set; }
        public List<OrderModel> data { get; set; }
    }
    public class ResponseDataRemove
    {
        public string status { get; set; }
        public bool data { get; set; }
    }
}
