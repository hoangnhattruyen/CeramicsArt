
using CeramicsArt.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CeramicsArt.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminKhachHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminKhachHangController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // DANH SÁCH KHÁCH HÀNG
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var khachHangs = await _context.NguoiDungs
                .AsNoTracking()
                .Where(x => x.VaiTro == "KhachHang")
                .OrderByDescending(x => x.NgayTao)
                .Select(x => new
                    CeramicsArt.Models.ViewModels.KhachHangAdminViewModel
                {
                    Id = x.Id,
                    HoTen = x.HoTen,
                    Email = x.Email,
                    VaiTro = x.VaiTro,
                    NgayTao = x.NgayTao
                })
                .ToListAsync();

            return View(khachHangs);
        }
    }
}
