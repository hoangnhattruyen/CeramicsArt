
using CeramicsArt.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CeramicsArt.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang tổng quan Admin
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Tổng số sản phẩm
            ViewBag.TongSanPham =
                await _context.SanPhams.CountAsync();

            // Tổng số tài khoản khách hàng
            ViewBag.TongKhachHang =
                await _context.NguoiDungs
                    .CountAsync(x => x.VaiTro == "KhachHang");

            // Tổng số lượng sản phẩm còn trong kho
            ViewBag.TongTonKho =
                await _context.SanPhams
                    .SumAsync(x => (long)x.SoLuong);

            return View();
        }
    }
}
