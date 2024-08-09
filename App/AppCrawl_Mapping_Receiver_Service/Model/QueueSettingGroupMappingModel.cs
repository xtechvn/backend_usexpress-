using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace App_Crawl_Mapping_Receiver_Service_v2.Models
{
    public class QueueSettingGroupMappingModel: QueueSettingViewModel
    {
        public string queue_name { get; set; }
        public string queue_name_detail { get; set; }
    }
}
