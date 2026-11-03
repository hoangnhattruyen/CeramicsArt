
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using CeramicsArt.Data;
using CeramicsArt.Models;
using CeramicsArt.Models.ViewModels;

namespace CeramicsArt.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<NguoiDung> _hasher;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
            _hasher = new PasswordHasher<NguoiDung>();
        }

        // Đăng ký
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(DangKyViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string email = model.Email.Trim().ToLowerInvariant();

            bool existed = await _context.NguoiDungs
                .AnyAsync(u => u.Email.ToLower() == email);

            if (existed)
            {
                ModelState.AddModelError(
                    "Email",
                    "Email này đã được đăng ký."
                );

                return View(model);
            }

            var user = new NguoiDung
            {
                HoTen = model.HoTen.Trim(),
                Email = email,
                VaiTro = "KhachHang",
                NgayTao = DateTime.UtcNow
            };

            user.MatKhau = _hasher.HashPassword(
                user,
                model.MatKhau
            );

            _context.NguoiDungs.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đăng ký thành công! Hãy đăng nhập.";

            return RedirectToAction("Login");
        }

        // Đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(DangNhapViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string email = model.Email.Trim().ToLowerInvariant();

            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Email hoặc mật khẩu không chính xác."
                );

                return View(model);
            }

            var result = _hasher.VerifyHashedPassword(
                user,
                user.MatKhau,
                model.MatKhau
            );

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "",
                    "Email hoặc mật khẩu không chính xác."
                );

                return View(model);
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.MatKhau = _hasher.HashPassword(
                    user,
                    model.MatKhau
                );

                await _context.SaveChangesAsync();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.HoTen),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.VaiTro)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);


            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            // Nếu là Admin, chuyển vào trang quản trị
            if (string.Equals(
                user.VaiTro?.Trim(),
                "Admin",
                StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Admin");
            }

            // Nếu là khách hàng, chuyển về trang chủ
            return RedirectToAction("Index", "Home");

        }

        // Đăng xuất
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Index", "Home");
        }
    }
}
