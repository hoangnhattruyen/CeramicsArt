
using System.Data;
using System.Security.Claims;
using CeramicsArt.Data;
using CeramicsArt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CeramicsArt.Controllers
{
    [Authorize]
    public class DatHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatHangController> _logger;

        public DatHangController(
            ApplicationDbContext context,
            ILogger<DatHangController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private long? LayNguoiDungId()
        {
            var claim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return long.TryParse(claim, out long id)
                && id > 0 ? id : null;
        }

        // HIỂN THỊ FORM ĐẶT HÀNG
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var nguoiDungId = LayNguoiDungId();

            if (nguoiDungId == null)
                return Forbid();

            var gioHang = await _context.GioHangs
                .AsNoTracking()
                .Include(g => g.ChiTietGioHangs)
                    .ThenInclude(ct => ct.SanPham)
                .FirstOrDefaultAsync(
                    g => g.NguoiDungId == nguoiDungId.Value);

            if (gioHang == null ||
                !gioHang.ChiTietGioHangs.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống.";

                return RedirectToAction(
                    "Index", "GioHang");
            }

            if (gioHang.ChiTietGioHangs.Any(x =>
                x.SanPham == null ||
                x.SoLuong < 1 ||
                x.SoLuong > 100 ||
                x.SoLuong > x.SanPham.SoLuong))
            {
                TempData["Error"] =
                    "Một số sản phẩm không đủ hàng hoặc " +
                    "có số lượng không hợp lệ.";

                return RedirectToAction(
                    "Index", "GioHang");
            }

            var nguoiDung = await _context.NguoiDungs
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == nguoiDungId.Value);

            var model = new DatHangViewModel
            {
                HoTen = nguoiDung?.HoTen ?? "",
                Email = nguoiDung?.Email ?? ""
            };

            ViewBag.GioHang = gioHang;

            return View(model);
        }

        // XÁC NHẬN ĐẶT HÀNG COD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan(
            DatHangViewModel model)
        {
            var nguoiDungId = LayNguoiDungId();

            if (nguoiDungId == null)
                return Forbid();

            if (!ModelState.IsValid)
                return await HienThiLaiForm(model);

            try
            {
                // SỬA LỖI:
                // Lấy execution strategy từ EF Core.
                var strategy = _context.Database
                    .CreateExecutionStrategy();

                // Toàn bộ transaction phải nằm
                // bên trong ExecuteAsync.
                long donHangId = await strategy.ExecuteAsync(
                    async () =>
                    {
                        // Tránh giữ lại entity từ lần
                        // thử trước nếu có retry.
                        _context.ChangeTracker.Clear();

                        await using var transaction =
                            await _context.Database
                                .BeginTransactionAsync(
                                    IsolationLevel.Serializable);

                        var gioHang = await _context.GioHangs
                            .Include(g => g.ChiTietGioHangs)
                            .FirstOrDefaultAsync(
                                g => g.NguoiDungId
                                    == nguoiDungId.Value);

                        if (gioHang == null ||
                            !gioHang.ChiTietGioHangs.Any())
                        {
                            throw new CheckoutException(
                                "Giỏ hàng đang trống.");
                        }

                        var soLuongs = gioHang
                            .ChiTietGioHangs
                            .GroupBy(x => x.SanPhamId)
                            .ToDictionary(
                                x => x.Key,
                                x => x.Sum(i =>
                                    (long)i.SoLuong));

                        if (soLuongs.Values.Any(
                            x => x < 1 || x > 100))
                        {
                            throw new CheckoutException(
                                "Số lượng sản phẩm " +
                                "không hợp lệ.");
                        }

                        var sanPhamIds =
                            soLuongs.Keys.ToList();

                        var sanPhams = await _context.SanPhams
                            .AsNoTracking()
                            .Where(x =>
                                sanPhamIds.Contains(x.Id))
                            .ToListAsync();

                        if (sanPhams.Count !=
                            sanPhamIds.Count)
                        {
                            throw new CheckoutException(
                                "Một số sản phẩm " +
                                "không còn tồn tại.");
                        }

                        decimal tongTien = 0;

                        var danhSachChiTiet =
                            new List<ChiTietDonHang>();

                        foreach (var sanPham in sanPhams)
                        {
                            int soLuong =
                                (int)soLuongs[sanPham.Id];

                            if (sanPham.SoLuong < soLuong)
                            {
                                throw new CheckoutException(
                                    "Sản phẩm " +
                                    sanPham.TenSanPham +
                                    " không đủ tồn kho.");
                            }

                            // Trừ tồn kho bằng câu lệnh
                            // cập nhật có điều kiện.
                            int updated =
                                await _context.SanPhams
                                    .Where(x =>
                                        x.Id == sanPham.Id &&
                                        x.SoLuong >= soLuong)
                                    .ExecuteUpdateAsync(setters =>
                                        setters.SetProperty(
                                            x => x.SoLuong,
                                            x => x.SoLuong - soLuong));

                            if (updated != 1)
                            {
                                throw new CheckoutException(
                                    "Không đủ tồn kho cho " +
                                    sanPham.TenSanPham);
                            }

                            decimal thanhTien =
                                sanPham.Gia * soLuong;

                            tongTien += thanhTien;

                            danhSachChiTiet.Add(
                                new ChiTietDonHang
                                {
                                    SanPhamId = sanPham.Id,
                                    TenSanPham =
                                        sanPham.TenSanPham,
                                    Gia = sanPham.Gia,
                                    SoLuong = soLuong,
                                    ThanhTien = thanhTien
                                });
                        }

                        var donHang = new DonHang
                        {
                            NguoiDungId =
                                nguoiDungId.Value,
                            HoTen = model.HoTen.Trim(),
                            Email = model.Email.Trim(),
                            SoDienThoai =
                                model.SoDienThoai.Trim(),
                            DiaChi = model.DiaChi.Trim(),
                            TongTien = tongTien,
                            TrangThai = "Cho xac nhan",
                            NgayDat = DateTime.UtcNow,
                            ChiTietDonHangs =
                                danhSachChiTiet
                        };

                        _context.DonHangs.Add(donHang);

                        await _context.SaveChangesAsync();

                        // Xóa các sản phẩm đã mua
                        // sau khi tạo đơn.
                        _context.ChiTietGioHangs.RemoveRange(
                            gioHang.ChiTietGioHangs);

                        await _context.SaveChangesAsync();

                        // Commit toàn bộ thay đổi.
                        await transaction.CommitAsync();

                        return donHang.Id;
                    });

                TempData["Success"] =
                    "Đặt hàng thành công!";

                return RedirectToAction(
                    nameof(ThanhCong),
                    new { id = donHangId });
            }
            catch (CheckoutException ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction(
                    "Index", "GioHang");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Lỗi khi đặt hàng COD cho người dùng {UserId}",
                    nguoiDungId);

                TempData["Error"] =
                    "Không thể hoàn tất đơn hàng. " +
                    "Vui lòng thử lại sau.";

                return RedirectToAction(
                    "Index", "GioHang");
            }
        }

        // TRANG ĐẶT HÀNG THÀNH CÔNG
        [HttpGet]
        public async Task<IActionResult> ThanhCong(long id)
        {
            var nguoiDungId = LayNguoiDungId();

            if (nguoiDungId == null)
                return Forbid();

            var donHang = await _context.DonHangs
                .AsNoTracking()
                .Include(x => x.ChiTietDonHangs)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.NguoiDungId == nguoiDungId.Value);

            if (donHang == null)
                return NotFound();

            return View(donHang);
        }

        // HIỂN THỊ LẠI FORM KHI VALIDATION LỖI
        private async Task<IActionResult> HienThiLaiForm(
            DatHangViewModel model)
        {
            var nguoiDungId = LayNguoiDungId();

            if (nguoiDungId == null)
                return Forbid();

            var gioHang = await _context.GioHangs
                .AsNoTracking()
                .Include(g => g.ChiTietGioHangs)
                    .ThenInclude(x => x.SanPham)
                .FirstOrDefaultAsync(
                    x => x.NguoiDungId
                        == nguoiDungId.Value);

            if (gioHang == null ||
                !gioHang.ChiTietGioHangs.Any())
            {
                TempData["Error"] =
                    "Giỏ hàng đang trống.";

                return RedirectToAction(
                    "Index", "GioHang");
            }

            ViewBag.GioHang = gioHang;

            return View("Index", model);
        }

        // Lỗi nghiệp vụ đặt hàng
        private sealed class CheckoutException : Exception
        {
            public CheckoutException(string message)
                : base(message)
            {
            }
        }
    }
}
