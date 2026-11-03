
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CeramicsArt.Models
{
    [Table("chi_tiet_don_hang")]
    public class ChiTietDonHang
    {
        // Mã chi tiết đơn hàng
        [Key]
        [Column("id")]
        public long Id { get; set; }

        // Mã đơn hàng
        [Required]
        [Column("don_hang_id")]
        public long DonHangId { get; set; }

        // Mã sản phẩm (có thể null)
        [Column("san_pham_id")]
        public long? SanPhamId { get; set; }

        // Tên sản phẩm tại thời điểm đặt hàng
        [Required(ErrorMessage = "Tên sản phẩm không được trống")]
        [Column("ten_san_pham")]
        public string TenSanPham { get; set; } = "";

        // Đơn giá tại thời điểm đặt hàng
        [Column("gia")]
        [Range(typeof(decimal), "0", "1000000000000",
            ErrorMessage = "Giá không được âm")]
        public decimal Gia { get; set; }

        // Số lượng đặt
        [Column("so_luong")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }

        // Thành tiền = Đơn giá × Số lượng
        [Column("thanh_tien")]
        public decimal ThanhTien { get; set; }

        // Liên kết với đơn hàng
        [ForeignKey(nameof(DonHangId))]
        public DonHang DonHang { get; set; } = null!;

        // Liên kết với sản phẩm (có thể null)
        [ForeignKey(nameof(SanPhamId))]
        public SanPham? SanPham { get; set; }
    }
}
