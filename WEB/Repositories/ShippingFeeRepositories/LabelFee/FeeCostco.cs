using Entities.ViewModels;
using Repositories.ShippingFeeRepositories.BaseFee;
using System;
using System.Collections.Generic;
using Utilities;
using Utilities.Contants;

namespace Repositories.ShippingFeeRepositories.LabelFee
{
    /// <summary>
    ///Áp dụng quy tắc SOLID: Liskov để định nghĩa 1 lớp kế thừa lớp BASE
    /// Giá của sản phẩm  được kế thừa bởi 1 lớp base giá cho amz dc xây dựng từ trước
    /// Lớp này được phép chỉnh sửa các hàm của lớp cha nhưng ko được thay đổi tính đúng đắn của lớp cha
    /// </summary>
    class FeeCostco : BaseCostcoShippingFee
    {
        public FeeCostco(ProductBuyerViewModel ProductBuyer, List<PriceLevelViewModel> PriceLevelLabel, List<IndustrySpecialViewModel> IndustrySpecial)
               : base(ProductBuyer, PriceLevelLabel, IndustrySpecial) { }

        public override double ProcessFee()
        {
            return base.ProcessFee();            
        }
        public override double PoundFee()
        {
           return base.PoundFee();           
        }
        public override double LuxuryFee()
        {
            return base.LuxuryFee();
        }

        // return ra list cac loai phi
        public Dictionary<string, double> getCostcoShippingFee()
        {
            try
            {
                var shipping_fee = new Dictionary<string, double>
                {
                    { FeeBuyType.ITEM_WEIGHT.ToString(),ProductBuyer.Pound },
                    { FeeBuyType.FIRST_POUND_FEE.ToString(),ProcessFee() },
                    { FeeBuyType.NEXT_POUND_FEE.ToString(),PoundFee() },
                    { FeeBuyType.LUXURY_FEE.ToString(),LuxuryFee() },
                    { FeeBuyType.TOTAL_SHIPPING_FEE.ToString(),ProcessFee() + PoundFee() + LuxuryFee() },
                    //Minh Nguyen: Them thong so sao cho trung voi amazon:
                    { FeeBuyType.DISCOUNT_FIRST_FEE.ToString(), 0 },
                    { FeeBuyType.PRICE_LAST.ToString(),ProcessFee() + PoundFee() + LuxuryFee()+ ProductBuyer.Price}
                };
                return shipping_fee;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getAmazoneShippingFee" + ex.Message);
                return null;
            }
        }
    }
}
