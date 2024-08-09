using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    /// <summary>
    ///Bảng check list giá phí theo Store của sản phẩm
    /// </summary>
   public class PriceLevelViewModel
    {
        public double Offset { get; set; } // han muc toi thieu
        public double Limit { get; set; } // han muc toi da
        public int FeeType { get; set; } // Loại phí của nhãn
        public string LabelId { get; set; } // danh sach cac loai nhan
        public double Price { get; set; } // Số tiền được giảm theo hạn mức
        public double Discount { get; set; } // Chiết khấu được giảm
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Note { get; set; }
    }
}
