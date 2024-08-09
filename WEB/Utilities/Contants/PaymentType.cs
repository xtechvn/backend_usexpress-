using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Contants
{
    public enum PaymentType
    {
        USEXPRESS_BANK = 1, // Chuyển khoản trực tiếp cho USEXPRESS
        ATM_PAYOO_PAY = 5, // Thanh toán qua kênh trung gian Payoo hình thức ATM
        VISA_PAYOO_PAY = 6 // Thanh toán qua kênh trung gian Payoo hình thức VISA (Thẻ ghi nợ)
    }
}
