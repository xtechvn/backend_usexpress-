using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Contants
{
    public enum PackagesShipmentStatus
    {
        Initialization=0, //khởi tạo, chỉ có thông tin mã awb và đơn hàng trong package
        OnPlane=1, //đã lên máy bay, có mã chuyến bay đầu tiên, ghi log
        OnFly=2, // Đang bay, đã có thời điểm cất cánh, chưa có time hạ cánh chính xác
        ArrivalTochangeover = 3, // Hạ cánh để chuyển bay
        ShipmentArrived = 4, // Packages đã hạ cánh
        ShipmentManifested = 5, // Packages đang được kê khai
        ShipmentAccepted = 6, // Packages được chấp thuận / thông quan
        ShipmentMatched = 7, // Packages được xác nhận là đúng
        ShipmentDelivered = 8, // Packages đã được chuyển đến destination
        Completed =9 // Packages đã tới địa điểm cuối cùng.
    }
    public enum FlightCarriesID
    {
        EVAAirCargo=0,
        CathayPacificCargo=1,
        AnaCargo=2
    }
}
