
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CeramicsArt.Models
{
    [Table("san_pham")]
    public class SanPham
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(200, ErrorMessage = "Tên tối đa 200 ký tự")]
        [Column("ten_san_pham")]
        public string TenSanPham { get; set; } = "";

        [Column("mo_ta")]
        public string? MoTa { get; set; }

        [Range(typeof(decimal), "1", "1000000000",
            ErrorMessage = "Giá phải lớn hơn 0")]
        [Column("gia")]
        public decimal Gia { get; set; }

        [Range(0, int.MaxValue,
            ErrorMessage = "Số lượng không được âm")]
        [Column("so_luong")]
        public int SoLuong { get; set; }

        [Column("hinh_anh")]
        public string? HinhAnh { get; set; }

        [Column("danh_muc_id")]
        public long? DanhMucId { get; set; }

        [ForeignKey(nameof(DanhMucId))]
        public DanhMuc? DanhMuc { get; set; }
    }
}
