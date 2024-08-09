using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Contants
{
    public enum LogActivityType
    {
        ERROR=-1,
        CHANGE_ORDER_CMS=0,
        LOGIN_CMS = 1,
        CHANGE_ORDER_BY_KERRRY=2,
        PRODUCT_DETAIL_CHANGE=3,
       AUTOMATIC_PURCHASE_CHANGE=4
    }
    public static class LogActivityName
    {
        public static string ERROR = "Global Error, Unknown Error or Out Of Log_Type Range";
        public static string CHANGE_ORDER_CMS = "Thay đổi trạng thái đơn hàng";
        public static string LOGIN_CMS = "Đăng nhập vào hệ thống";
        public static string CHANGE_ORDER_BY_KERRRY = "Thay đổi trạng thái đơn từ Kerry";
        public static string PRODUCT_DETAIL_CHANGE = "Thay đổi thông tin sản phẩm";
        public static string AUTOMATIC_PURCHASE_CHANGE = "Thay đổi thông tin mua tự động";

    }
    public static class LogActivityKeyWord
    {
        public static string ERROR = "Global, Unknown Error or Testing.";
        public static string CHANGE_ORDER_CMS = "orders,change";
        public static string LOGIN_CMS = "login,cms";
        public static string CHANGE_ORDER_BY_KERRRY = "order,change,status,kerry";
        public static string PRODUCT_DETAIL_CHANGE = "product,change,detail";
        public static string AUTOMATIC_PURCHASE_CHANGE = "automatic,bot,purchase";

    }
    public static class LogActivityBSONDocuments
    {
        public static string CMS = "UsersLogActivity";
        public static string FE_Common = "UsersLogActivityFE";
        public static string API = "UsersLogActivityAPI";

    }

}
