
using System.Data;
using CeramicsArt.Data;
using CeramicsArt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CeramicsArt.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDonHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminDonHangController> _logger;

        private static readonly string[] TrangThaiHopLe =
        {
            "Cho xac nhan",
            "Da xac nhan",
            "Dang giao",
            "Hoan thanh",
            "Da huy"
        };

        public AdminDonHangController(
            ApplicationDbContext context,
            ILogger<AdminDonHangController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 1. DANH SÁCH ĐƠN HÀNG
        [HttpGet]
        public async Task<IActionResult> Index(
            string? trangThai)
        {
            var query = _context.DonHangs
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                if (!TrangThaiHopLe.Contains(trangThai))
                {
                    return BadRequest(
                        "Trạng thái không hợp lệ.");
                }

                query = query.Where(
                    x => x.TrangThai == trangThai);
            }

            var donHangs = await query
                .OrderByDescending(x => x.NgayDat)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            ViewBag.TrangThaiDangChon = trangThai;
            ViewBag.DanhSachTrangThai = TrangThaiHopLe;

            return View(donHangs);
        }

        // 2. XEM CHI TIẾT ĐƠN HÀNG
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            var donHang = await _context.DonHangs
                .AsNoTracking()
                .Include(x => x.ChiTietDonHangs)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (donHang == null)
                return NotFound();

            ViewBag.DanhSachTrangThai = TrangThaiHopLe;

            return View(donHang);
        }

        // 3. CẬP NHẬT TRẠNG THÁI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(
            long id,
            string trangThai)
        {
            if (id <= 0 ||
                string.IsNullOrWhiteSpace(trangThai) ||
                !TrangThaiHopLe.Contains(trangThai))
            {
                return BadRequest("Dữ liệu không hợp lệ.");
            }

            try
            {
                // Tương thích EnableRetryOnFailure
                // đã cấu hình trong Program.cs.
                var strategy = _context.Database
                    .CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    _context.ChangeTracker.Clear();

                    await using var transaction =
                        await _context.Database
                            .BeginTransactionAsync(
                                IsolationLevel.Serializable);

                    var donHang = await _context.DonHangs
                        .AsNoTracking()
                        .Include(x => x.ChiTietDonHangs)
                        .FirstOrDefaultAsync(x => x.Id == id);

                    if (donHang == null)
                    {
                        throw new InvalidOperationException(
                            "Không tìm thấy đơn hàng.");
                    }

                    // Kiểm tra trạng thái chuyển đổi
                    bool chuyenHopLe =
                        (donHang.TrangThai == "Cho xac nhan" &&
                            (trangThai == "Da xac nhan" ||
                             trangThai == "Da huy")) ||

                        (donHang.TrangThai == "Da xac nhan" &&
                            (trangThai == "Dang giao" ||
                             trangThai == "Da huy")) ||

                        (donHang.TrangThai == "Dang giao" &&
                            trangThai == "Hoan thanh");

                    if (!chuyenHopLe)
                    {
                        throw new InvalidOperationException(
                            "Không thể chuyển sang trạng thái này.");
                    }

                    // Chỉ thay đổi trạng thái nếu trạng thái
                    // trong database vẫn đúng như vừa đọc.
                    int updated = await _context.DonHangs
                        .Where(x =>
                            x.Id == id &&
                            x.TrangThai == donHang.TrangThai)
                        .ExecuteUpdateAsync(setters =>
                            setters.SetProperty(
                                x => x.TrangThai,
                                trangThai));

                    if (updated != 1)
                    {
                        throw new InvalidOperationException(
                            "Đơn hàng vừa được cập nhật bởi người khác.");
                    }

                    // HỦY ĐƠN: HOÀN LẠI TỒN KHO
                    if (trangThai == "Da huy")
                    {
                        // Gộp số lượng theo mã sản phẩm.
                        var soLuongs = donHang.ChiTietDonHangs
                            .Where(x => x.SanPhamId.HasValue)
                            .GroupBy(x => x.SanPhamId!.Value)
                            .Select(x => new
                            {
                                SanPhamId = x.Key,
                                SoLuong = x.Sum(i =>
                                    (long)i.SoLuong)
                            })
                            .ToList();

                        if (donHang.ChiTietDonHangs.Any(
                            x => !x.SanPhamId.HasValue))
                        {
                            throw new InvalidOperationException(
                                "Đơn hàng có sản phẩm không còn liên kết. " +
                                "Cần kiểm tra tồn kho trước khi hủy.");
                        }

                        foreach (var item in soLuongs)
                        {
                            if (item.SoLuong <= 0 ||
                                item.SoLuong > int.MaxValue)
                            {
                                throw new InvalidOperationException(
                                    "Số lượng hoàn kho không hợp lệ.");
                            }

                            int soLuong = (int)item.SoLuong;

                            int restored = await _context.SanPhams
                                .Where(x =>
                                    x.Id == item.SanPhamId &&
                                    x.SoLuong <= int.MaxValue - soLuong)
                                .ExecuteUpdateAsync(setters =>
                                    setters.SetProperty(
                                        x => x.SoLuong,
                                        x => x.SoLuong + soLuong));

                            if (restored != 1)
                            {
                                throw new InvalidOperationException(
                                    "Không thể hoàn kho sản phẩm #" +
                                    item.SanPhamId);
                            }
                        }
                    }

                    // Trạng thái và tồn kho cùng được lưu.
                    await transaction.CommitAsync();
                });

                TempData["Success"] =
                    trangThai == "Da huy"
                    ? "Đã hủy đơn hàng và hoàn lại tồn kho."
                    : "Cập nhật trạng thái thành công!";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Lỗi cập nhật trạng thái đơn hàng {OrderId}",
                    id);

                TempData["Error"] =
                    "Không thể cập nhật đơn hàng. " +
                    "Vui lòng kiểm tra lại.";
            }

            return RedirectToAction(
                nameof(Details),
                new { id });
        }
    }
}
