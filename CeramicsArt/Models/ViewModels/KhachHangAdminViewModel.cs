
using System;

namespace CeramicsArt.Models.ViewModels
{
    public class KhachHangAdminViewModel
    {
        // Mã khách hàng
        public long Id { get; set; }

        // Họ và tên khách hàng
        public string HoTen { get; set; } = "";

        // Email đăng ký
        public string Email { get; set; } = "";

        // Vai trò tài khoản
        public string VaiTro { get; set; } = "";

        // Ngày đăng ký
        public DateTime NgayTao { get; set; }
    }
}
