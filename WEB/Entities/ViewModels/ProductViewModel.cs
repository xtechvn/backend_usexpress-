using System;
using System.Collections.Generic;
namespace Entities.ViewModels
{
    /// <summary>
    /// Chạy cho ES
    /// </summary>

    public class ProductViewModel
    {
        public long id { get; set; } // khóa chính identity       
        public long product_id { get; set; }
        public bool page_not_found { get; set; } // sản phẩm hết hàng, bị khóa hoặc lý do nào đó ko bán nữa trên mặt trang
        public string product_code { get; set; }  // mã sản phẩm quy ước chung cho các nhãn hàng
        public int product_map_id { get; set; } // Là id tự tăng của bảng orderitem bên hệ thống cũ
        public string product_name { get; set; } // Tên sản phẩm

        public double price { get; set; } // Chi phí gốc  don vi dollar   
        public double shiping_fee { get; set; } // phi noi dia My
        public double discount { get; set; }
        public double amount { get; set; } // Giá bán cuối cùng don vi dollar
        public double price_vnd { get; set; } // Giá bán trước giảm don vi vnd sau khi đã nhân với tỷ giá
        public double amount_vnd { get; set; } // Giá bán cuối cùng don vi vnd sau khi đã nhân với tỷ giá
        public double rate { get; set; } // Tỷ giá VNĐ
        public string link_product { get; set; } // link san pham cua he thong
        public string image_thumb { get; set; } // link ảnh đại diện
        public List<string> image_product { get; set; } // link ảnh sp
        public List<ImageSizeViewModel> image_size_product { get; set; } // link ảnh sp theo các kích cỡ
        public List<ColorImagesViewModel> color_images { get; set; } // Link ảnh của màu 
        public List<string> variation_name { get; set; } // tên biến thể
        public DateTime create_date { get; set; }// ngày tạo sản phẩm

        public string rating { get; set; } // thu hang sp
        public string manufacturer { get; set; } // nha sx
        public int reviews_count { get; set; } // so luot review sp
        public double[] reviews_table { get; set; } // so luot review sp
        public double star { get; set; }
        public int label_id { get; set; } // nhan sp
        public string label_name { get; set; } // nhan sp
        public bool is_prime_eligible { get; set; } // ham prime hay non prime
        public string seller_id { get; set; } // id seller
        public string seller_name { get; set; } // ten seller
        public string in_stock { get; set; }
        public string variations { get; set; }
        public string page_source_html { get; set; } // html page 
        public List<VariationViewModel> list_variations { get; set; }
        public string img_variation { get; set; } // Lấy ra danh sách ảnh của của các màu
        public DateTime update_last { get; set; } // ngay cap nhat gan nhat
        public List<string> dimensions_display_image { get; set; } // quyết định dc việc variation có ảnh ko | true la co anh
        public bool is_crawl_weight { get; set; } // param này cho biết sp này có cân nặng hay ko
        public string item_weight { get; set; } // can nang
        public long product_ratings { get; set; } // số lượt đánh giá sản phẩm
        public ProductFeeViewModel list_product_fee { get; set; }
        public string keywork_search { get; set; } // link user nhập
        public int regex_step { get; set; } // Trình tự bước regex trang. 1: Regex trang chi tiết ở màn hình đầu tiên. 2 regex weight và các thành phần tiếp theo
        public List<SellerListViewModel> seller_list { get; set; }// danh sach seller ban cung san pham nay
        public bool is_has_seller { get; set; }// detect seller more in page
        public bool is_redirect_extension { get; set; }// detect có phải sp được gọi từ Extension Chrome không
        public int group_product_id { get; set; } // Nhóm sản phẩm
        public string group_product_name { get; set; }
        public int industry_special_type { get; set; } //Sản phẩm có thuộc nhóm hàng đặc thù nào không, từ AllCodeType.INDUSTRY_SPECIAL_TYPE
        public int product_type { get; set; } // 0:auto crawl, 1: manual , 2: báo giá thủ công
        public long product_bought_quantity { get; set; } // so luong mua
        public bool is_amazon_choice { get; set; } // Có phải sản phẩm amz choice hay không
        public string product_freq_buy_with { get; set; } // các sản phẩm liên quan thường được mua cùng.
        public double product_save_price { get; set; }  // Giá giảm so với giá gốc
        public List<ProductRelated> product_related { get; set; } // các sản phẩm liên quan
        public Dictionary<string,string> product_specification { get; set; } // thông số về sản phẩm.
        public List<string> product_infomation { get; set; } // Thông tin sản phẩm.

        public DateTime sale_exprire_time { get; set; } // Thời gian kết thúc sale
        public List<ProductVideo> product_videos { get; set; } // Thông tin link video quảng cáo sản phẩm
        public DateTime product_lifetime_start { get; set; }
        public DateTime product_lifetime_end { get; set; }
        public int product_status { get; set; } // Trạng thái sản phẩm hiển thị trên trang : 0 Hoạt động, -1: không tìm thấy sản phẩm.
        public string product_infomation_HTML { get; set; } // thông tin sản phẩm, edit bằng tiny MCE
        public double package_volume_weight { get; set; } // thể tích sản phẩm quy đổi, tính theo pounds
        public string selected_weight { get; set; } // cân nặng được lựa chọn
    }

    //public class EsProductViewModel : ProductViewModel
    //{
    //    [PropertyName("es_id")]
    //    public override long id { get; set; } // khóa chính identity    
    //}
    public class ProductFilterModel
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int[] Labels { get; set; }
        public int[] Categories { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int Status { get; set; }

        /// <summary>
        ///  0: Ngày tạo
        ///  1: Giá về tay
        ///  2: Đã mua 
        ///  3: Phí mua hộ
        /// </summary>
        public int SortField { get; set; }

        /// <summary>
        /// 0 : Up
        /// 1 : Down
        /// </summary>
        public int SortType { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class ESModifyProductModel
    {
        public string product_code { get; set; }
        public int label_id { get; set; }
        public int group_product_id { get; set; }
        public string group_product_name { get; set; }
        public int product_bought_quantity { get; set; }
    }
    public class ProductRelated
    {
        public string product_code { get; set; }
        public string product_image { get; set; }
        public string product_name { get; set; }
        public double rate { get; set; } // Số sao
        public double ratings { get; set; } // lượt đánh giá
        public double price { get; set; } // giá sp
        public bool is_prime_eligible { get; set; } // sp prime hay non prime

    }
    public class ProductVideo
    {
        public string url { get; set; } 
        public string thumb { get; set; } 
        public int width { get; set; } 
        public int height { get; set; } 
    }
}
