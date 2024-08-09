using AppMappingOrder.ViewModels.mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppMappingOrder.ViewModels
{
    /// <summary>
    /// object product ben usexpress old.
    /// </summary>
    public class ProductMappingModel
    {
        public int status { get; set; }
        public int viewer_timer_stamp { get; set; } //Thoi gian xem san pham gan nhat
        public DateTime GetApiDate { get; set; } // ngay get API crawl tu amazon
        public DateTime update_cache_time { get; set; } // Thời gian nạp cache gần nhất.
        public double ty_gia_usd { get; set; }
        public long ProductId { get; set; }
        public string CategoryName { get; set; }
        public long CategoryId { get; set; }
        public string ASIN { get; set; }
        public double PercentageSaved { get; set; }
        public string ProductName { get; set; }
        public string ProductGroup { get; set; }
        public string Condition { get; set; }
        public string PriceOld { get; set; }// gia goc chua giam
        public string PriceNew { get; set; }// gia sau khi saleoff
        public double PriceLast { get; set; }// gia bao gom cac loai phi tren doi
        public double ship_price_us { get; set; }// Phí ship noi dia US

        public double fee_luxury { get; set; }
        public double ship_buy_fee { get; set; }//TONG Phí MUA HO

        public int quantity { get; set; }
        public string Description { get; set; }

        public string ImageThumb { get; set; }
        public string Link { get; set; }
        public double SaveMoney { get; set; }
        public double AmountSaved { get; set; }
        public string TotalComment { get; set; }
        public string SalesRank { get; set; }

        //public List<ProductModel> SimilarProducts { get; set; }
        public string DetailPageURL { get; set; }
        public string ParentASIN { get; set; }
        public string[] Feature { get; set; }

        public string TotalRefurbished { get; set; }
        //public ItemAttributesItemDimensions ItemDimensions { get; set; }

        public Dictionary<string, double> PriceBuyDetails { get; set; } //chi phi mua ho chi tiet
        public string Binding { get; set; }
        public string UPC { get; set; }
      //  public ItemAttributesPackageDimensions PackageDimensions { get; set; }
        public string SKU { get; set; }
        public string Model { get; set; }
        public string IFrameURL { get; set; }
        public string Color { get; set; }
        public string size { get; set; }
        public string brand { get; set; }
        public string Merchant { get; set; }
        public string merchant_id { get; set; } // Là id của seller_id bán với giá đó
        public string Manufacturer { get; set; }

     //   public ImageSet[] ImageSets { get; set; }
        public bool IsEligibleForPrime { get; set; }
        //public Item[] ProductDetail { get; set; }
        //public ItemLookupResponse Variations { get; set; } // thong tin ve size, color.... san pham
  //      public List<ProductModel> objProductRelated { get; set; }// nhóm san phẩm liên quan
//        public List<UsExpress.Models.DBContext.Image> objImageProduct { get; set; } // Nhóm hình ảnh của sản phẩm

        public double stars { get; set; }
        public string[] product_details { get; set; }

        //public ItemLookupResponse obj_product_variations { get; set; } // lst obj này để chứa thông tin màu sắc kích thước của sản phẩm
        public List<variant_specifics> arr_size { get; set; }
        public List<variant_specifics> arr_color { get; set; }

        public int storeId { get; set; }
        public double percent { get; set; }

        public string j_product_detail { get; set; }// thong tin toan bo product convert duoi dang 

        public int apply_sale { get; set; } //sp nay co phai la giam gia hay ko. 1: giam gia. 0: la khong giam

        public string grey_text { get; set; }// THông báo thời gian hàng về
        public string offering_id { get; set; }

        public bool is_active { get; set; } = true; // Trạng thái khóa hay mở bán của sản phẩm. True: mo ban, false: khoa lai hien thi thong bao
        public string note_deactive { get; set; }// ghi chú với sản phẩm bị khóa không bày bán

        public List<offers_model> seller_list { get; set; }

        public bool is_has_pound { get; set; } = true; // Phân biệt sản phẩm không có cân nặng

        public string productSpecifications_crawl { get; set; } // TuanDo-14/12/2018-Lấy Specifications trong phần crawl data của anh Tuấn Nguyễn - Chưa remove style

        public string JsonCrawl { get; set; } // TuanDo-14/12/2018-Lấy toàn bộ chuỗi JSON trong phần crawl data của anh Tuấn Nguyễn - Dùng cho việc Insert dữ liệu vào DB
        public string CategoryNameCrawl { get; set; } // TuanDo-28/12/2018-Trả về category khi Crawl link - trường này không insert trong DB - chỉ dùng để mapping pound đối với ngành hàng
        public string UrlSearch { get; set; } // TuanDo-14/12/2018-Trả về đường link(url-costco) dùng để search

        public bool IsCrawl { get; set; } = false; // TuanDo-14/12/2018-Trường này để phân biệt đây là sản phẩm Crawl bằng link

        public bool is_has_seller { get; set; } = false; // true: báo hiệu Có giá ngoài mặt trang. Ko cần crawl trong sellerlist nữa
    }
}
