
using CeramicsArt.Data;
using CeramicsArt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CeramicsArt.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSanPhamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminSanPhamController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH SẢN PHẨM
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sanPhams = await _context.SanPhams
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(sanPhams);
        }

        // 2. HIỂN THỊ FORM THÊM SẢN PHẨM
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ThemSanPhamViewModel());
        }

        // 3. LƯU SẢN PHẨM MỚI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ThemSanPhamViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra danh mục có tồn tại không
            if (model.DanhMucId.HasValue)
            {
                bool danhMucTonTai =
                    await _context.DanhMucs.AnyAsync(
                        x => x.Id == model.DanhMucId.Value
                    );

                if (!danhMucTonTai)
                {
                    ModelState.AddModelError(
                        nameof(model.DanhMucId),
                        "Danh mục không tồn tại."
                    );

                    return View(model);
                }
            }

            // Tạo sản phẩm
            var sanPham = new SanPham
            {
                TenSanPham = model.TenSanPham.Trim(),
                MoTa = model.MoTa?.Trim(),
                Gia = model.Gia,
                SoLuong = model.SoLuong,
                HinhAnh = model.HinhAnh?.Trim(),
                DanhMucId = model.DanhMucId
            };

            // Lưu sản phẩm vào Supabase
            _context.SanPhams.Add(sanPham);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Thêm sản phẩm thành công!";

            return RedirectToAction(nameof(Index));
        }

        // 4. HIỂN THỊ FORM SỬA
        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var sanPham = await _context.SanPhams
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        // 5. CẬP NHẬT SẢN PHẨM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            long id,
            [Bind("Id,TenSanPham,MoTa,Gia,SoLuong,HinhAnh,DanhMucId")]
            SanPham model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            ModelState.Remove(nameof(SanPham.DanhMuc));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra danh mục
            if (model.DanhMucId.HasValue)
            {
                bool danhMucTonTai =
                    await _context.DanhMucs.AnyAsync(
                        x => x.Id == model.DanhMucId.Value
                    );

                if (!danhMucTonTai)
                {
                    ModelState.AddModelError(
                        nameof(model.DanhMucId),
                        "Danh mục không tồn tại."
                    );

                    return View(model);
                }
            }

            var sanPham = await _context.SanPhams
                .FirstOrDefaultAsync(p => p.Id == id);

            if (sanPham == null)
            {
                return NotFound();
            }

            sanPham.TenSanPham = model.TenSanPham.Trim();
            sanPham.MoTa = model.MoTa?.Trim();
            sanPham.Gia = model.Gia;
            sanPham.SoLuong = model.SoLuong;
            sanPham.HinhAnh = model.HinhAnh?.Trim();
            sanPham.DanhMucId = model.DanhMucId;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Cập nhật sản phẩm thành công!";

            return RedirectToAction(nameof(Index));
        }

        // 6. TRANG XÁC NHẬN XÓA
        [HttpGet]
        public async Task<IActionResult> Delete(long id)
        {
            var sanPham = await _context.SanPhams
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        // 7. XÓA SẢN PHẨM
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var sanPham = await _context.SanPhams
                .FirstOrDefaultAsync(p => p.Id == id);

            if (sanPham == null)
            {
                return NotFound();
            }

            _context.SanPhams.Remove(sanPham);

            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Xóa sản phẩm thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa sản phẩm vì có dữ liệu liên quan.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
