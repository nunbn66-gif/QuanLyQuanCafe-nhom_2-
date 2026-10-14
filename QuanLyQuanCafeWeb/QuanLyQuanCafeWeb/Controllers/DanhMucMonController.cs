using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Filters;
using QuanLyQuanCafeWeb.Models;
namespace QuanLyQuanCafeWeb.Controllers
{
    [SessionAuthorize("ChuQuan")]
    public class DanhMucMonController : Controller
    {
        private readonly CafeDbContext _context;
        public DanhMucMonController (CafeDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string? tuKhoa)
        {
            var query = _context.Set<DanhMucMon>().AsNoTracking().AsQueryable();
            if(!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(d => d.TenDm.Contains(tuKhoa));
            }
            ViewBag.TuKhoa = tuKhoa;
            var dsDanhMuc = await query.OrderBy(d => d.ThuTu).ThenBy(d => d.TenDm).ToListAsync();
            return View(dsDanhMuc);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new DanhMucMon { TrangThai = true, ThuTu = 0 });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDm,ThuTu,TrangThai")] DanhMucMon dm)
        {
            if (!string.IsNullOrWhiteSpace(dm.TenDm)) dm.TenDm = dm.TenDm.Trim();
            if (string.IsNullOrWhiteSpace(dm.TenDm)) ModelState.AddModelError(nameof(dm.TenDm), "Tên danh mục không được để trống.");
            if (dm.ThuTu < 0) ModelState.AddModelError(nameof(dm.ThuTu), "Thứ tự không được âm.");
            if (!string.IsNullOrWhiteSpace(dm.TenDm) && await _context.Set<DanhMucMon>().AnyAsync(d => d.TenDm == dm.TenDm)) ModelState.AddModelError(nameof(dm.TenDm), "Tên danh mục đã tồn tại.");
            ModelState.Remove("Mons");
            if (ModelState.IsValid)
            {
                _context.Set<DanhMucMon>().Add(dm);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm danh mục thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(dm);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var dm = await _context.Set<DanhMucMon>().AsNoTracking().FirstOrDefaultAsync(d => d.MaDm == id);
            if (dm == null) return NotFound();
            return View(dm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDm,TenDm,ThuTu,TrangThai")] DanhMucMon dm)
        {
            if (id != dm.MaDm) return NotFound();
            if (!string.IsNullOrWhiteSpace(dm.TenDm)) dm.TenDm = dm.TenDm.Trim();
            if (string.IsNullOrWhiteSpace(dm.TenDm)) ModelState.AddModelError(nameof(dm.TenDm), "Tên danh mục không được để trống.");
            if (dm.ThuTu < 0) ModelState.AddModelError(nameof(dm.ThuTu), "Thứ tự không được âm.");
            if (!string.IsNullOrWhiteSpace(dm.TenDm) && await _context.Set<DanhMucMon>().AnyAsync(d => d.TenDm == dm.TenDm && d.MaDm != id)) ModelState.AddModelError(nameof(dm.TenDm), "Tên danh mục đã tồn tại.");
            ModelState.Remove("Mons");
            if (!ModelState.IsValid) return View(dm);
            var dmDb = await _context.Set<DanhMucMon>().FirstOrDefaultAsync(d => d.MaDm == id);
            if (dmDb == null) return NotFound();
            dmDb.TenDm = dm.TenDm;
            dmDb.ThuTu = dm.ThuTu;
            dmDb.TrangThai = dm.TrangThai;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Sửa danh mục thành công.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var dm = await _context.Set<DanhMucMon>().AsNoTracking().FirstOrDefaultAsync(d => d.MaDm == id);
            if (dm == null) return NotFound();
            return View(dm);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dm = await _context.Set<DanhMucMon>().FirstOrDefaultAsync(d => d.MaDm == id);
            if (dm == null) return NotFound();
            bool dangSuDung = await _context.Set<Mon>().AnyAsync(m => m.MaDm == id);
            if (dangSuDung)
            {
                TempData["Error"] = "Không thể xóa danh mục vì vẫn còn món đang thuộc danh mục này. Hãy chuyển các món sang danh mục khác trước.";
                return RedirectToAction(nameof(Index));
            }
            _context.Set<DanhMucMon>().Remove(dm);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa danh mục thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
