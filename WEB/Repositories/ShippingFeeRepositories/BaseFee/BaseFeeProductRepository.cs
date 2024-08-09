using Entities.ViewModels;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Contants;

namespace Repositories.ShippingFeeRepositories.BaseFee
{
    /// <summary>
    ///Áp dụng quy tắc SOLID: Liskov để định nghĩa 1 lớp PARENT trừu tượng là lớp phí mua hộ
    /// ///Lớp này là lớp Base và k dc phép sửa đổi
    /// </summary>
    public class BaseFeeProductRepository 
    {
        public ProductBuyerViewModel ProductBuyer { get; set; } // san pham ma nguoi dung se dat mua
        public List<PriceLevelViewModel> PriceLevelLabel { get; set; } // Danh sách các loại phí của 1 Label
        public List<IndustrySpecialViewModel> IndustrySpecial { get; set; } // Danh sach cac nganh hàng đặc biệt để tính phí Luxury

        public BaseFeeProductRepository(ProductBuyerViewModel _ProductBuyer, List<PriceLevelViewModel> _PriceLevel, List<IndustrySpecialViewModel> _IndustrySpecial)
        {
            ProductBuyer = _ProductBuyer;
            PriceLevelLabel = _PriceLevel;
            IndustrySpecial = _IndustrySpecial;
        }

        // Tính phí luxury 
        /// <summary>
        /// Luxury Fee (LF) là phí hàng cao cấp, áp dụng với các sản phẩm có giá từ $200 sẽ tính phí 9%
        ///giá sản phẩm.Riêng các sản phẩm đặc thù sẽ có mức LF riêng, hiện tại các sản phẩm đặc thù
        ///gồm: Sản phẩm Apple, Điện thoại, Máy tính, Máy tính bảng
        /// </summary>
        /// <returns></returns>
        public virtual double LuxuryFee()
        {
            try
            {
                double price_product = ProductBuyer.Price;// da bao gom shipping fee
                double price_discount = 0;  // Phần trăm sản phẩm khi vượt quá số tiền luxury cho phép          
                double price_special = 0;// Giá phụ trội khi sản phẩm thuộc ngành hàng đặc biệt
                double luxury_fee = 0;
                var luxury_discount = PriceLevelLabel.OrderByDescending(x => x.FromDate).FirstOrDefault(x => x.FeeType == (int)FeeBuyType.LUXURY_FEE);
                if (luxury_discount != null)
                {
                    if (price_product >= luxury_discount.Offset) // giá sản phẩm lơn hơn mức giá Luxury
                    {
                        // Kiểm tra ngành hàng của sản phẩm có nằm trong nhóm ngành hàng đặc biệt không ?
                        var obj_industry_special = IndustrySpecial.FirstOrDefault(x => x.SpecialType == ProductBuyer.IndustrySpecialType);
                      

                        if (obj_industry_special != null)
                        {
                            // Giá phụ trội
                            price_special = obj_industry_special.Price;

                            price_discount = luxury_discount == null ? 0 : (ProductBuyer.Price * luxury_discount.Discount) / 100; // phí phụ trội luxury theo giá sp
                            //Kiểm tra nhãn hàng này 

                            // Nhóm sản phẩm áp dụng so sánh phí sản phẩm nội trội so với chiết khấu Luxury
                            if (obj_industry_special.GroupLabelId.IndexOf(ProductBuyer.LabelId.ToString()) >= 0)
                            {
                                if (obj_industry_special.IsAllowDiscountCompare) // Kiểm tra loại ngành hàng special này có cho phép so sánh với phí phụ trội ko
                                {
                                    luxury_fee = Math.Max(price_discount, price_special); // Lấy ra mức phí phụ trội cao nhất                            
                                }
                                else
                                {
                                    luxury_fee = price_special; // lay gia muc phi phu troi theo nganh hang
                                }
                            }
                            else
                            {
                                // Áp dung chiết khấu Luxury cho đơn giá sản phẩm
                                price_discount = (ProductBuyer.Price * luxury_discount.Discount) / 100; // phí phụ trội luxury theo giá sp
                                luxury_fee = price_discount; // lay gia muc phi phu troi theo nganh hang
                            }                          
                        }
                        else
                        {
                            // Ko detect được nhóm ngành cho sản phẩm mặc định tính theo ck của luxury fee
                            price_discount = (ProductBuyer.Price * luxury_discount.Discount) / 100; // phí phụ trội luxury theo giá sp
                            luxury_fee = price_discount; // lay gia muc phi phu troi theo nganh hang
                           
                        }
                        return luxury_fee;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("LuxuryFee - amz" + ex.Message);
                return 0;
            }
        }

    }

}
