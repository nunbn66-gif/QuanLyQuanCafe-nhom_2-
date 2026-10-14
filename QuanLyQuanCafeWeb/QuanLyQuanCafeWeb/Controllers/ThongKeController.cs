using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Filters;
using QuanLyQuanCafeWeb.Models;
using QuanLyQuanCafeWeb.ViewModels;
namespace QuanLyQuanCafeWeb.Controllers
{
    [SessionAuthorize("ChuQuan")]
    public class ThongKeController : Controller
    {
        private readonly CafeDbContext _context;
        public ThongKeController(CafeDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay)
        {
            var homNay = DateTime.Today;
            var tu = tuNgay?.Date ?? new DateTime(homNay.Year, homNay.Month, 1);
            var den = denNgay?.Date ?? homNay;
            var vm = new ThongKeDoanhThuVM { TuNgay = tu, DenNgay = den };
            if (den < tu)
            {
                ModelState.AddModelError(string.Empty, "Ngày kết thúc phải bằng hoặc sau ngày bắt đầu.");
                return View(vm);
            }
            var denDocQuyen = den.AddDays(1);
            var query = _context.Set<Order>().AsNoTracking().Where(o => o.TrangThai == "Đã thanh toán" && o.GioThanhToan.HasValue && o.GioThanhToan.Value >= tu && o.GioThanhToan.Value < denDocQuyen);
            vm.TongDoanhThu = await query.SumAsync(o => (decimal?)o.TongTien) ?? 0m;
            vm.SoDonThanhToan = await query.CountAsync();
            var duLieu = await query.Select(o => new { Gio = o.GioThanhToan!.Value, o.TongTien }).ToListAsync();
            vm.DoanhThuTheoGio = duLieu.GroupBy(o => o.Gio.Hour).OrderBy(g => g.Key).Select(g => new DoanhThuTheoGioVM { Gio = g.Key, SoDon = g.Count(), DoanhThu = g.Sum(o => o.TongTien) }).ToList();
            return View(vm);
        }
    }
}