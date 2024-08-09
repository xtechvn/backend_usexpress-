using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.UI.ViewModels
{
    public class SearchEntitiesViewModel
    {
        public string keyword { get; set; }
        public int total_item_store { get; set; } // Tổng số sản phẩm tìm thấy trên mặt trang gốc
        public int total_item { get; set; } // Tổng số sản phẩm sẽ hiển thị trên mặt trang
        public int page_index { get; set; } // Trang focus
        public List<ProductListViewModel> obj_lst_product_result { get; set; } // ds sp tìm được
    }
}
