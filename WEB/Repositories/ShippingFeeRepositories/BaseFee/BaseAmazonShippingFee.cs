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
    class BaseAmazonShippingFee : BaseFeeProductRepository
    {
        public BaseAmazonShippingFee(ProductBuyerViewModel ProductBuyer, List<PriceLevelViewModel> PriceLevelLabel, List<IndustrySpecialViewModel> IndustrySpecial)
            : base(ProductBuyer, PriceLevelLabel, IndustrySpecial) { }

        // Tính phí pound đầu
        public virtual double FirstPoundFee()
        {
            double price = ProductBuyer.Price; // Giá gốc sản phẩm đã + shippingFeeUS
            var fpf_level = PriceLevelLabel.OrderByDescending(x => x.FromDate).FirstOrDefault(x => x.FeeType == (int)FeeBuyType.FIRST_POUND_FEE && x.Offset <= price && price <= x.Limit); // lay ra han muc cua amz

            return fpf_level == null ? 0 : fpf_level.Price;
        }

        // Tính phí pound kế
        public virtual double NextPoundFee()
        {
            double NextPoudFee = 0;
            double pound = Math.Ceiling(ProductBuyer.Pound);

            // Lấy ra next pound fee mới nhất
            var npf_level = PriceLevelLabel.OrderByDescending(x => x.FromDate).FirstOrDefault(x => x.FeeType == (int)FeeBuyType.NEXT_POUND_FEE);

            NextPoudFee = npf_level == null ? 0 : npf_level.Price; //Price of Next pound fee  by label

            if (pound > 1)
            {
                NextPoudFee = Math.Ceiling(pound - 1) * NextPoudFee;
            }
            else
            {
                NextPoudFee = 0;
            }
            return NextPoudFee;
        }

        /// <summary>
        /// Chiết khấu được giảm cho phí của pound đầu tiên
        /// </summary>
        /// <returns></returns>
        public virtual double DiscountFirstFee()
        {
            var discount = PriceLevelLabel.OrderByDescending(x => x.FromDate).FirstOrDefault(x => x.FeeType == (int)FeeBuyType.DISCOUNT_FIRST_FEE);
            return discount == null ? 0 : discount.Discount;
        }

    }
}
