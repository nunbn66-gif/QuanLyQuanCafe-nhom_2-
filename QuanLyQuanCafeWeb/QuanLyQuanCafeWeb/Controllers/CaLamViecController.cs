using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Filters;
using QuanLyQuanCafeWeb.Models;
using QuanLyQuanCafeWeb.TempModels;
using QuanLyQuanCafeWeb.ViewModels;

namespace QuanLyQuanCafeWeb.Controllers
{
    public class CaLamViecController : Controller
    {
        private readonly CafeDbContext _context;
        public CaLamViecController(CafeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [SessionAuthorize("ChuQuan", "NhanVien")]
        public async Task<IActionResult> Index(DateOnly? ngayLam)
        {
            var query = _context.Set<CaLamViec>().AsNoTracking().AsQueryable();
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro != null && vaiTro.Equals("NhanVien", StringComparison.OrdinalIgnoreCase))
            {
                var maTk = HttpContext.Session.GetInt32("MaTK");
                if (!maTk.HasValue)
                    return StatusCode(StatusCodes.Status403Forbidden);
                query = query.Where(c => c.MaTk == maTk.Value);
            }
            if (ngayLam.HasValue)
                query = query.Where(c => c.NgayLam == ngayLam.Value);
            ViewBag.NgayLam = ngayLam?.ToString("yyyy-MM-dd");
            var dsCa = await query.OrderByDescending(c => c.NgayLam)
                .ThenBy(c => c.GioBatDau)
                .ToListAsync();
            return View(dsCa);
        }

