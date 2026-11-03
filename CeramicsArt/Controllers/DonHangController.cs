
using System.Security.Claims;
using CeramicsArt.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CeramicsArt.Controllers
{
    [Authorize]
    public class DonHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonHangController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // DANH SÁCH ĐƠN HÀNG CỦA TÔI
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string? claimId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!long.TryParse(claimId, out long nguoiDungId)
                || nguoiDungId <= 0)
            {
                return Forbid();
            }

            var donHangs = await _context.DonHangs
                .AsNoTracking()
                .Where(x =>
                    x.NguoiDungId == nguoiDungId)
                .OrderByDescending(x => x.NgayDat)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            return View(donHangs);
        }

        // CHI TIẾT ĐƠN HÀNG CỦA TÔI
        [HttpGet]
        public async Task<IActionResult> Details(long id)
        {
            string? claimId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!long.TryParse(claimId, out long nguoiDungId)
                || nguoiDungId <= 0)
            {
                return Forbid();
            }

            var donHang = await _context.DonHangs
                .AsNoTracking()
                .Include(x => x.ChiTietDonHangs)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.NguoiDungId == nguoiDungId);

            if (donHang == null)
            {
                return NotFound();
            }

            // Tái sử dụng giao diện chi tiết đơn hàng
            // đã có từ bước đặt hàng thành công.
            return View(
                "~/Views/DatHang/ThanhCong.cshtml",
                donHang);
        }
    }
}
