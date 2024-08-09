using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Utilities.Contants
{
    public class Constants
    {
        public const string Success = "SUCCESS";
        public const string Fail = "FAIL";
        public const string Error = "ERROR";
        public enum CampaignType
        {
            Campaign_Product = 1,
            Campaign_Voucher = 2,
        }
        public enum Status_Crawl
        {
            HOAN_THANH = 2,
            CHO_XU_LY = 0,
            THAT_BAI = 1
        }

        public enum AttachFileType
        {
            ORDER = 1,
            PRODUCT = 2
        }

        public enum NoteType
        {
            ORDER = 1,
            ORDER_ITEM = 2
        }

        public enum OrderStatus
        {
            [Display(Name = "Đơn hàng khởi tạo")]
            CREATED_ORDER = 3,

            [Display(Name = "Đơn hàng hoàn thành")]
            SUCCEED_ORDER = 4,

            [Display(Name = "Khách hàng hủy đơn hàng")]
            CANCEL_ORDER = 5,

            [Display(Name = "Đơn hàng đã được mua")]
            BOUGHT_ORDER = 6,

            [Display(Name = "Đơn hàng đang giao cho khách")]
            CLIENT_TRANSPORT_ORDER = 7,

            [Display(Name = "Hàng về kho ở Mỹ")]
            US_STORAGE_ORDER = 9,

            [Display(Name = "Hàng đang chuyển về VN")]
            VN_TRANSPORT_ORDER = 10,

            [Display(Name = "Hàng về kho Việt Nam")]
            VN_TRANSPORT_STORAGE_ORDER = 11,

            [Display(Name = "Đơn hàng đã mua thất bại")]
            BUY_FAILED_ORDER = 12,

            [Display(Name = "Đơn hàng đã thanh toán")]
            PAID_ORDER = 13,

            [Display(Name = "Lưu kho Việt Nam")]
            VN_STORAGE_ORDER = 16,
        }

        public enum Chart_Revenu_Type
        {
            Week = 1,
            Month = 2,
        }
        public enum Chart_Label_Type
        {
            Today = 1,
            Yesterday = 2,
            Week = 3,
            Month = 4,
        }
        public enum Chart_Type_Label
        {
            Revenu = 1,
            Quantity = 2,
        }
        public enum Product_Not_Found_Status
        {
            Product_Exists = 0,
            Product_Crawl_Failed = 1,
        }
    }
}
