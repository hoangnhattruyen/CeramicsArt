
using System.ComponentModel.DataAnnotations;

namespace CeramicsArt.Models
{
    public class DatHangViewModel
    {
        // Họ tên người nhận hàng
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100,
            ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = "";

        // Email liên hệ
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        // Số điện thoại nhận hàng
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(
            @"^0[0-9]{9,10}$",
            ErrorMessage = "Số điện thoại phải có 10 hoặc 11 chữ số và bắt đầu bằng 0")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = "";

        // Địa chỉ nhận hàng
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(500,
            ErrorMessage = "Địa chỉ tối đa 500 ký tự")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string DiaChi { get; set; } = "";
    }
}
