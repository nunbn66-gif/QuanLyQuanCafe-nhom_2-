using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Models;
using Azure.Core;
using QuanLyQuanCafeWeb.Filters;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace QuanLyQuanCafeWeb.Controllers
{
    public class AuthController : Controller
    {
        private readonly CafeDbContext _context;
        private readonly PasswordHasher<TaiKhoan> _hasher = new();
        public AuthController(CafeDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.TenDangNhap = Request.Cookies["RememberTenDangNhap"];
            ViewBag.GhiNho = !string.IsNullOrWhiteSpace(Request.Cookies["RememberTenDangNhap"]);
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string? tenDangNhap, string? matKhau, bool ghiNhoTenDangNhap = false)
        {
            ViewBag.TenDangNhap = tenDangNhap;
            ViewBag.GhiNho = ghiNhoTenDangNhap;
            tenDangNhap = tenDangNhap?.Trim();
            if (string.IsNullOrWhiteSpace(tenDangNhap)) ModelState.AddModelError("", "Vui lòng nhập tên đăng nhập");
            if (string.IsNullOrWhiteSpace(matKhau)) ModelState.AddModelError("", "Vui lòng nhập mật khẩu");
            if (!ModelState.IsValid) return View();
            var tk = await _context.Set<TaiKhoan>().SingleOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);
            if (tk == null||!tk.TrangThai)
            {
                ModelState.AddModelError("", "THông tin đăng nhập ko đúng or tài khoản bị khóa");
                return View();
            }
            PasswordVerificationResult ketQua;
            try
            {
                ketQua = _hasher.VerifyHashedPassword(tk, tk.MatKhauHash, matKhau!);
            }
            catch (FormatException)
            {
                ModelState.AddModelError("", "Tài khoản chưa có mật khẩu hợp lệ. Hãy tạo tài khoản thử nghiệm");
                return View();
            }
            if(ketQua==PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Thông tin đăng nhập ko đúng hoặc tài khoản bị khóa");
                return View();
            }
            if (ketQua == PasswordVerificationResult.SuccessRehashNeeded)
            {
                tk.MatKhauHash = _hasher.HashPassword(tk, matKhau!);
                await _context.SaveChangesAsync();
            }
            HttpContext.Session.SetInt32("MaTK", tk.MaTk);
            HttpContext.Session.SetString("HoTen", tk.HoTen);
            HttpContext.Session.SetString("VaiTro", tk.VaiTro);
            if (ghiNhoTenDangNhap)
            {
                Response.Cookies.Append("RememberTenDangNhap", tk.TenDangNhap, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(30), HttpOnly = true, Secure = Request.IsHttps, SameSite = SameSiteMode.Lax, Path = "/" });
            }
            else
            {
                Response.Cookies.Delete("RememberTenDangNhap", new CookieOptions { HttpOnly = true, Secure = Request.IsHttps, SameSite = SameSiteMode.Lax, Path = "/" });
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login)); 
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("TenDangNhap,HoTen")] TaiKhoan tk, string? matKhau, string? xacNhanMatKhau)
        {
            if (!string.IsNullOrWhiteSpace(tk.TenDangNhap)) tk.TenDangNhap = tk.TenDangNhap.Trim();
            if (!string.IsNullOrWhiteSpace(tk.HoTen)) tk.HoTen = tk.HoTen.Trim();
            if (string.IsNullOrWhiteSpace(tk.TenDangNhap)) ModelState.AddModelError(nameof(tk.TenDangNhap), "Tên đăng nhập ko đc để trống .");
            if (string.IsNullOrWhiteSpace(tk.HoTen)) ModelState.AddModelError(nameof(tk.HoTen), "Họ tên không để trống");
            if (!string.IsNullOrWhiteSpace(tk.TenDangNhap) && await _context.Set<TaiKhoan>().AnyAsync(t => t.TenDangNhap == tk.TenDangNhap)) ModelState.AddModelError(nameof(tk.TenDangNhap), "Tên đăng nhập đã tồn tại");
            if (string.IsNullOrWhiteSpace(matKhau) || matKhau.Length < 8) ModelState.AddModelError("matKhau", "Mật khẩu pải có ít nhất 8 kí tự");
            if (matKhau != xacNhanMatKhau) ModelState.AddModelError("xacNhanMatkhau", "Mật khẩu xác nhận ko khớp");
            ModelState.Remove(nameof(tk.MatKhauHash));
            ModelState.Remove(nameof(tk.VaiTro));
            ModelState.Remove(nameof(tk.DiemTichLuy));
            ModelState.Remove(nameof(tk.TrangThai));
            ModelState.Remove(nameof(tk.TrangThai));
            ModelState.Remove("Orders");
            if (!ModelState.IsValid) return View(tk);
            tk.VaiTro = "KhachHang";
            tk.DiemTichLuy = 0;
            tk.TrangThai = true;
            tk.MatKhauHash = _hasher.HashPassword(tk, matKhau!);
            _context.Set<TaiKhoan>().Add(tk);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đăng ký thành công . Hãy đăng nhập bằng tài khoản bạn vừa tạo";
            return RedirectToAction(nameof(Login));
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
