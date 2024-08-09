using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.Product
{
  public  class SearchEsEntitiesViewModel
    {        
        public long total_item_store { get; set; } // Tổng số ket qua tìm thấy trên mặt trang gốc                
        public List<ProductViewModel> obj_lst_product_result { get; set; } // ds sp tìm được
    }
}
