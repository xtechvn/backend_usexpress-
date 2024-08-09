using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Contants
{
   public class VoucherRuleType
    {
        /// <summary>
        /// rule này sẽ giảm giá trực tiếp trên toàn bộ đơn hàng theo nhãn. 
        /// Nó sẽ giới hạn số lần sử dụng Voucher trên đơn hàng của 1 User bất kể đơn hàng của user đó đã thanh toán hay chưa được thanh toán        
        /// </summary>
        public const int GIAM_GIA_TREN_DON_HANG = 1;

        /// <summary>
        /// rule này sẽ giảm giá trực tiếp trên toàn bộ đơn hàng theo nhãn. 
        /// Nó sẽ áp dụng cho 1 User là mới khi mà User đó chưa phát sinh doanh thu lần nào trên hệ thống. Dựa vào đếm những Record có paymentStatus = 1
        /// Nếu đếm paymentStatus = 0 của user trên đơn hàng. Trả ra 0 nghĩa là User đó là mới
        /// </summary>
        public const int GIAM_GIA_THEO_USER_MOI = 2;

        /// <summary>
        /// RULE NÀY SẼ ÁP DỤNG ĐỂ TEST NÊN CHỈ THỰC HIỆN TRÊN NHÓM NHỮNG USER ĐƯỢC SET
        /// rule này sẽ giảm giá trực tiếp trên toàn bộ đơn hàng theo nhãn. Không quan trọng là user đó là mới hay cũ
        /// Nó sẽ dựa vào trường limit của bảng Voucher trong database để check giới hạn số lần sử dụng của voucher cho 1 user
        /// NÓ sẽ chỉ áp dụng với trường hợp phải có  User được fix.
        /// 1 User có thể sử dụng nhiều lần k vượt quá số lần LIMIT được set trong db        /// 
        /// </summary>
        public const int GIAM_GIA_GIOI_HAN_SO_LAN_SU_DUNG_THEO_VOUCHER = 3;

        /// <summary>
        /// RULE NÀY ÁP DỤNG CHO KHÁCH HÀNG CŨ ĐÃ MUA HÀNG VÀ KHÔNG DÙNG ĐƯỢC CHO TÀI KHOẢN KHÁC
        /// Em tạo 1 mã duy nhất: USTHANKYOU.Khi có người(gọi là A) nhập mã USTHANKYOU em kiểm tra:
        /// If(A đã dùng mã, FALSE & Thông báo Bạn đã dùng mã này,
        /// else IF (A chưa từng hoàn thành đơn hàng nào trước 30/11/2018, FALSE & thông báo Mã không có hiệu lực,
        /// else IF (Hôm nay - Max (Ngày hoàn thành đơn hàng gần nhất trước 30/11/2018, Ngày rule bắt đầu chạy live) > 30days, FALSE & thông báo Mã không hiệu lực do đã hết thời hạn sử dụng mã,
        /// else TRUE(Mã Voucher giảm giá 86k, Update vào db A đã dùng mã USTHANKYOU)
        /// </summary>
        public const int GIAM_GIA_KHACH_HANG_CU_KHONG_AP_DUNG_TK_KHAC = 4;

        /// <summary>
        /// Mục đích: Trong chương trình Mời bạn bè đi du lịch, bạn bè sau khi dùng mã THAGA sẽ được cấp 1 mã BATDAUxxxx để đi mời bạn bè họ sử dụng.
        /// Yêu cầu:
        /// Mã: BATDAUxxxxx
        /// Trong đó xxxxx là 5 chữ số mã khách hàng(User ID), không cần tạo mã sẵn nên ko cần care cái này
        /// Nội dung: Giảm 77,000 đ cho khách hàng chưa từng mua hàng, Thời gian từ giờ đến 31/12/2018.
        /// Thuật toán gợi ý:
        /// Giống như THAGA: Kiểm tra đơn hàng đầu tiên, Kiểm tra thời hạn 31/12, Kiểm tra chưa có đơn hàng được tạo nào dùng mã BATDAU
        /// Nhưng có 1 số điểm khác: THAGA tạo 25 mã, nhưng cái này không cần tạo mã, mà chỉ cần kiểm tra bằng lệnh IF(6 kí tự đầu của mã khách nhập = BATDAU, tiếp theo là 4 chữ số)
        /// </summary>
        public const int GIAM_GIA_BATDAUXXX = 5;

        //Mã voucher:Gồm 5000 mã voucher, chia ra 100 bộ voucher V1, V2,..., V100, mỗi bộ gồm 50 voucher.Các voucher trong mỗi bộ có mức giảm như nhau, với mức giảm 100 bộ là 1%, 2 %,..., 100%
        //Điều kiện áp dụng:
        //Áp dụng cho mọi user(mới và cũ), mọi đơn hàng.
        //Mỗi mã chỉ áp dụng 1 lần(nghĩa là chỉ cần 1 user dùng thì mã hết hiệu lực luôn).
        //Áp dụng từ ngày 23/11-30/11/2018

        //update lai rule 20-11
        // Đơn hàng 1 sản phẩm giá 230$, phí là 18$, được giảm 50% (x=50) nên được giảm 9$, phí còn 9$
        //Đơn hàng có 2 sản phẩm: A giá 50$ - phí là 15$, B giá 100$ - phí là 18$. Được giảm voucher 50% phí của sản phẩm giá cao(tức giảm 50%x18$ = 9$), giảm 20% phí sản phẩm thứ 2 (tức 20%x15$ = 3$), tổng giảm là 12$
        //Đơn hàng có 2 sản phẩm: A giá 50$ - phí là 15$, B giá 200$ - phí là 28$ (gồm 18$ first pound và 5%=10$ Luxury). Được giảm voucher 50% phí của sản phẩm giá cao(tức giảm 50%x18$ = 9$), giảm 20% phí sản phẩm thứ 2 (tức 20%x15$ = 3$), tổng giảm là 12$.

        public const int GIAM_GIA_VOUCHER_TREN_PHI_MUA_HO = 6;

        /// Rule 7: Giảm 100% phí mua hộ
        public const int GIAM_TOAN_BO_PHI_MUA_HO = 7;

        public const int GIAM_GIA_TOI_DA_1_TRIEU = 8;

        // Rule 8: Giảm 8 USD cho đơn hàng costco, mỗi người chỉ dùng duy nhất 1 lần

        public const int MA_COSTCO = 9;
        public const int ACCESSTRADE = 10;
        public const int COSTCO_2PRODUCTS = 11;
        public const int VC04 = 12;
        public const int VC05 = 13;
        public const int HLNRNOIBO = 14;
        public const int DISCOUNTFORSTORE = 15;


        //Code: NORDSTROM66, HAUTELOOK66, COSTCO66, AMAZON66
        //Đối tượng áp dụng:
        //Mọi đối tượng có code
        //Giá trị Voucher:
        //Giảm 66k cho đơn hàng >= 1 triệu      
        //Điều kiện khác:
        //Không giới hạn số lần sử dụng
        //    Không có hạn sử dụng
        //    Không áp dụng đồng thời với code khác
        //    Mỗi mã tương ứng với đúng store ví dụ như Amazon là AMAZON66
        public const int GIAM_GIA_TREN_DON_HANG_THEO_STORE = 16;

        // Rule 17: Rule này được tạo ra vì để tránh ảnh hưởng đến rule 6
        // Rule này khi áp dụng sẽ free phí shipping tính trên đơn hàng đang áp dụng cho HR,NL,VS,JM
        // KhoaNguyen - 16/08/2019
        public const int GIAM_GIA_VOUCHER_TREN_PHI_MUA_HO_VA_PHI_SHIPPING_ORDER = 17;

        public const int AMZ_DISCOUNT_FPF = 18; // rule giảm của amz khi mua từ  2 sp trở lên
        public const int JOMA_DISCOUNT = 19; // rule giảm của JOMA khi mua từ  2 sp trở lên

        //Link requirement: https://usexpressvn-my.sharepoint.com/:x:/g/personal/hanhnguyen_usexpress_vn/EUM3XSVw9ZVBjwnA4ILHCOkBROdIQ3vIREp89dQ75vuchg?rtime=8g5NER6V10g
    }
}
