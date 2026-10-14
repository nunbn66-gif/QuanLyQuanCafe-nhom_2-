
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Filters;
using QuanLyQuanCafeWeb.Models;
namespace QuanLyQuanCafeWeb.Controllers
{
    [SessionAuthorize("KhachHang", "NhanVien", "ChuQuan")]
    public class DatBanController : Controller
    {
        private readonly CafeDbContext _context;
        public DatBanController(CafeDbContext context)
        {
            _context = context;
        }
        private async Task NapBanTrong(int? maBanDuocChon = null)
        {
            var dsBan = await _context.Set<Ban>().AsNoTracking().Where(b => b.TrangThai == "Trống").OrderBy(b => b.TenBan).ToListAsync();
            ViewBag.BanList = new SelectList(dsBan, "MaBan", "TenBan", maBanDuocChon);
        }
        public async Task<IActionResult> Index()
        {
            var query = _context.Set<Order>().AsNoTracking().Include(o => o.MaBanNavigation).Include(o => o.MaKhNavigation).Where(o => o.GioDatBan != null && o.TrangThai != "Đã hủy");
            if (HttpContext.Session.GetString("VaiTro") == "KhachHang")
            {
                int? maTk = HttpContext.Session.GetInt32("MaTK");
                query = query.Where(o => o.MaKh == maTk);
            }
            var dsDatBan = await query.OrderBy(o => o.GioDatBan).ToListAsync();
            return View(dsDatBan);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await NapBanTrong();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? maBan, DateTime gioDatBan, int soNguoi, string? ghiChu)
        {
            int? maTk = HttpContext.Session.GetInt32("MaTK");
            if (!maTk.HasValue) return RedirectToAction("Login", "Auth");
            Ban? ban = null;
            if (!maBan.HasValue) ModelState.AddModelError(nameof(maBan), "Vui lòng chọn bàn.");
            else
            {
                ban = await _context.Set<Ban>().FirstOrDefaultAsync(b => b.MaBan == maBan.Value);
                if (ban == null || ban.TrangThai != "Trống") ModelState.AddModelError(nameof(maBan), "Bàn không tồn tại hoặc hiện không trống.");
                else if (soNguoi > ban.SoGhe) ModelState.AddModelError(nameof(soNguoi), $"Bàn này chỉ có {ban.SoGhe} ghế.");
            }
            if (gioDatBan <= DateTime.Now) ModelState.AddModelError(nameof(gioDatBan), "Thời gian đặt bàn phải ở tương lai.");
            if (soNguoi <= 0) ModelState.AddModelError(nameof(soNguoi), "Số người phải lớn hơn 0.");
            if (ban != null && gioDatBan > DateTime.Now)
            {
                var tu = gioDatBan.AddMinutes(-60);
                var den = gioDatBan.AddMinutes(60);
                bool biTrung = await _context.Set<Order>().AnyAsync(o => o.MaBan == maBan && o.GioDatBan.HasValue && o.TrangThai != "Đã hủy" && o.GioDatBan.Value >= tu && o.GioDatBan.Value <= den);
                if (biTrung) ModelState.AddModelError(nameof(gioDatBan), "Bàn đã có lịch đặt trong khoảng thời gian này.");
            }
            if (!ModelState.IsValid)
            {
                await NapBanTrong(maBan);
                return View();
            }
            var order = new Order { MaBan = maBan, MaKh = maTk, LoaiOrder = "Tại chỗ", GioVao = DateTime.Now, GioDatBan = gioDatBan, SoNguoi = soNguoi, TongTien = 0, TrangThai = "Đang tạo", GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim() };
            _context.Set<Order>().Add(order);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đặt bàn thành công. Mã đặt bàn: {order.MaOrder}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
