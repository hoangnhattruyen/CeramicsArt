
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CeramicsArt.Models
{
    [Table("nguoi_dung")]
    public class NguoiDung
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("ho_ten")]
        public string HoTen { get; set; } = "";

        [Required]
        [Column("email")]
        public string Email { get; set; } = "";

        [Required]
        [Column("mat_khau")]
        public string MatKhau { get; set; } = "";

        [Column("vai_tro")]
        public string VaiTro { get; set; } = "KhachHang";

        [Column("ngay_tao")]
        public DateTime NgayTao { get; set; }
    }
}
