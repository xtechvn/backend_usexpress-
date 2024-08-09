using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Utilities.Contants;

namespace Repositories.ShippingFeeRepositories.BaseFee
{

    /// <summary>
    ///Áp dụng quy tắc SOLID: Liskov để định nghĩa 1 lớp Child trừu tượng là lớp phí mua hộ cho Amazon
    ///Lớp này là lớp Base và k dc phép sửa đổi
    ///
    /// </summary>
    class BaseCostcoShippingFee : BaseFeeProductRepository
    {
        public BaseCostcoShippingFee(ProductBuyerViewModel ProductBuyer, List<PriceLevelViewModel> PriceLevelLabel, List<IndustrySpecialViewModel> IndustrySpecial)
             : base(ProductBuyer, PriceLevelLabel, IndustrySpecial) { }


        // 1. Processing Fee (PrF) là phí xử lý: PrF = $3 với mọi sản phẩm
        public virtual double ProcessFee()
        {
            var pf_level = PriceLevelLabel.OrderByDescending(x => x.FromDate).FirstOrDefault(x => x.FeeType == (int)FeeBuyType.FIRST_POUND_FEE);
            return pf_level == null ? 0 : pf_level.Price;
        }

        // Tính phí pound xử lý cân nặng
        public virtual double PoundFee()
        {
            double pound =Math.Ceiling(ProductBuyer.Pound);

            // Lấy ra next pound fee mới nhất
            var pound_fee = PriceLevelLabel.OrderByDescending(x => x.FromDate).FirstOrDefault(x => x.FeeType == (int)FeeBuyType.NEXT_POUND_FEE);

            return (pound_fee == null ? 0 : pound_fee.Price) * pound; //Phí xử lý cân nặng;
        }

     


    }
}
