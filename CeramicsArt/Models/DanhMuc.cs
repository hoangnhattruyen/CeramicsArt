using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CeramicsArt.Models
{
    [Table("danh_muc")]
    public class DanhMuc
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("ten_danh_muc")]
        public string TenDanhMuc { get; set; } = "";

        [Column("mo_ta")]
        public string? MoTa { get; set; }

        public List<SanPham> SanPhams { get; set; } = new();
    }
}