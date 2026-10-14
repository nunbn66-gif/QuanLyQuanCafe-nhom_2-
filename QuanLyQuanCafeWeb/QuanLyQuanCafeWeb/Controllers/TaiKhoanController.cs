using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Filters;
using QuanLyQuanCafeWeb.Models;
namespace QuanLyQuanCafeWeb.Controllers
{
    [SessionAuthorize("ChuQuan")]
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

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var tk = await _context.Set<TaiKhoan>().AsNoTracking().FirstOrDefaultAsync(t => t.MaTk == id);
            if (tk == null) return NotFound();
            return View(tk);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaTk,TenDangNhap,HoTen,VaiTro,DiemTichLuy,TrangThai")] TaiKhoan tk)
        {
            if (id != tk.MaTk) return NotFound();
            if (!string.IsNullOrWhiteSpace(tk.TenDangNhap)) tk.TenDangNhap = tk.TenDangNhap.Trim();
            if (!string.IsNullOrWhiteSpace(tk.HoTen)) tk.HoTen = tk.HoTen.Trim();
            if (string.IsNullOrWhiteSpace(tk.TenDangNhap)) ModelState.AddModelError(nameof(tk.TenDangNhap), "Tên đăng nhập không được để trống.");
            if (string.IsNullOrWhiteSpace(tk.HoTen)) ModelState.AddModelError(nameof(tk.HoTen), "Họ tên không được để trống.");
            if (!string.IsNullOrWhiteSpace(tk.TenDangNhap) && await _context.Set<TaiKhoan>().AnyAsync(t => t.TenDangNhap == tk.TenDangNhap && t.MaTk != id)) ModelState.AddModelError(nameof(tk.TenDangNhap), "Tên đăng nhập đã tồn tại.");
            string[] vaiTroHopLe = { "KhachHang", "NhanVien", "ChuQuan" };
            if (!vaiTroHopLe.Contains(tk.VaiTro)) ModelState.AddModelError(nameof(tk.VaiTro), "Vai trò không hợp lệ.");
            if (tk.DiemTichLuy < 0) ModelState.AddModelError(nameof(tk.DiemTichLuy), "Điểm tích lũy không được âm.");
            int? maTkDangNhap = HttpContext.Session.GetInt32("MaTK");
            if (maTkDangNhap == id && (tk.VaiTro != "ChuQuan" || !tk.TrangThai)) ModelState.AddModelError("", "Không thể tự đổi vai trò hoặc khóa tài khoản chủ quán đang đăng nhập.");
            ModelState.Remove(nameof(tk.MatKhauHash));
            ModelState.Remove("Orders");
            if (!ModelState.IsValid) return View(tk);
            var tkDb = await _context.Set<TaiKhoan>().FirstOrDefaultAsync(t => t.MaTk == id);
            if (tkDb == null) return NotFound();
            tkDb.TenDangNhap = tk.TenDangNhap;
            tkDb.HoTen = tk.HoTen;
            tkDb.VaiTro = tk.VaiTro;
            tkDb.DiemTichLuy = tk.DiemTichLuy;
            tkDb.TrangThai = tk.TrangThai;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var tk = await _context.Set<TaiKhoan>().AsNoTracking().FirstOrDefaultAsync(t => t.MaTk == id);
            if (tk == null) return NotFound();
            return View(tk);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tk = await _context.Set<TaiKhoan>().FirstOrDefaultAsync(t => t.MaTk == id);
            if (tk == null) return NotFound();
            int? maTkDangNhap = HttpContext.Session.GetInt32("MaTK");
            if (maTkDangNhap == id)
            {
                TempData["Error"] = "Bạn không thể tự xóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }
            if (tk.VaiTro == "ChuQuan" && tk.TrangThai)
            {
                int soChuQuanHoatDong = await _context.Set<TaiKhoan>().CountAsync(t => t.VaiTro == "ChuQuan" && t.TrangThai);
                if (soChuQuanHoatDong <= 1)
                {
                    TempData["Error"] = "Không thể xóa chủ quán hoạt động cuối cùng.";
                    return RedirectToAction(nameof(Index));
                }
            }
            bool coOrder = await _context.Set<Order>().AnyAsync(o => o.MaKh == id);
            if (coOrder)
            {
                TempData["Error"] = "Tài khoản đã liên kết với Order nên không thể xóa. Bạn có thể khóa tài khoản thay thế.";
                return RedirectToAction(nameof(Index));
            }
            _context.Set<TaiKhoan>().Remove(tk);
            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa tài khoản thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Không thể xóa tài khoản do dữ liệu liên quan. Hãy khóa tài khoản thay thế.";
            }
            return RedirectToAction(nameof(Index));
        }

    }
}
