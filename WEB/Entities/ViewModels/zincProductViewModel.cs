using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    [Serializable]
    public class zincProductViewModel
    {
        public string asin { get; set; }
        public string status { get; set; }
        public string extra_description { get; set; }
        public string seller_id { get; set; }
        public string shipping_weight { get; set; }
        public string shipping_weight_unit { get; set; }
        public string product_name { get; set; }
        public bool is_prime_eligible { get; set; }
        public string description { get; set; }
        public string ratting { get; set; }
        public string featured_merchant { get; set; }
        public string manufacturer { get; set; }
        public string retailer { get; set; }
        public string product_description { get; set; }
        public string seller_name { get; set; }
        public string package_dimensions { get; set; }
        //public string epids { get; set; }//
        public string product_id { get; set; }
        public string price_shipping { get; set; }
        public string review_count { get; set; }
        //  public string epids_map { get; set; }
        public string title { get; set; }
        public string question_count { get; set; }
        public string answered_questions_count { get; set; }
        public string brand { get; set; }
        public string stars { get; set; }
        // public string fresh { get; set; }
        public string price { get; set; }
        public string main_image { get; set; } // co hoac khong
        public List<string> images { get; set; } // co hoac khong
    }
    public class resultproductViewModel
    {
        public string stars { get; set; }//sao
        public string title { get; set; }//ten sp
        public string seller_name { get; set; }//nguoi ban
        public string asin { get; set; }//ma san pham
        public string price_last_vnd { get; set; }//tong gia VNd (da bao gom thue, phi)
        public string price_last { get; set; }//tong gia(da bao gom thue, phi)
        public string shipping_details { get; set; }//chi tiet bang gia phi mua ho
        public string first_pound_fee { get; set; }//phi mua ho pound dau tien
        public string luxury_fee { get; set; }//phu thu hang hoa dac biet
        public string link_amazon { get; set; }//link sp tren amz
        public string sales_rank { get; set; }//danh gia san pham
        public string ship_price_us { get; set; }//phi ship noi dia My
        public string ship_buy_fee { get; set; } //phi mua ho tu US Express
        public string price { get; set; }//gia san pham tren amazon
    }
}
