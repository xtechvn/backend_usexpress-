using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEB.UI.Common;

namespace WEB.UI.ViewModels
{
    public class PaginationEntitiesViewModel
    {
        public int cur_page { get; set; } // trang hiện tại
        
        public long total_item_store { get; set; } // tong so sp co trong 1 muc
        public int number_page { get; set; } // tổng số trang
        public string base_url { get; set; } // link đến trang kế
        public int per_page { get; set; } // tổng số sp hiển thị trên mặt trang
    }
}
