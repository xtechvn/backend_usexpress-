using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    public class IndustrySpecialViewModel
    {
        public int Id { get; set; } // Nganh hang dac biet
        public int SpecialType { get; set; } // Nganh hang dac biet
        public string SpecialName { get; set; } // tên ngành đặc biệt. Quy uoc trong all code
        public double Price { get; set; } // Giá phụ trội cho ngành hàng đặc biệt
        public int Status { get; set; }
        public bool IsAllowDiscountCompare { get; set; } // true: cho phép so sánh với chiết khấu của phí luxury. Trong 2 param % luxury của sp với Giá phụ trội ngành hàng dặc biệt cái nào lớn sẽ lấy
        public string GroupLabelId { get; set; } // Nhóm sản phẩm được so sánh phí nổi trội với chiết khấu sản phẩm
    }
}
