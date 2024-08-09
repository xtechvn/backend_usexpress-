using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
  public  class SellerListViewModel
    {
        public string seller_percent_positive { get; set; } //% độ tin cậy        
        public double price { get; set; } // giá sản phẩm
        public double shiping_fee { get; set; } //phi ship noi dia
        public string seller_name { get; set; } // ten seller        
        public string ratings { get; set; } 
        public string seller_id { get; set; }//id nguoi ban
        public string offerlisting_id { get; set; }        
        public string condition { get; set; } // loại hàng . ví dụ; user hay new hay tân trang                
        public string stars { get; set; } // thứ hạng sao
        public string arrives { get; set; } // ngày giao tới kho Mỹ
        public string fastest_delivery { get; set; } // thời gian giao nhanh nhất 
        public DateTime update_last { get; set; } // ngay cap nhat gia gan nhat
    }
}
