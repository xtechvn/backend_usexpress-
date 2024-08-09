using Entities.Models;
using Entities.ValidationAtribute;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Utilities;


namespace Entities.ViewModels
{
    //https://www.learnrazorpages.com/security/request-verification
    public class ClientViewModel
    {
        [Key]
        public long ClientId { get; set; }

        public long AddressId { get; set; }
        public long ClientMapID { get; set; } //truong de map client tu he thong cu
        public string SourceLoginId { get; set; } // id facebook or google
        public int SourceRegisterId { get; set; } // đăng ký từ nguồn nào
        [Required(ErrorMessage = "Bạn phải nhập Email")]
        [RegularExpression(PresentationUtils.EmailPattern, ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email không được dài quá 100 ký tự")]
        [Remote("checkEmailExist", "Client", ErrorMessage = "Email này đã được sử dụng", AdditionalFields = "ClientId", HttpMethod = "POST")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(PresentationUtils.MobilePattern, ErrorMessage = "Số điện thoại không đúng định dạng")]
        [StringLength(30, ErrorMessage = "Số điện thoại không được dài quá 30 ký tự")]
        [Remote("checkPhoneExist", "Client", ErrorMessage = "Số điện thoại này đã tồn tại trong hệ thống", AdditionalFields = "ClientId", HttpMethod = "POST")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Bạn phải nhập họ tên")]
        [StringLength(64, ErrorMessage = "Họ tên không được dài quá 64 ký tự")]
        public string ClientName { get; set; }

        [Required(ErrorMessage = "Bạn phải nhập mật khẩu")]
        [RegularExpression("[\\S]{6,100}", ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string PasswordBackup { get; set; }

        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận phải trùng khớp")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public int TotalOrder { get; set; }
        public int Gender { get; set; }
        public DateTime TokenCreatedDate { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime CreateDateIdentity { get; set; }
        public string ActiveToken { get; set; }
        public string ForgotPasswordToken { get; set; }
        public int Status { get; set; }
        public string Note { get; set; }
        public string Avartar { get; set; }
        public DateTime JoinDate { get; set; }
        public bool IsReceiverInfoEmail { get; set; }
        public string FullAddress { get; set; }
        public string CodeVerify { get; set; }

        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
        public string WardId { get; set; }

        public bool IsRegisterAffiliate { get; set; }
        public string ReferralId { get; set; }
    }
    public class AddressClientViewModel
    {
        public virtual long Id { get; set; }
        public long ClientId { get; set; }

        public virtual string ReceiverName { get; set; }
        public virtual string Phone { get; set; }
        public virtual string ProvinceId { get; set; }

        public virtual string DistrictId { get; set; }
        public virtual string WardId { get; set; }
        public virtual string Address { get; set; }
        public int? Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public long order_id { get; set; }
    }

    /// <summary>
    /// Lớp interface địa chỉ nhận hàng này sẽ kế thừa lớp danh sách địa chỉ của client mà không làm ảnh hưởng tới lớp cha của nó
    /// </summary>
    public class AddressReceiverOrderViewModel : AddressClientViewModel
    {


        [Required(ErrorMessage = "Bạn chưa nhập họ tên")]
        public override string ReceiverName { get; set; }
        [Required(ErrorMessage = "Bạn chưa nhập số điện thoại")]
        public override string Phone { get; set; }
        [Range(1, 100000, ErrorMessage = "Bạn chưa chọn tỉnh thành")]
        public override string ProvinceId { get; set; }
        public List<Province> ProvinceListReceiver { get; set; }

        [Range(1, 100000, ErrorMessage = "Bạn chưa chọn quận huyện")]
        public override string DistrictId { get; set; }
        public List<District> DistrictListReceiver { get; set; }

        [Range(1, 100000, ErrorMessage = "Bạn chưa chọn xã phường")]
        public override string WardId { get; set; }
        public List<Ward> WardListReceiver { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập địa chỉ")]
        public override string Address { get; set; }
        public string FullAddress { get; set; }
    }

    /// <summary>
    ///Mục đích sử dụng: Bổ sung thông tin tài khoản ngoài Frontend
    /// Lớp interface thông tin khách hàng này sẽ kế thừa lớp danh sách địa chỉ của client mà không làm ảnh hưởng tới lớp cha của nó
    /// </summary>
    public class ClientInfoViewModel : AddressClientViewModel
    {
        [Required(ErrorMessage = "Bạn chưa nhập họ tên")]
        public override string ReceiverName { get; set; }

        public override long Id { get; set; }//addressid
        public string Email { get; set; }
        public override string Phone { get; set; }

        [BindProperty]

        public int Gender { get; set; }
        public string[] Genders = new[] { "Nam", "Nữ" };

        [Range(1, 31, ErrorMessage = "Bạn chưa chọn ngày sinh")]
        public int BirthdayDay { get; set; }
        public IEnumerable<SelectListItem> BirthdayDayList { get; set; }

        [Range(1, 12, ErrorMessage = "Bạn chưa chọn tháng sinh")]
        public int BirthdayMonth { get; set; }
        public IEnumerable<SelectListItem> BirthdayMonthList { get; set; }

        [Range(1920, 9000, ErrorMessage = "Bạn chưa chọn năm sinh")]
        public int BirthdayYear { get; set; }
        public IEnumerable<SelectListItem> BirthdayYearList { get; set; }

        [Range(1, 100000, ErrorMessage = "Bạn chưa chọn tỉnh thành")]
        public override string ProvinceId { get; set; }
        public List<Province> ProvinceListReceiver { get; set; }

        [Range(1, 100000, ErrorMessage = "Bạn chưa chọn quận huyện")]
        public override string DistrictId { get; set; }
        public List<District> DistrictListReceiver { get; set; }

        [Range(1, 100000, ErrorMessage = "Bạn chưa chọn xã phường")]
        public override string WardId { get; set; }
        public List<Ward> WardListReceiver { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập địa chỉ")]
        public string FullAddress { get; set; }


        [DataType(DataType.Password)]
        [Remote("checkPasswordOldExist", "Client", ErrorMessage = "Mật khẩu cũ không đúng", AdditionalFields = "PasswordOld", HttpMethod = "POST")]
        public string PasswordOld { get; set; }

        [RegularExpression("[\\S]{6,100}", ErrorMessage = "Mật khẩu mới phải từ 6 ký tự trở lên")]
        [DataType(DataType.Password)]
        //[Remote("validationPassword", "Client", ErrorMessage = "Số điện thoại này đã tồn tại trong hệ thống", AdditionalFields = "ClientId", HttpMethod = "POST")]
        public string PasswordNew { get; set; }

        //[Remote("checkPhoneExist", "Client", ErrorMessage = "Số điện thoại này đã tồn tại trong hệ thống", AdditionalFields = "ClientId", HttpMethod = "POST")]
        [Compare("PasswordNew", ErrorMessage = "Nhập lại mật khẩu phải trùng khớp với mật khẩu mới")]
        [DataType(DataType.Password)]
        public string ConfirmPasswordNew { get; set; }

    }
    public class ClientSearchModel
    {
        public string ClientName { get; set; }
        public string ReferralId { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string OrderCode { get; set; }
        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
        public string WardId { get; set; }
        public string Address { get; set; }
        public double FromAmount { get; set; }
        public double ToAmount { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public DateTime FromJoinDate { get; set; }
        public DateTime ToJoinDate { get; set; }

        public int FromQuantity { get; set; }
        public int ToQuantity { get; set; }

        public int IsBackClientInDay { get; set; }
        public int IsPaymentInDay { get; set; }

        public string LableId { get; set; }
        public string GroupProductId { get; set; }
    }

    public class ClientListingModel
    {
        public long Id { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime JoinDate { get; set; }
        public string Address { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }
        public string Province { get; set; }
        public int TotalOrder { get; set; }
        public int TotalReferralOrder { get; set; }
        public string RefferalID { get; set; }
        public string UTMMedium { get; set; }
    }

    public class ClientDetailModel : Client
    {
        public string Phone { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string Address { get; set; }
    }

    public class ClientDetailViewModel
    {
        public ClientDetailModel Detail { get; set; }
        public List<AddressModel> AddressList { get; set; }
        public List<OrderGridModel> OrderList { get; set; }
        public List<OrderGridModel> ReferralOrderList { get; set; }
    }
}
