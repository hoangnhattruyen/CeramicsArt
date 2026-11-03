
using System.ComponentModel.DataAnnotations;

namespace CeramicsArt.Models
{
    public class ThemSanPhamViewModel
    {
        // Tên sản phẩm
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(200, ErrorMessage = "Tên tối đa 200 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string TenSanPham { get; set; } = "";

        // Mô tả
        [Display(Name = "Mô tả sản phẩm")]
        public string? MoTa { get; set; }

        // Giá bán
        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Range(typeof(decimal), "1", "1000000000",
            ErrorMessage = "Giá phải từ 1 đến 1 tỷ đồng")]
        [Display(Name = "Giá bán")]
        public decimal Gia { get; set; }

        // Số lượng trong kho
        [Range(0, int.MaxValue,
            ErrorMessage = "Số lượng không được âm")]
        [Display(Name = "Số lượng tồn kho")]
        public int SoLuong { get; set; }

        // Hình ảnh
        [Url(ErrorMessage = "Đường dẫn hình ảnh không hợp lệ")]
        [Display(Name = "Hình ảnh")]
        public string? HinhAnh { get; set; }

        // Danh mục sản phẩm
        [Display(Name = "Danh mục")]
        public long? DanhMucId { get; set; }
    }
}
