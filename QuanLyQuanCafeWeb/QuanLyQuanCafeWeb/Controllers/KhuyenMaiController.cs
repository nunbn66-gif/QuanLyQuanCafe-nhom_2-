
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Filters;
using QuanLyQuanCafeWeb.Models;
using QuanLyQuanCafeWeb.ViewModels;
namespace QuanLyQuanCafeWeb.Controllers
{
    [SessionAuthorize("ChuQuan")]
    public class KhuyenMaiController : Controller
    {
        private readonly CafeDbContext _context;
        public KhuyenMaiController(CafeDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string? tuKhoa)
        {
            var query = _context.Set<Mon>().AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(m => m.TenMon.Contains(tuKhoa));
            }
            ViewBag.TuKhoa = tuKhoa;
            var dsMon = await query.OrderBy(m => m.TenMon).ToListAsync();
            return View(dsMon);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var mon = await _context.Set<Mon>().AsNoTracking().FirstOrDefaultAsync(m => m.MaMon == id);
            if (mon == null) return NotFound();
            var vm = new KhuyenMaiEditVM { MaMon = mon.MaMon, TenMon = mon.TenMon, GiaBan = mon.GiaBan, PhanTramGiam = mon.PhanTramGiam, NgayBatDauKhuyenMai = mon.NgayBatDauKhuyenMai, NgayKetThucKhuyenMai = mon.NgayKetThucKhuyenMai };
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KhuyenMaiEditVM vm)
        {
            if (vm.PhanTramGiam < 0 || vm.PhanTramGiam > 100) ModelState.AddModelError(nameof(vm.PhanTramGiam), "Phần trăm giảm phải từ 0 đến 100.");
            if (vm.NgayBatDauKhuyenMai.HasValue && vm.NgayKetThucKhuyenMai.HasValue && vm.NgayBatDauKhuyenMai.Value > vm.NgayKetThucKhuyenMai.Value) ModelState.AddModelError(nameof(vm.NgayKetThucKhuyenMai), "Ngày kết thúc phải bằng hoặc sau ngày bắt đầu.");
            var mon = await _context.Set<Mon>().FirstOrDefaultAsync(m => m.MaMon == vm.MaMon);
            if (mon == null) return NotFound();
            if (!ModelState.IsValid)
            {
                vm.TenMon = mon.TenMon;
                vm.GiaBan = mon.GiaBan;
                return View(vm);
            }
            mon.PhanTramGiam = vm.PhanTramGiam;
            mon.NgayBatDauKhuyenMai = vm.PhanTramGiam == 0 ? null : vm.NgayBatDauKhuyenMai;
            mon.NgayKetThucKhuyenMai = vm.PhanTramGiam == 0 ? null : vm.NgayKetThucKhuyenMai;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật khuyến mãi thành công.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaKhuyenMai(int id)
        {
            var mon = await _context.Set<Mon>().FirstOrDefaultAsync(m => m.MaMon == id);
            if (mon == null) return NotFound();
            mon.PhanTramGiam = 0;
            mon.NgayBatDauKhuyenMai = null;
            mon.NgayKetThucKhuyenMai = null;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã gỡ khuyến mãi của món.";
            return RedirectToAction(nameof(Index));
        }
    }
}
