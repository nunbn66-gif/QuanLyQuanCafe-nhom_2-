using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Models;
namespace QuanLyQuanCafeWeb.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly CafeDbContext _context;
        public TaiKhoanController(CafeDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string? tuKhoa, string? vaiTro)
        {
            var query = _context.Set<TaiKhoan>().AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(t => t.TenDangNhap.Contains(tuKhoa) || t.HoTen.Contains(tuKhoa));
            }
            if (!string.IsNullOrWhiteSpace(vaiTro)) query = query.Where(t => t.VaiTro == vaiTro);
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.VaiTro = vaiTro;
            ViewBag.VaiTroList = new SelectList(new[] { "KhachHang", "NhanVien", "ChuQuan" }, vaiTro);
            var dsTaiKhoan = await query.OrderBy(t => t.HoTen).ToListAsync();
            return View(dsTaiKhoan);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new TaiKhoan { VaiTro = "KhachHang", TrangThai = true, DiemTichLuy = 0 });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDangNhap,HoTen,VaiTro,TrangThai")] TaiKhoan tk, string? matKhau, string? xacNhanMatKhau)
        {
            if (!string.IsNullOrWhiteSpace(tk.TenDangNhap)) tk.TenDangNhap = tk.TenDangNhap.Trim();
            if (!string.IsNullOrWhiteSpace(tk.HoTen)) tk.HoTen = tk.HoTen.Trim();
            if (string.IsNullOrWhiteSpace(tk.TenDangNhap)) ModelState.AddModelError(nameof(tk.TenDangNhap), "Tên đăng nhập ko đc để trống .");
            if (string.IsNullOrWhiteSpace(tk.HoTen)) ModelState.AddModelError(nameof(tk.HoTen), "Họ tên ko đc để trống .");
            if (!string.IsNullOrWhiteSpace(tk.TenDangNhap) && await _context.Set<TaiKhoan>().AnyAsync(t => t.TenDangNhap == tk.TenDangNhap)) ModelState.AddModelError(nameof(tk.TenDangNhap), "Tên đăng nhập đã tồn tại");
            if (string.IsNullOrWhiteSpace(matKhau) || matKhau.Length < 8) ModelState.AddModelError("matKhau", "Mật khẩu phải có ít nhất 8 kí tự .");
            if (matKhau != xacNhanMatKhau) ModelState.AddModelError("xacNhanMatKhau", "Mật khẩu xác nhận ko khớp");
            string[] vaiTroHopLe = { "KhachHang", "NhanVien", "ChuQuan" };
            if (!vaiTroHopLe.Contains(tk.VaiTro)) ModelState.AddModelError(nameof(tk.VaiTro), "Vai trò không hợp lệ ");
            ModelState.Remove(nameof(tk.MatKhauHash));
            ModelState.Remove("Orders");
            if(ModelState.IsValid)
            {
                tk.DiemTichLuy = 0;
                tk.MatKhauHash = new PasswordHasher<TaiKhoan>().HashPassword(tk, matKhau!);
                _context.Set<TaiKhoan>().Add(tk);
                await _context.SaveChangesAsync();
                TempData["Success"] = "THêm tài khoản thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(tk);

        }
    }
}
