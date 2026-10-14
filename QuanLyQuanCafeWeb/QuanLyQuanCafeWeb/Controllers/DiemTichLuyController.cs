
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Filters;
using QuanLyQuanCafeWeb.Models;
using QuanLyQuanCafeWeb.ViewModels;
namespace QuanLyQuanCafeWeb.Controllers
{
    [SessionAuthorize("KhachHang")]
    public class DiemTichLuyController : Controller
    {
        private readonly CafeDbContext _context;
        public DiemTichLuyController(CafeDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            int? maTk = HttpContext.Session.GetInt32("MaTK");
            if (!maTk.HasValue) return RedirectToAction("Login", "Auth");
            var tk = await _context.Set<TaiKhoan>().AsNoTracking().Where(t => t.MaTk == maTk.Value && t.VaiTro == "KhachHang").Select(t => new DiemTichLuyVM { HoTen = t.HoTen, TenDangNhap = t.TenDangNhap, DiemTichLuy = t.DiemTichLuy }).FirstOrDefaultAsync();
            if (tk == null) return NotFound();
            return View(tk);
        }
    }
}
