using System;
using System.Collections.Generic;
using System.Text;

namespace App_Crawl_SearchList_Receiver.Models
{
    public class SLQueueItem
    {
        public int groupProductid { get; set; }
        public int labelid { get; set; }
        public string linkdetail { get; set; }
        public string? fixed_amount_id { get; set; }

    }
}
