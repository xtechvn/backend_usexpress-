using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Utilities;

namespace Entities.ViewModels
{
  public  class ClientChangePasswordViewModel
    {
        [Key]
        public long ClientId { get; set; }

        [Required(ErrorMessage = "Bạn phải nhập Email")]
        [RegularExpression(PresentationUtils.EmailPattern, ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email không được dài quá 100 ký tự")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Bạn phải nhập mật khẩu mới")]
        [RegularExpression("[\\S]{6,100}", ErrorMessage = "Mật khẩu mới phải từ 6 ký tự trở lên")]
        [DataType(DataType.Password)]
        public string PasswordNew { get; set; }


        [Compare("PasswordNew", ErrorMessage = "Nhập lại mật khẩu phải trùng khớp với mật khẩu mới")]
        [DataType(DataType.Password)]
        public string ConfirmPasswordNew { get; set; }
    }
}
