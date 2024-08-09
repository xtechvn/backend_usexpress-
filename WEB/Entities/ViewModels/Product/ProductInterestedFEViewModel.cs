using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Entities.ViewModels.Product
{
    public class ProductFEInterested
    {
        public long ID { get; set; }
        public string ProductCode { get; set; }
        public int LabelId { get; set; }
    }

    public class ProductInterestedFEViewModel
    {
        public double discount { get; set; }
        public string image_thumb { get; set; }
        public double star { get; set; }
        public long product_bought_quantity { get; set; }
        public bool is_prime_eligible { get; set; } // ham prime hay non prime
        public string label_name { get; set; } // nhan sp
        public string link_product { get; set; } // link san pham cua he thong
        public string product_name { get; set; } // Tên sản phẩm

        public string amount_vnd { get; set; } // Giá bán cuối cùng don vi vnd sau khi đã nhân với tỷ giá
        public string seller_name { get; set; } // ten seller
        public string brand_label_url { get; set; }
    }

}
