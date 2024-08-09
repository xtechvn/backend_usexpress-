
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
    /// Phí mua hộ của sản phẩm  được kế thừa bởi 1 lớp AmazonShippingFee  cho amz đã được định nghĩa
    /// Lớp này được phép chỉnh sửa các hàm của lớp cha nhưng ko được thay đổi tính đúng đắn của lớp cha
    /// </summary>
    class FeeNordstromRack : BaseJomashopShippingFee
    {
        //Inject đầu vào
        public FeeNordstromRack(ProductBuyerViewModel ProductBuyer, List<PriceLevelViewModel> PriceLevelLabel, List<IndustrySpecialViewModel> IndustrySpecial)
                : base(ProductBuyer, PriceLevelLabel, IndustrySpecial) { }


        // Hàm tính phí mua hộ
        // Được phép chỉnh sửa các hàm của lớp cha nhưng không được thay đổi tính đúng đắn của lớp cha        
        public override double FirstPoundFee()
        {
            return base.FirstPoundFee();
        }

        public override double LuxuryFee()
        {
            return base.LuxuryFee();
        }
        public override double DiscountFirstFee()
        {
            return base.DiscountFirstFee();
        }
        public override double ShippingFeeUs()
        {
            return base.ShippingFeeUs();
        }

        // return ra list cac loai phi
        public Dictionary<string, double> getNordstromRackShippingFee()
        {
            try
            {
                var shipping_fee = new Dictionary<string, double>
                {
                    { FeeBuyType.ITEM_WEIGHT.ToString(),ProductBuyer.Pound },
                    { FeeBuyType.FIRST_POUND_FEE.ToString(),FirstPoundFee() },
                    { FeeBuyType.LUXURY_FEE.ToString(),LuxuryFee() },
                    { FeeBuyType.DISCOUNT_FIRST_FEE.ToString(),DiscountFirstFee() },
                    { FeeBuyType.SHIPPING_US_FEE.ToString(),ShippingFeeUs() },
                    { FeeBuyType.TOTAL_SHIPPING_FEE.ToString(),FirstPoundFee() +  LuxuryFee() },
                    { FeeBuyType.PRICE_LAST.ToString(),FirstPoundFee()+ LuxuryFee()+ ProductBuyer.Price}
                };
                return shipping_fee;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("getNordstromRackShippingFee" + ex.Message);
                return null;

            }
        }

    }
}
