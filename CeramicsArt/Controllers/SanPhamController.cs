
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CeramicsArt.Data;

namespace CeramicsArt.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SanPhamController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            var sanPhams = await _context.SanPhams
                .Include(p => p.DanhMuc)
                .ToListAsync();

            return View(sanPhams);
        }

        // Hiển thị chi tiết sản phẩm
        public async Task<IActionResult> Details(long id)
        {
            var sanPham = await _context.SanPhams
                .Include(p => p.DanhMuc)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }
    }
}
