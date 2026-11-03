
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CeramicsArt.Models
{
    [Table("chi_tiet_gio_hang")]
    public class ChiTietGioHang
    {
        // Mã chi tiết giỏ hàng
        [Key]
        [Column("id")]
        public long Id { get; set; }

        // Mã giỏ hàng
        [Column("gio_hang_id")]
        public long GioHangId { get; set; }

        // Mã sản phẩm
        [Column("san_pham_id")]
        public long SanPhamId { get; set; }

        // Số lượng sản phẩm trong giỏ
        [Range(1, int.MaxValue,
            ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Column("so_luong")]
        public int SoLuong { get; set; } = 1;

        // Liên kết với giỏ hàng
        [ForeignKey(nameof(GioHangId))]
        public GioHang GioHang { get; set; } = null!;

        // Liên kết với sản phẩm
        [ForeignKey(nameof(SanPhamId))]
        public SanPham SanPham { get; set; } = null!;
    }
}
