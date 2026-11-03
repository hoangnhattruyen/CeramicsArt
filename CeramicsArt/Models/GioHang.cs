
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CeramicsArt.Models
{
    [Table("gio_hang")]
    public class GioHang
    {
        // Mã giỏ hàng
        [Key]
        [Column("id")]
        public long Id { get; set; }

        // Mã người dùng sở hữu giỏ hàng
        [Required]
        [Column("nguoi_dung_id")]
        public long NguoiDungId { get; set; }

        // Ngày tạo giỏ hàng (UTC)
        [Column("ngay_tao")]
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        // Danh sách sản phẩm trong giỏ hàng
        public List<ChiTietGioHang> ChiTietGioHangs
        { get; set; } = new();
    }
}
