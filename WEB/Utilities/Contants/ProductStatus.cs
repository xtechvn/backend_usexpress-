using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Contants
{
    public enum ProductStatus
    {
       NOT_FOUND = -1, //Không hoạt động , Không hiển thị thông tin và thông báo không tìm thấy sản phẩm.
       NORMAL_SELL = 0, // Sản phẩm đang được bán bình thường và có hiển thị thông tin tại FR
       NOT_AVAILABLE =1, //Sản phẩm có hiển thị thông tin nhưng không thể mua được, thông báo liên hệ CSKH
    }
}
