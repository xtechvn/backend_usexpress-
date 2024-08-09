using System;
using System.Collections.Generic;
using System.Text;

namespace AppReceiver_Keyword_Analyst_New.Model
{
    public class AmzSearchResult
    {
        //-- URL tren trang Label: amazon.com/....., 
        //-- public string url_in_store { get; set; }
        public string url_store  { get; set; }
        //-- URL Image
        public string image_url { get; set; }
        //-- URL tren USExpress
        //-- public string url_in_usexpress  { get; set; }
        public string url { get; set; }
        //-- Ten sp:
        public string product_name { get; set; }
        //-- Star danh gia
        public double star { get; set; }
        //-- So luong review
        public int reviews_count { get; set; }
        //-- Ma san pham:
        //-- public string asin { get; set; }
        public string product_code { get; set; }
        //-- Gia san pham:
        public double price { get; set; }
    }
    public class AMZSearchViewModel {
        //-- Danh sách sản phẩm trong trang search
        public List<AmzSearchResult> data { get; set; }
        //-- Tổng page
        public int total_page { get; set; }
    }
}
