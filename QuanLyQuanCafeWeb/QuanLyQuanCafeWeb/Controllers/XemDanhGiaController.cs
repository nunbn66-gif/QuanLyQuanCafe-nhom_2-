
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Filters;
using QuanLyQuanCafeWeb.Models;
using QuanLyQuanCafeWeb.TempModels;
using QuanLyQuanCafeWeb.ViewModels;

namespace QuanLyQuanCafeWeb.Controllers
{
    [SessionAuthorize("ChuQuan", "NhanVien", "KhachHang")]
    public class XemDanhGiaController : Controller
    {
        private readonly CafeDbContext _context;
        public XemDanhGiaController(CafeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int maMon)
        {
            if (maMon <= 0) return BadRequest();
            var mon = await _context.Set<Mon>()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaMon == maMon);
            if (mon == null) return NotFound();

            var danhGias = await (
                from d in _context.Set<DanhGiaMon>().AsNoTracking()
                join tk in _context.Set<TaiKhoan>().AsNoTracking()
                    on d.MaTk equals tk.MaTk
                where d.MaMon == maMon
                orderby d.NgayDanhGia descending
                select new DanhGiaChiTietItemVM
                {
                    HoTen = tk.HoTen,
                    SoSao = (int)d.SoSao,
                    NoiDung = d.NoiDung,
                    NgayDanhGia = d.NgayDanhGia
                }
            ).ToListAsync();

            var vm = new XemDanhGiaVM
            {
                MaMon = mon.MaMon,
                TenMon = mon.TenMon,
                SoLuotDanhGia = danhGias.Count,
                DiemTrungBinh = danhGias.Count > 0
                    ? danhGias.Average(d => (decimal)d.SoSao)
                    : 0,
                DanhGias = danhGias
            };
            return View(vm);
        }
    }
}
