using Entities.ValidationAtribute;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Utilities;


namespace Entities.ViewModels
{
  public  class ClientLogOnViewModel
    {
        [Required(ErrorMessage = "Bạn phải nhập Email")]
        [RegularExpression(PresentationUtils.EmailPattern, ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email không được dài quá 100 ký tự")]        
        public string Email { get; set; }

        [DisplayName("Mật khẩu:")]
        [Required(ErrorMessage = "Bạn phải nhập mật khẩu")]
        [StringLength(64, ErrorMessage = "Mật khẩu không được dài quá 64 ký tự")]
        [RegularExpression("[\\S]{6,100}", ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        
        public string return_url { get; set; }

        //[DisplayName("Ghi nhớ mật khẩu")]
        [ScaffoldColumn(false)]
        public bool remember_me { get; set; }
    }
}
