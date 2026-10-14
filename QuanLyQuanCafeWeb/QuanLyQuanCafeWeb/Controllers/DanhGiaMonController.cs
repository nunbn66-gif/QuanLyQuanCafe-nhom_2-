
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Filters;
using QuanLyQuanCafeWeb.Models;
using QuanLyQuanCafeWeb.TempModels;
using QuanLyQuanCafeWeb.ViewModels;

namespace QuanLyQuanCafeWeb.Controllers
{
    [SessionAuthorize("KhachHang")]
    public class DanhGiaMonController : Controller
    {
        private readonly CafeDbContext _context;
        public DanhGiaMonController(CafeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var maTk = HttpContext.Session.GetInt32("MaTK");
            if (!maTk.HasValue)
                return RedirectToAction("Login", "Auth");

            var dsMonDaMua = await (
                from ct in _context.Set<ChiTietOrder>().AsNoTracking()
                join o in _context.Set<Order>().AsNoTracking()
                    on ct.MaOrder equals o.MaOrder
                join m in _context.Set<Mon>().AsNoTracking()
                    on ct.MaMon equals m.MaMon
                where o.MaKh == maTk.Value
                    && o.TrangThai == "Đã thanh toán"
                select new { m.MaMon, m.TenMon }
            ).Distinct().OrderBy(m => m.TenMon).ToListAsync();

            var dsDanhGia = await _context.Set<DanhGiaMon>()
                .AsNoTracking()
                .Where(d => d.MaTk == maTk.Value)
                .ToDictionaryAsync(d => d.MaMon);

            var model = dsMonDaMua.Select(m =>
            {
                dsDanhGia.TryGetValue(m.MaMon, out var dg);
                return new DanhGiaMonItemVM
                {
                    MaMon = m.MaMon,
                    TenMon = m.TenMon,
                    SoSao = dg == null ? null : (int?)dg.SoSao,
                    NoiDung = dg?.NoiDung,
                    NgayDanhGia = dg?.NgayDanhGia
                };
            }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Luu(int maMon, int soSao, string? noiDung)
        {
            var maTk = HttpContext.Session.GetInt32("MaTK");
            if (!maTk.HasValue)
                return RedirectToAction("Login", "Auth");

            if (soSao < 1 || soSao > 5)
            {
                TempData["Error"] = "Số sao phải từ 1 đến 5.";
                return RedirectToAction(nameof(Index));
            }

            if (noiDung?.Trim().Length > 500)
            {
                TempData["Error"] = "Nhận xét không được vượt quá 500 ký tự.";
                return RedirectToAction(nameof(Index));
            }

            var daMua = await (
                from ct in _context.Set<ChiTietOrder>().AsNoTracking()
                join o in _context.Set<Order>().AsNoTracking()
                    on ct.MaOrder equals o.MaOrder
                where ct.MaMon == maMon
                    && o.MaKh == maTk.Value
                    && o.TrangThai == "Đã thanh toán"
                select o.MaOrder
            ).AnyAsync();

            if (!daMua)
            {
                TempData["Error"] = "Bạn chỉ được đánh giá món trong đơn hàng đã thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            var danhGia = await _context.Set<DanhGiaMon>()
                .FirstOrDefaultAsync(d => d.MaTk == maTk.Value && d.MaMon == maMon);

            if (danhGia == null)
            {
                danhGia = new DanhGiaMon
                {
                    MaTk = maTk.Value,
                    MaMon = maMon,
                    SoSao = (byte)soSao,
                    NoiDung = string.IsNullOrWhiteSpace(noiDung) ? null : noiDung.Trim(),
                    NgayDanhGia = DateTime.Now
                };
                _context.Set<DanhGiaMon>().Add(danhGia);
            }
            else
            {
                danhGia.SoSao = (byte)soSao;
                danhGia.NoiDung = string.IsNullOrWhiteSpace(noiDung) ? null : noiDung.Trim();
                danhGia.NgayDanhGia = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Lưu đánh giá món thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
