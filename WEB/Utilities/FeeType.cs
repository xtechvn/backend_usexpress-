using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public class FeeType
    {

        public const string FEE_SHIPING_PRICE_US = "FEE_SHIPING_PRICE_US"; // phí ship nội địa Mỹ
        public const string FIRST_POUND_FEE = "FIRST_POUND_FEE";
        public const string PRODUCT_PRICE_FEE = "PRODUCT_PRICE_FEE";
        public const string POUND_FEE = "POUND_FEE";
        public const string CUBIC_FEE = "CUBIC_FEE";
        public const string PHONE_FEE = "PHONE_FEE";
        public const string APPLE_FEE = "APPLE_FEE";
        public const string BATTERY_FEE = "BATTERY_FEE";
        public const string LUXURY_FEE = "LUXURY_FEE";
        public const string LIQUID_FEE = "LIQUID_FEE";
        public const string TOTAL_FEE = "TOTAL_FEE";
        public const string DISCOUNT_PRODUCT_2ND = "DISCOUNT_PRODUCT_2ND";

        public const string FIRST_PROCESS = "FIRST_PROCESS";

        public const string FEE_WEIGHT_POUND = "FEE_WEIGHT_POUND";

        public static string GetStatus(string _FeeType)
        {

            string orderStatusName = "";
            switch (_FeeType)
            {
                case FeeType.POUND_FEE:
                    orderStatusName = "Phí mua hộ các pound tiếp theo";
                    break;

                case FeeType.FIRST_POUND_FEE:
                    orderStatusName = "Phí mua hộ pound đầu tiên";
                    break;

                case FeeType.FEE_WEIGHT_POUND:
                    orderStatusName = "Phí theo cân nặng (Pound)";
                    break;

                case FeeType.FIRST_PROCESS:
                    orderStatusName = "Phí xử lý sản phẩm";
                    break;

                case FeeType.FEE_SHIPING_PRICE_US:
                    orderStatusName = "Phí vận chuyển nội địa Mỹ";
                    break;

                case FeeType.LUXURY_FEE:
                case FeeType.PHONE_FEE:
                case FeeType.CUBIC_FEE:
                case FeeType.APPLE_FEE:
                case FeeType.BATTERY_FEE:
                case FeeType.DISCOUNT_PRODUCT_2ND:

                    orderStatusName = "Phụ thu hàng hóa đặc biệt";
                    break;

                default:
                    orderStatusName = "Phí mua hộ";
                    break;

            }
            return orderStatusName;
        }
    }
}
