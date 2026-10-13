using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Models;

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
        public IActionResult Index()
        {
            return View();
        }
    }
}
