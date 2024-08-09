using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WEB.UI.Common
{
    public class Lib
    {
        // BIND RA NGÀY GIỜ BOOKING
        public static IEnumerable<SelectListItem> BindNumberToDropDownlist(int FromNumb, int ToNumb, string Prefex)
        {
            // Nạp vào đối tượng List
            var List = new List<SelectListItem>();
            for (int i = FromNumb; i <= ToNumb - 1; i++)
            {
                var item = new SelectListItem() { Text = i.ToString() + Prefex, Value = i.ToString() };
                List.Add(item);
            }
            return List;
        }
        public static string CorrectAddressModel(AddressReceiverOrderViewModel model)
        {
            try
            {
                var phone_list = new List<string>() { "086", "096", "097", "098", "091", "094", "089", "090", "099", "093", "092", "088", "032", "033", "034", "035", "036", "037", "038", "039", "070", "079", "077", "076", "078", "083", "084", "085", "081", "082", "056", "058", "059" };
                for (int i = 20; i < 30; i++)
                {
                    phone_list.Add("0" + i.ToString());
                }
                bool have_special_char = Regex.Replace(model.Phone, @"[\d]", "") == string.Empty ? false : true;
                if (model.Phone.Length ==10 && !have_special_char)
                {
                    var phone_network_prefix = model.Phone.Trim().Substring(0, 3);
                    var match = phone_list.IndexOf(phone_network_prefix);
                    if (match == -1)
                    {
                        return "Số điện thoại đã nhập không thuộc về bất kỳ nhà mạng nào tại Việt Nam, vui lòng thử lại.";
                    }
                }
                else
                {
                    return "Số điện thoại nhập vào không chính xác (Số điện thoại phải là số di động và chỉ chứa ký tự số)";
                }
                have_special_char = Regex.Replace(model.Address, @"[0-9aAàÀảẢãÃáÁạẠăĂằẰẳẲẵẴắẮặẶâÂầẦẩẨẫẪấẤậẬbBcCdDđĐeEèÈẻẺẽẼéÉẹẸêÊềỀểỂễỄếẾệỆfFgGhHiIìÌỉỈĩĨíÍịỊjJkKlLmMnNoOòÒỏỎõÕóÓọỌôÔồỒổỔỗỖốỐộỘơƠờỜởỞỡỠớỚợỢpPqQrRsStTuUùÙủỦũŨúÚụỤưƯừỪửỬữỮứỨựỰvVwWxXyYỳỲỷỶỹỸýÝỵỴzZ \\\/.,()-]", "") == string.Empty ? false : true;
                if (!have_special_char)
                {
                    return null;
                }
                else
                {
                    return "Địa chỉ nhập vào có chứa ký tự không hợp lệ (Vui lòng chỉ nhập chữ, số, các dấu ',.-\\/' và khoảng trắng)";
                }
            }
            catch (Exception)
            {
                return "Error On Excution";
            }
        }
    }
}
