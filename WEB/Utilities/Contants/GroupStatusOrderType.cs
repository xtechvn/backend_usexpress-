using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Contants
{
    public enum GroupStatusOrderType
    {
        all_orders = -1, //Tất cả đơn hàng         
        wait_for_payment_count = 0, // chờ thanh toán
        wait_to_receive_count = 1, // chờ nhận hàng
        received_order_count = 2, // đã đặt hàng
        failed_order_count = 3 // HUY
    }
}