        [HttpGet]
        [SessionAuthorize("ChuQuan")]
        public async Task<IActionResult> Create()
        {
            await NapNhanVien();
            return View(new CaLamViecFormVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize("ChuQuan")]
        public async Task<IActionResult> Create(CaLamViecFormVM vm)
        {
            await NapNhanVien(vm.MaTk);
            if (vm.GioBatDau >= vm.GioKetThuc)
                ModelState.AddModelError(nameof(vm.GioKetThuc), "Giờ kết thúc phải sau giờ bắt đầu.");
            if (vm.MaTk > 0 && !await LaNhanVienHoatDong(vm.MaTk))
                ModelState.AddModelError(nameof(vm.MaTk), "Tài khoản không tồn tại, không hoạt động hoặc không phải nhân viên.");
            if (vm.MaTk > 0 && vm.GioBatDau < vm.GioKetThuc && await CoTrungLichAsync(vm))
                ModelState.AddModelError("", "Nhân viên đã có ca làm trùng giờ trong ngày này.");
            if (!ModelState.IsValid)
                return View(vm);
            var ca = new CaLamViec
            {
                MaTk = vm.MaTk,
                NgayLam = vm.NgayLam,
                GioBatDau = vm.GioBatDau,
                GioKetThuc = vm.GioKetThuc,
                TrangThai = "Đã phân công",
                GhiChu = string.IsNullOrWhiteSpace(vm.GhiChu) ? null : vm.GhiChu.Trim()
            };
            _context.Set<CaLamViec>().Add(ca);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Phân công ca làm thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [SessionAuthorize("ChuQuan")]
        public async Task<IActionResult> Edit(int id)
        {
            var ca = await _context.Set<CaLamViec>().AsNoTracking()
                .FirstOrDefaultAsync(c => c.MaCa == id);
            if (ca == null) return NotFound();
            if (ca.TrangThai == "Đã hủy")
            {
                TempData["Error"] = "Ca đã hủy không thể chỉnh sửa.";
                return RedirectToAction(nameof(Index));
            }
            await NapNhanVien(ca.MaTk);
            return View(new CaLamViecFormVM
            {
                MaCa = ca.MaCa,
                MaTk = ca.MaTk,
                NgayLam = ca.NgayLam,
                GioBatDau = ca.GioBatDau,
                GioKetThuc = ca.GioKetThuc,
                GhiChu = ca.GhiChu
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize("ChuQuan")]
        public async Task<IActionResult> Edit(CaLamViecFormVM vm)
        {
            var ca = await _context.Set<CaLamViec>()
                .FirstOrDefaultAsync(c => c.MaCa == vm.MaCa);
            if (ca == null) return NotFound();
            if (ca.TrangThai == "Đã hủy")
            {
                TempData["Error"] = "Ca đã hủy không thể chỉnh sửa.";
                return RedirectToAction(nameof(Index));
            }
            await NapNhanVien(vm.MaTk);
            if (vm.GioBatDau >= vm.GioKetThuc)
                ModelState.AddModelError(nameof(vm.GioKetThuc), "Giờ kết thúc phải sau giờ bắt đầu.");
            if (vm.MaTk > 0 && !await LaNhanVienHoatDong(vm.MaTk))
                ModelState.AddModelError(nameof(vm.MaTk), "Tài khoản không tồn tại, không hoạt động hoặc không phải nhân viên.");
            if (vm.MaTk > 0 && vm.GioBatDau < vm.GioKetThuc && await CoTrungLichAsync(vm, vm.MaCa))
                ModelState.AddModelError("", "Nhân viên đã có ca làm trùng giờ trong ngày này.");
            if (!ModelState.IsValid)
                return View(vm);
            ca.MaTk = vm.MaTk;
            ca.NgayLam = vm.NgayLam;
            ca.GioBatDau = vm.GioBatDau;
            ca.GioKetThuc = vm.GioKetThuc;
            ca.GhiChu = string.IsNullOrWhiteSpace(vm.GhiChu) ? null : vm.GhiChu.Trim();
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật ca làm thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize("ChuQuan")]
        public async Task<IActionResult> Huy(int id)
        {
            var ca = await _context.Set<CaLamViec>()
                .FirstOrDefaultAsync(c => c.MaCa == id);
            if (ca == null) return NotFound();
            if (ca.TrangThai == "Hoàn thành")
            {
                TempData["Error"] = "Không thể hủy ca đã hoàn thành.";
                return RedirectToAction(nameof(Index));
            }
            ca.TrangThai = "Đã hủy";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã hủy ca làm.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize("ChuQuan", "NhanVien")]
        public async Task<IActionResult> BatDau(int id)
        {
            return await DoiTrangThaiCa(id, "Đã phân công", "Đang làm");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize("ChuQuan", "NhanVien")]
        public async Task<IActionResult> HoanThanh(int id)
        {
            return await DoiTrangThaiCa(id, "Đang làm", "Hoàn thành");
        }
        private async Task<IActionResult> DoiTrangThaiCa(int id, string trangThaiCu, string trangThaiMoi)
        {
            var ca = await _context.Set<CaLamViec>().FirstOrDefaultAsync(c => c.MaCa == id);
            if (ca == null) return NotFound();
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            if (vaiTro == "NhanVien")
            {
                var maTk = HttpContext.Session.GetInt32("MaTK");
                if (!maTk.HasValue || ca.MaTk != maTk.Value)
                {
                    TempData["Error"] = "Bạn chỉ được cập nhật ca làm việc của chính mình.";
                    return RedirectToAction(nameof(Index));
                }
            }
            if (ca.NgayLam != DateOnly.FromDateTime(DateTime.Today))
            {
                TempData["Error"] = "Chỉ được bắt đầu hoặc kết thúc ca trong ngày làm việc đã phân công.";
                return RedirectToAction(nameof(Index));
            }
            if (ca.TrangThai != trangThaiCu)
            {
                TempData["Error"] = $"Không thể chuyển ca từ trạng thái hiện tại sang {trangThaiMoi}.";
                return RedirectToAction(nameof(Index));
            }
            ca.TrangThai = trangThaiMoi;
            await _context.SaveChangesAsync();
            TempData["Success"] = trangThaiMoi == "Đang làm" ? "Đã bắt đầu ca làm." : "Đã hoàn thành ca làm.";
            return RedirectToAction(nameof(Index));
        }

        private async Task NapNhanVien(int? selected = null)
        {
            var dsNhanVien = await _context.Set<TaiKhoan>()
                .AsNoTracking()
                .Where(t => t.VaiTro == "NhanVien" && t.TrangThai)
                .OrderBy(t => t.MaTk)
                .ToListAsync();
            ViewBag.NhanVienList = dsNhanVien.Select(t => new SelectListItem
            {
                Value = t.MaTk.ToString(),
                Text = $"{t.HoTen} (Mã: {t.MaTk})",
                Selected = selected == t.MaTk
            }).ToList();
        }

        private async Task<bool> LaNhanVienHoatDong(int maTk)
        {
            return await _context.Set<TaiKhoan>().AnyAsync(t =>
                t.MaTk == maTk && t.VaiTro == "NhanVien" && t.TrangThai);
        }

        private async Task<bool> CoTrungLichAsync(CaLamViecFormVM vm, int? boQuaMaCa = null)
        {
            var query = _context.Set<CaLamViec>().AsNoTracking()
                .Where(c => c.MaTk == vm.MaTk
                    && c.NgayLam == vm.NgayLam
                    && c.TrangThai != "Đã hủy");
            if (boQuaMaCa.HasValue)
                query = query.Where(c => c.MaCa != boQuaMaCa.Value);
            return await query.AnyAsync(c =>
                c.GioBatDau < vm.GioKetThuc
                && vm.GioBatDau < c.GioKetThuc);
        }
    }
}