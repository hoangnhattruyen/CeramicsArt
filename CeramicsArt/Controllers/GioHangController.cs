
using System.Security.Claims;
using CeramicsArt.Data;
using CeramicsArt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CeramicsArt.Controllers
{
    [Authorize]
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GioHangController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // Lấy ID từ tài khoản đang đăng nhập
        private long? LayNguoiDungId()
        {
            string? id = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (long.TryParse(id, out long nguoiDungId)
                && nguoiDungId > 0)
            {
                return nguoiDungId;
            }

            return null;
        }

        // 1. XEM GIỎ HÀNG
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            long? nguoiDungId = LayNguoiDungId();

            if (nguoiDungId == null)
            {
                return Forbid();
            }

            var gioHang = await _context.GioHangs
                .AsNoTracking()
                .Include(x => x.ChiTietGioHangs)
                    .ThenInclude(x => x.SanPham)
                .FirstOrDefaultAsync(
                    x => x.NguoiDungId == nguoiDungId.Value);

            // Tài khoản chưa có giỏ hàng
            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    NguoiDungId = nguoiDungId.Value
                };
            }

            return View(gioHang);
        }

        // 2. THÊM SẢN PHẨM VÀO GIỎ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(
            long sanPhamId,
            int soLuong = 1)
        {
            long? nguoiDungId = LayNguoiDungId();

            if (nguoiDungId == null)
            {
                return Forbid();
            }

            if (soLuong < 1 || soLuong > 100)
            {
                TempData["Error"] =
                    "Số lượng thêm phải từ 1 đến 100.";

                return RedirectToAction(
                    "Index", "Home");
            }

            var sanPham = await _context.SanPhams
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == sanPhamId);

            if (sanPham == null)
            {
                return NotFound();
            }

            if (sanPham.SoLuong < soLuong)
            {
                TempData["Error"] =
                    "Sản phẩm không đủ số lượng tồn kho.";

                return RedirectToAction(
                    "Index", "Home");
            }

            // Tìm giỏ hàng của người đang đăng nhập
            var gioHang = await _context.GioHangs
                .FirstOrDefaultAsync(
                    x => x.NguoiDungId == nguoiDungId.Value);

            // Nếu chưa có, tạo giỏ hàng
            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    NguoiDungId = nguoiDungId.Value,
                    NgayTao = DateTime.UtcNow
                };

                _context.GioHangs.Add(gioHang);

                await _context.SaveChangesAsync();
            }

            // Tìm sản phẩm trong giỏ
            var chiTiet = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(x =>
                    x.GioHangId == gioHang.Id &&
                    x.SanPhamId == sanPhamId);

            if (chiTiet == null)
            {
                // Chưa có sản phẩm: thêm dòng mới
                chiTiet = new ChiTietGioHang
                {
                    GioHangId = gioHang.Id,
                    SanPhamId = sanPhamId,
                    SoLuong = soLuong
                };

                _context.ChiTietGioHangs.Add(chiTiet);
            }
            else
            {
                // Đã có sản phẩm: cộng thêm số lượng
                long soLuongMoi =
                    (long)chiTiet.SoLuong + soLuong;

                if (soLuongMoi > 100 ||
                    soLuongMoi > sanPham.SoLuong)
                {
                    TempData["Error"] =
                        "Số lượng trong giỏ vượt quá tồn kho hoặc giới hạn 100.";

                    return RedirectToAction(
                        nameof(Index));
                }

                chiTiet.SoLuong = (int)soLuongMoi;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã thêm sản phẩm vào giỏ hàng!";

            return RedirectToAction(nameof(Index));
        }

        // 3. CẬP NHẬT SỐ LƯỢNG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhat(
            long chiTietId,
            int soLuong)
        {
            long? nguoiDungId = LayNguoiDungId();

            if (nguoiDungId == null)
            {
                return Forbid();
            }

            if (soLuong < 1 || soLuong > 100)
            {
                TempData["Error"] =
                    "Số lượng phải từ 1 đến 100.";

                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra chi tiết thuộc giỏ của người dùng
            var chiTiet = await _context.ChiTietGioHangs
                .Include(x => x.GioHang)
                .Include(x => x.SanPham)
                .FirstOrDefaultAsync(x =>
                    x.Id == chiTietId &&
                    x.GioHang.NguoiDungId == nguoiDungId.Value);

            if (chiTiet == null)
            {
                return NotFound();
            }

            if (chiTiet.SanPham == null)
            {
                TempData["Error"] =
                    "Sản phẩm không còn tồn tại.";

                return RedirectToAction(nameof(Index));
            }

            if (soLuong > chiTiet.SanPham.SoLuong)
            {
                TempData["Error"] =
                    "Số lượng yêu cầu vượt quá tồn kho.";

                return RedirectToAction(nameof(Index));
            }

            chiTiet.SoLuong = soLuong;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã cập nhật số lượng sản phẩm.";

            return RedirectToAction(nameof(Index));
        }

        // 4. XÓA SẢN PHẨM KHỎI GIỎ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(long chiTietId)
        {
            long? nguoiDungId = LayNguoiDungId();

            if (nguoiDungId == null)
            {
                return Forbid();
            }

            // Chỉ được xóa sản phẩm trong giỏ của mình
            var chiTiet = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(x =>
                    x.Id == chiTietId &&
                    x.GioHang.NguoiDungId == nguoiDungId.Value);

            if (chiTiet == null)
            {
                return NotFound();
            }

            _context.ChiTietGioHangs.Remove(chiTiet);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã xóa sản phẩm khỏi giỏ hàng.";

            return RedirectToAction(nameof(Index));
        }
    }
}
