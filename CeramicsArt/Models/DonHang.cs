
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CeramicsArt.Models
{
    [Table("don_hang")]
    public class DonHang
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Column("ho_ten")]
        public string HoTen { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Column("email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Column("so_dien_thoai")]
        public string SoDienThoai { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [Column("dia_chi")]
        public string DiaChi { get; set; } = "";

        [Column("tong_tien")]
        [Precision(18, 2)]
        public decimal TongTien { get; set; }

        [Column("trang_thai")]
        public string TrangThai { get; set; } = "Cho xac nhan";

        [Column("ngay_dat")]
        public DateTime NgayDat { get; set; } = DateTime.UtcNow;

        [Column("nguoi_dung_id")]
        public long? NguoiDungId { get; set; }

        // Danh sách sản phẩm trong đơn hàng
        public List<ChiTietDonHang> ChiTietDonHangs
        { get; set; } = new();
    }
}
