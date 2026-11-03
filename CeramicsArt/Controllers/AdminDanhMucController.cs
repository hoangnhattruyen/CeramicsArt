
using CeramicsArt.Data;
using CeramicsArt.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CeramicsArt.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDanhMucController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDanhMucController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. DANH SÁCH DANH MỤC
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var danhMucs = await _context.DanhMucs
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .ToListAsync();

            return View(danhMucs);
        }

        // 2. FORM THÊM DANH MỤC
        [HttpGet]
        public IActionResult Create()
        {
            return View(new DanhMuc());
        }

        // 3. LƯU DANH MỤC MỚI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TenDanhMuc,MoTa")] DanhMuc model)
        {
            ModelState.Remove(nameof(DanhMuc.SanPhams));

            model.TenDanhMuc =
                model.TenDanhMuc?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(model.TenDanhMuc))
            {
                ModelState.AddModelError(
                    nameof(DanhMuc.TenDanhMuc),
                    "Vui lòng nhập tên danh mục.");
            }

            if (!ModelState.IsValid)
                return View(model);

            model.MoTa = model.MoTa?.Trim();

            _context.DanhMucs.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Thêm danh mục thành công!";

            return RedirectToAction(nameof(Index));
        }

        // 4. FORM SỬA DANH MỤC
        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var danhMuc = await _context.DanhMucs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (danhMuc == null)
                return NotFound();

            return View(danhMuc);
        }

        // 5. LƯU THAY ĐỔI DANH MỤC
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            long id,
            [Bind("Id,TenDanhMuc,MoTa")] DanhMuc model)
        {
            if (id != model.Id)
                return BadRequest();

            ModelState.Remove(nameof(DanhMuc.SanPhams));

            model.TenDanhMuc =
                model.TenDanhMuc?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(model.TenDanhMuc))
            {
                ModelState.AddModelError(
                    nameof(DanhMuc.TenDanhMuc),
                    "Vui lòng nhập tên danh mục.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var danhMuc = await _context.DanhMucs
                .FirstOrDefaultAsync(x => x.Id == id);

            if (danhMuc == null)
                return NotFound();

            danhMuc.TenDanhMuc = model.TenDanhMuc;
            danhMuc.MoTa = model.MoTa?.Trim();

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Cập nhật danh mục thành công!";

            return RedirectToAction(nameof(Index));
        }

        // 6. TRANG XÁC NHẬN XÓA
        [HttpGet]
        public async Task<IActionResult> Delete(long id)
        {
            var danhMuc = await _context.DanhMucs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (danhMuc == null)
                return NotFound();

            return View(danhMuc);
        }

        // 7. XÓA DANH MỤC
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var danhMuc = await _context.DanhMucs
                .FirstOrDefaultAsync(x => x.Id == id);

            if (danhMuc == null)
                return NotFound();

            // Không xóa danh mục đang có sản phẩm
            bool coSanPham = await _context.SanPhams
                .AnyAsync(x => x.DanhMucId == id);

            if (coSanPham)
            {
                TempData["Error"] =
                    "Không thể xóa danh mục đang có sản phẩm.";

                return RedirectToAction(nameof(Index));
            }

            _context.DanhMucs.Remove(danhMuc);

            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Xóa danh mục thành công!";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa danh mục vì có dữ liệu liên quan.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
