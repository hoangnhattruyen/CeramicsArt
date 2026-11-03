
using CeramicsArt.Models;
using Microsoft.EntityFrameworkCore;

namespace CeramicsArt.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ==============================
        // CÁC BẢNG ĐÃ CÓ
        // ==============================

        // Danh mục
        public DbSet<DanhMuc> DanhMucs { get; set; }

        // Sản phẩm
        public DbSet<SanPham> SanPhams { get; set; }

        // Người dùng
        public DbSet<NguoiDung> NguoiDungs { get; set; }

        // Đơn hàng
        public DbSet<DonHang> DonHangs { get; set; }

        // Chi tiết đơn hàng
        public DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; }

        // ==============================
        // GIỎ HÀNG - BỔ SUNG MỚI
        // ==============================

        public DbSet<GioHang> GioHangs { get; set; }

        public DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==============================
            // NGƯỜI DÙNG
            // ==============================

            modelBuilder.Entity<NguoiDung>()
                .HasIndex(x => x.Email)
                .IsUnique();

            // ==============================
            // SẢN PHẨM
            // ==============================

            modelBuilder.Entity<SanPham>()
                .Property(x => x.Gia)
                .HasPrecision(18, 2);

            // ==============================
            // ĐƠN HÀNG
            // ==============================

            modelBuilder.Entity<DonHang>()
                .Property(x => x.TongTien)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ChiTietDonHang>()
                .Property(x => x.Gia)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ChiTietDonHang>()
                .Property(x => x.ThanhTien)
                .HasPrecision(18, 2);

            // Đơn hàng - Chi tiết đơn hàng
            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(x => x.DonHang)
                .WithMany(x => x.ChiTietDonHangs)
                .HasForeignKey(x => x.DonHangId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chi tiết đơn hàng - Sản phẩm
            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(x => x.SanPham)
                .WithMany()
                .HasForeignKey(x => x.SanPhamId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==============================
            // GIỎ HÀNG
            // ==============================

            // Một người dùng có thể có giỏ hàng.
            // Ánh xạ khóa ngoại, không tạo thuộc tính
            // điều hướng mới trong NguoiDung.
            modelBuilder.Entity<GioHang>()
                .HasOne<NguoiDung>()
                .WithMany()
                .HasForeignKey(x => x.NguoiDungId)
                .OnDelete(DeleteBehavior.Restrict);

            // Một giỏ hàng có nhiều chi tiết.
            modelBuilder.Entity<ChiTietGioHang>()
                .HasOne(x => x.GioHang)
                .WithMany(x => x.ChiTietGioHangs)
                .HasForeignKey(x => x.GioHangId)
                .OnDelete(DeleteBehavior.Cascade);

            // Một sản phẩm có thể xuất hiện
            // trong nhiều giỏ hàng.
            modelBuilder.Entity<ChiTietGioHang>()
                .HasOne(x => x.SanPham)
                .WithMany()
                .HasForeignKey(x => x.SanPhamId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
