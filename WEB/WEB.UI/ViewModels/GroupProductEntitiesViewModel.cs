
using System.Collections.Generic;


namespace WEB.UI.ViewModels
{
    public class GroupProductEntitiesViewModel
    {
       // public string base_url { get; set; }
       // public int cur_page { get; set; }
        public string group_product_name { get; set; }
      //  public long total_item_store { get; set; } // Tổng số sản phẩm có
      //  public int total_item { get; set; } // Tổng số sản phẩm sẽ hiển thị trên 1 mặt trang        
        public List<ProductListViewModel> obj_lst_product_result { get; set; } // ds sp tìm được
        public PaginationEntitiesViewModel Pagination { get; set; }
    }
}
