using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Utilities;
namespace WEB.UI.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Bạn phải nhập Email")]
        [RegularExpression(PresentationUtils.EmailPattern, ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email không được dài quá 100 ký tự")]        
        public string EmailFogot { get; set; }
        public long ClientId { get; set; }
    }
}
