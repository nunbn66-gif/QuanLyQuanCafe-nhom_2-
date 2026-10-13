using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Models;
namespace QuanLyQuanCafeWeb.Controllers
{
    public class BanController : Controller
    {
        private readonly CafeDbContext _context;
        public BanController(CafeDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string? tuKhoa, string? khuVuc, string? trangThai)
        {
            var query = _context.Set<Ban>().AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(b => b.TenBan.Contains(tuKhoa));
            }
            if (!string.IsNullOrWhiteSpace(khuVuc)) query = query.Where(b => b.KhuVuc == khuVuc);
            if (!string.IsNullOrWhiteSpace(trangThai)) query = query.Where(b => b.TrangThai == trangThai);
            var khuVucList = await _context.Set<Ban>().AsNoTracking().Select(b => b.KhuVuc).Distinct().OrderBy(k => k).ToListAsync();
            ViewBag.KhuVucList = new SelectList(khuVucList, khuVuc);
            ViewBag.TrangThaiList = new SelectList(new[] { "Trống", "Có khách", "Đã đặt", "Bảo trì" }, trangThai);
            ViewBag.TuKhoa = tuKhoa;
            var dsBan = await query.OrderBy(b => b.KhuVuc).ThenBy(b => b.TenBan).ToListAsync();
            return View(dsBan);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Ban { SoGhe = 4, TrangThai = "Trống" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenBan,KhuVuc,SoGhe,TrangThai")] Ban ban)
        {
            if (!string.IsNullOrWhiteSpace(ban.TenBan)) ban.TenBan = ban.TenBan.Trim();
            if (!string.IsNullOrWhiteSpace(ban.KhuVuc)) ban.KhuVuc = ban.KhuVuc.Trim();
            if (string.IsNullOrWhiteSpace(ban.TenBan)) ModelState.AddModelError(nameof(ban.TenBan), "Tên bàn không được để trống.");
            if (string.IsNullOrWhiteSpace(ban.KhuVuc)) ModelState.AddModelError(nameof(ban.KhuVuc), "Khu vực không được để trống.");
            if (ban.SoGhe <= 0) ModelState.AddModelError(nameof(ban.SoGhe), "Số ghế phải lớn hơn 0.");
            string[] trangThaiHopLe = { "Trống", "Có khách", "Đã đặt", "Bảo trì" };
            if (!trangThaiHopLe.Contains(ban.TrangThai)) ModelState.AddModelError(nameof(ban.TrangThai), "Trạng thái không hợp lệ.");
            if (!string.IsNullOrWhiteSpace(ban.TenBan) && await _context.Set<Ban>().AnyAsync(b => b.TenBan == ban.TenBan)) ModelState.AddModelError(nameof(ban.TenBan), "Tên bàn đã tồn tại.");
            ModelState.Remove("Orders");
            if (ModelState.IsValid)
            {
                _context.Set<Ban>().Add(ban);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm bàn thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(ban);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var ban = await _context.Set<Ban>().AsNoTracking().FirstOrDefaultAsync(b => b.MaBan == id);
            if (ban == null) return NotFound();
            return View(ban);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaBan,TenBan,KhuVuc,SoGhe,TrangThai")] Ban ban)
        {
            if (id != ban.MaBan) return NotFound();
            if (!string.IsNullOrWhiteSpace(ban.TenBan)) ban.TenBan = ban.TenBan.Trim();
            if (!string.IsNullOrWhiteSpace(ban.KhuVuc)) ban.KhuVuc = ban.KhuVuc.Trim();
            if (string.IsNullOrWhiteSpace(ban.TenBan)) ModelState.AddModelError(nameof(ban.TenBan), "Tên bàn không được để trống.");
            if (string.IsNullOrWhiteSpace(ban.KhuVuc)) ModelState.AddModelError(nameof(ban.KhuVuc), "Khu vực không được để trống.");
            if (ban.SoGhe <= 0) ModelState.AddModelError(nameof(ban.SoGhe), "Số ghế phải lớn hơn 0.");
            string[] trangThaiHopLe = { "Trống", "Có khách", "Đã đặt", "Bảo trì" };
            if (!trangThaiHopLe.Contains(ban.TrangThai)) ModelState.AddModelError(nameof(ban.TrangThai), "Trạng thái không hợp lệ.");
            if (!string.IsNullOrWhiteSpace(ban.TenBan) && await _context.Set<Ban>().AnyAsync(b => b.TenBan == ban.TenBan && b.MaBan != id)) ModelState.AddModelError(nameof(ban.TenBan), "Tên bàn đã tồn tại.");
            ModelState.Remove("Orders");
            if (!ModelState.IsValid) return View(ban);
            var banDb = await _context.Set<Ban>().FirstOrDefaultAsync(b => b.MaBan == id);
            if (banDb == null) return NotFound();
            banDb.TenBan = ban.TenBan;
            banDb.KhuVuc = ban.KhuVuc;
            banDb.SoGhe = ban.SoGhe;
            banDb.TrangThai = ban.TrangThai;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật bàn thành công.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var ban = await _context.Set<Ban>().AsNoTracking().FirstOrDefaultAsync(b => b.MaBan == id);
            if (ban == null) return NotFound();
            return View(ban);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ban = await _context.Set<Ban>().FirstOrDefaultAsync(b => b.MaBan == id);
            if (ban == null) return NotFound();
            bool daCoOrder = await _context.Set<Order>().AnyAsync(o => o.MaBan == id);
            if (daCoOrder)
            {
                TempData["Error"] = "Không thể xóa bàn đã được sử dụng trong order. Bạn có thể chuyển trạng thái bàn thành Bảo trì.";
                return RedirectToAction(nameof(Index));
            }
            _context.Set<Ban>().Remove(ban);
            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa bàn thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Không thể xóa bàn do có dữ liệu liên quan.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
