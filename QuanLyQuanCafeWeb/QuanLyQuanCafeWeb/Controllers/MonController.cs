using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
namespace QuanLyQuanCafeWeb.Controllers
{
    public class MonController : Controller
    {
        private readonly CafeDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public MonController(CafeDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        public async Task<IActionResult> Index(string? tuKhoa,int? maDm,int page =1)
        {
            int pageSize = 10;
            page = page < 1 ? 1 : page;

            var query = _context.Set<Mon>().AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(m => m.TenMon.Contains(tuKhoa));
            }
            if(maDm.HasValue)
            {
                query = query.Where(m => m.MaDm == maDm.Value);
            }
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;
            var dsMon = await query.OrderBy(m => m.TenMon).Skip((page -1)*pageSize).Take(pageSize).ToListAsync();
            var danhMucs = await _context.Set<DanhMucMon>().AsNoTracking().OrderBy(d => d.TenDm).ToListAsync();
            ViewBag.DanhMucFilter = new SelectList(danhMucs, "MaDm", "TenDm", maDm);
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            return View(dsMon);
            
        }
        private async Task NapDanhMuc(int? maDmDuocChon=null)
        {
            var danhMucs=await _context.Set<DanhMucMon>().AsNoTracking().OrderBy(d => d.TenDm).ToListAsync();
            ViewData["MaDm"] = new SelectList(danhMucs, "MaDm", "TenDm", maDmDuocChon);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await NapDanhMuc();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenMon,MaDm,GiaBan,MoTa,TrangThai")] Mon mon, IFormFile? anhFile)
        {
            if (mon.GiaBan < 0)
                ModelState.AddModelError(nameof(mon.GiaBan), "Giá bán không được âm.");
            bool danhMucTonTai = await _context.Set<DanhMucMon>().AnyAsync(d => d.MaDm == mon.MaDm);
            if (!danhMucTonTai)
                ModelState.AddModelError(nameof(mon.MaDm), "Vui lòng chọn danh mục hợp lệ.");
            string[] duoiAnhHopLe = { ".jpg", ".jpeg", ".png", ".webp" };
            if (anhFile != null && anhFile.Length > 0)
            {
                string duoiAnh = Path.GetExtension(anhFile.FileName).ToLowerInvariant();
                if (!duoiAnhHopLe.Contains(duoiAnh))
                    ModelState.AddModelError("anhFile", "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");
                if (anhFile.Length > 5 * 1024 * 1024)
                    ModelState.AddModelError("anhFile", "Dung lượng ảnh không được vượt quá 5 MB.");
            }
            ModelState.Remove("MaDmNavigation");
            if (ModelState.IsValid)
            {
                if (anhFile != null && anhFile.Length > 0)
                {
                    string duoiAnh = Path.GetExtension(anhFile.FileName).ToLowerInvariant();
                    string thuMuc = Path.Combine(_environment.WebRootPath, "images", "mon");
                    Directory.CreateDirectory(thuMuc);
                    string tenFile = $"{Guid.NewGuid():N}{duoiAnh}";
                    string duongDan = Path.Combine(thuMuc, tenFile);
                    await using (var stream = new FileStream(duongDan, FileMode.Create))
                    {
                        await anhFile.CopyToAsync(stream);
                    }
                    mon.AnhMon = $"/images/mon/{tenFile}";
                }
                _context.Set<Mon>().Add(mon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await NapDanhMuc(mon.MaDm);
            return View(mon);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            {
                if (id == null) return NotFound();

                var mon = await _context.Set<Mon>().FirstOrDefaultAsync(m => m.MaMon == id);
                if (mon == null) return NotFound();
                await NapDanhMuc(mon.MaDm);
                return View(mon);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaMon,TenMon,MaDm,GiaBan,MoTa,TrangThai")]Mon mon ,IFormFile ? anhFile)
        {
            if(id != mon.MaMon) return NotFound();
            if (mon.GiaBan < 0)
                ModelState.AddModelError(nameof(mon.GiaBan), "Giá bán không được âm.");
            bool danhMucTonTai = await _context.Set<DanhMucMon>().AnyAsync(d => d.MaDm == mon.MaDm);
            if (!danhMucTonTai)
                ModelState.AddModelError(nameof(mon.MaDm), "Danh mục không tồn tại.");
            string[] duoiAnhHopLe = { ".jpg", ".jpeg", ".png", ".webp" };
            if(anhFile != null && anhFile.Length > 0)
            {
                string duoiAnh = Path.GetExtension(anhFile.FileName).ToLowerInvariant();
                if (!duoiAnhHopLe.Contains(duoiAnh))
                    ModelState.AddModelError("anhFile", "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");
                if (anhFile.Length > 5 * 1024 * 1024)
                    ModelState.AddModelError("anhFile", "Dung lượng ảnh không được vượt quá 5 MB.");
            }
            ModelState.Remove("MaDmNavigation");
            if(!ModelState.IsValid)
            {
                await NapDanhMuc(mon.MaDm);
                return View(mon);
            }
            var monDb=await _context.Set<Mon>().FirstOrDefaultAsync(m=> m.MaMon == id);
            if (monDb == null) return NotFound();
            string? anhCu = monDb.AnhMon;
            monDb.TenMon = mon.TenMon;
            monDb.MaDm = mon.MaDm;
            monDb.GiaBan = mon.GiaBan;
            monDb.MoTa = mon.MoTa;
            monDb.TrangThai = mon.TrangThai;
            if(anhFile != null && anhFile.Length >0)
            {
                string duoiAnh = Path.GetExtension(anhFile.FileName).ToLowerInvariant();
                string thuMuc = Path.Combine(_environment.WebRootPath, "images", "mon");
                Directory.CreateDirectory(thuMuc); 
                string tenFile = $"{Guid.NewGuid():N}{duoiAnh}";    
                string duongDan = Path.Combine(thuMuc, tenFile);
                await using (var stream = new FileStream(duongDan, FileMode.Create))
                {
                    await anhFile.CopyToAsync(stream);
                }
                monDb.AnhMon = $"/images/mon/{tenFile}";
            }
            await _context.SaveChangesAsync();
            if(anhFile != null && anhFile.Length >0 && !string.IsNullOrWhiteSpace(anhCu) && anhCu.StartsWith("/images/mon/",StringComparison.OrdinalIgnoreCase))
            {
                string tenAnhCu = Path.GetFileName(anhCu);

            }
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var mon = await _context.Set<Mon>().AsNoTracking().FirstOrDefaultAsync(m => m.MaMon == id);
            if (mon == null) return NotFound();
            return View(mon);
        }
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mon = await _context.Set<Mon>().FirstOrDefaultAsync(m => m.MaMon == id);
            if (mon == null) return NotFound();
            try
            {
                string? anhCu = mon.AnhMon;
                _context.Set<Mon>().Remove(mon);
                await _context.SaveChangesAsync();
                //.startwith(...) xem có bắt đầu bằng kí tự chỉ định k
                //    StringComparison.OrdinalIgnoreCase ko phân biệt hoa thường trong kí tự chỉ định
                if (!string.IsNullOrWhiteSpace(anhCu) && anhCu.StartsWith("/images/mon/",StringComparison.OrdinalIgnoreCase))
                {
                    string duongDanAnh = Path.Combine(_environment.WebRootPath, "images", "mon", Path.GetFileName(anhCu));
                    if (System.IO.File.Exists(duongDanAnh)) System.IO.File.Delete(duongDanAnh);
                }
                TempData["Success"] = "Đã xóa món thành công";
            }
            catch(DbUpdateException)
            {
                TempData["Error"] = "Không thể xóa món này vì đã được sử dụng trong order. Bạn nên chuyển trạng thái món thành ngừng bán";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
