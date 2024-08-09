using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utilities;
using Utilities.Contants;

namespace Repositories.ShippingFeeRepositories.BaseFee
{
    /// <summary>
    ///Áp dụng quy tắc SOLID: Liskov để định nghĩa 1 lớp trừu tượng là lớp phí mua hộ cho Amazon
    ///Lớp này là lớp Base và k dc phép sửa đổi
    ///
    /// </summary>
    class BaseJomashopShippingFee : BaseFeeProductRepository
    {
        public BaseJomashopShippingFee(ProductBuyerViewModel ProductBuyer, List<PriceLevelViewModel> PriceLevelLabel, List<IndustrySpecialViewModel> IndustrySpecial)
            : base(ProductBuyer, PriceLevelLabel, IndustrySpecial) { }

        // Tính phí pound đầu
        public virtual double FirstPoundFee()
        {
            double price = ProductBuyer.Price; // Giá gốc sản phẩm đã + shippingFeeUS
            var fpf_level = PriceLevelLabel.OrderByDescending(x => x.FromDate).FirstOrDefault(x => x.FeeType == (int)FeeBuyType.FIRST_POUND_FEE && x.Offset <= price && price < x.Limit); // lay ra han muc cua 

            return fpf_level == null ? 0 : fpf_level.Price;
        }
        public virtual double DiscountFirstFee()
        {
            var discount = PriceLevelLabel.OrderByDescending(x => x.FromDate).FirstOrDefault(x => x.FeeType == (int)FeeBuyType.DISCOUNT_FIRST_FEE);
            return discount == null ? 0 : discount.Discount;
        }

        /// <summary>
        /// Tinsh phi noi dia US
        /// </summary>
        /// <returns></returns>
        public virtual double ShippingFeeUs()
        {
            double shipping_fee_us = 0;
            var Price_fee = PriceLevelLabel.OrderByDescending(x => x.FromDate).FirstOrDefault(x => x.FeeType == (int)FeeBuyType.SHIPPING_US_FEE && ProductBuyer.Price >= x.Offset && ProductBuyer.Price < x.Limit);
            if (Price_fee != null)
            {
                shipping_fee_us = Price_fee.Price; // so tien phu troi
            }
            return shipping_fee_us;
        }
    }
}
