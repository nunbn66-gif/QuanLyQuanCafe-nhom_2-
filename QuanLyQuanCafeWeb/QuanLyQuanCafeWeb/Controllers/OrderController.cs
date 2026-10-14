using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Data;
using QuanLyQuanCafeWeb.Filters;
using QuanLyQuanCafeWeb.Models;
using QuanLyQuanCafeWeb.ViewModels;
namespace QuanLyQuanCafeWeb.Controllers
{
    [SessionAuthorize("NhanVien", "ChuQuan")]
    public class OrderController : Controller
    {
        private readonly CafeDbContext _context;
        public OrderController(CafeDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? maOrder, string? loaiOrder, string? trangThai)
        {
            var query = _context.Set<Order>().AsNoTracking().AsQueryable();
            if (maOrder.HasValue) query = query.Where(o => o.MaOrder == maOrder.Value);
            if (!string.IsNullOrWhiteSpace(loaiOrder)) query = query.Where(o => o.LoaiOrder == loaiOrder);
            if (!string.IsNullOrWhiteSpace(trangThai)) query = query.Where(o => o.TrangThai == trangThai);
            var dsOrder = await (from o in query
                                 join b in _context.Set<Ban>().AsNoTracking() on o.MaBan equals (int?)b.MaBan into banJoin
                                 from b in banJoin.DefaultIfEmpty()
                                 join tk in _context.Set<TaiKhoan>().AsNoTracking() on o.MaKh equals (int?)tk.MaTk into khJoin
                                 from tk in khJoin.DefaultIfEmpty()
                                 select new OrderIndexVM
                                 {
                                     MaOrder = o.MaOrder,
                                     LoaiOrder = o.LoaiOrder,
                                     TenBan = b == null ? null : b.TenBan,
                                     TenKhachHang = tk == null ? null : tk.HoTen,
                                     GioVao = o.GioVao,
                                     TongTien = o.TongTien,
                                     TrangThai = o.TrangThai
                                 }).OrderByDescending(o => o.GioVao).ToListAsync();
            ViewBag.MaOrder = maOrder;
            ViewBag.LoaiOrderList = new SelectList(new[] { "Tại chỗ", "Mang đi" }, loaiOrder);
            ViewBag.TrangThaiList = new SelectList(new[] { "Đang tạo", "Chờ thanh toán", "Đã thanh toán", "Đã hủy" }, trangThai);
            return View(dsOrder);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await NapBanTrong();
            await NapKhachHang();
            return View();
        }
        private async Task NapBanTrong(int? maBanDuocChon = null)
        {
            var dsBan = await _context.Set<Ban>().AsNoTracking().Where(b => b.TrangThai == "Trống").OrderBy(b => b.TenBan).ToListAsync();
            ViewBag.BanList = new SelectList(dsBan, "MaBan", "TenBan", maBanDuocChon);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? loaiOrder, int? maBan, int? maKh)
        {
            if (maKh.HasValue && !await _context.Set<TaiKhoan>().AnyAsync(t => t.MaTk == maKh.Value && t.VaiTro == "KhachHang" && t.TrangThai))
                ModelState.AddModelError(nameof(maKh), "Khách hàng không tồn tại hoặc tài khoản đã bị khóa.");
            string[] loaiHopLe = { "Tại chỗ", "Mang đi" };
            if (string.IsNullOrWhiteSpace(loaiOrder) || !loaiHopLe.Contains(loaiOrder)) ModelState.AddModelError(nameof(loaiOrder), "Vui lòng chọn loại Order hợp lệ.");
            Ban? ban = null;
            if (loaiOrder == "Tại chỗ")
            {
                if (!maBan.HasValue) ModelState.AddModelError(nameof(maBan), "Vui lòng chọn bàn.");
                else
                {
                    ban = await _context.Set<Ban>().FirstOrDefaultAsync(b => b.MaBan == maBan.Value);
                    if (ban == null || ban.TrangThai != "Trống") ModelState.AddModelError(nameof(maBan), "Bàn không tồn tại hoặc không còn trống.");
                }
            }
            ModelState.Remove("ChiTietOrders");
            ModelState.Remove("MaBanNavigation");
            ModelState.Remove("MaKhNavigation");
            if (!ModelState.IsValid)
            {
                await NapBanTrong(maBan);
                await NapKhachHang(maKh);
                return View();
            }
            var order = new Order { MaBan = loaiOrder == "Tại chỗ" ? maBan : null, MaKh = maKh, LoaiOrder = loaiOrder!, GioVao = DateTime.Now, TongTien = 0, TrangThai = "Đang tạo" };
            if (loaiOrder == "Tại chỗ" && ban != null) ban.TrangThai = "Có khách";
            _context.Set<Order>().Add(order);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Tạo Order #{order.MaOrder} thành công.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Set<Order>().Include(o => o.MaBanNavigation).Include(o => o.MaKhNavigation).Include(o => o.ChiTietOrders).ThenInclude(ct => ct.MaMonNavigation).AsNoTracking().FirstOrDefaultAsync(o => o.MaOrder == id);
            if (order == null) return NotFound();
            return View(order);
        }
        private async Task NapMon(int? selected = null)
        {
            var now = DateTime.Now;
            var dsMon = await _context.Set<Mon>()
                .AsNoTracking()
                .Where(m => m.TrangThai == "Còn bán")
                .OrderBy(m => m.TenMon)
                .ToListAsync();
            ViewBag.MonList = dsMon.Select(mon =>
            {
                bool dangKhuyenMai = mon.PhanTramGiam > 0
                    && (!mon.NgayBatDauKhuyenMai.HasValue || mon.NgayBatDauKhuyenMai.Value <= now)
                    && (!mon.NgayKetThucKhuyenMai.HasValue || mon.NgayKetThucKhuyenMai.Value >= now);
                decimal giaApDung = dangKhuyenMai
                    ? decimal.Round(mon.GiaBan * (100m - mon.PhanTramGiam) / 100m, 0, MidpointRounding.AwayFromZero)
                    : mon.GiaBan;
                string tenHienThi = dangKhuyenMai
                    ? $"{mon.TenMon} - {giaApDung:N0}đ (giá gốc {mon.GiaBan:N0}đ, giảm {mon.PhanTramGiam:0.##}%)"
                    : $"{mon.TenMon} - {mon.GiaBan:N0}đ";
                return new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = mon.MaMon.ToString(),
                    Text = tenHienThi,
                    Selected = selected.HasValue && selected.Value == mon.MaMon
                };
            }).ToList();
        }
        [HttpGet]
        public async Task<IActionResult> AddMon(int id)
        {
            var order = await _context.Set<Order>().AsNoTracking().FirstOrDefaultAsync(o => o.MaOrder == id);
            if (order == null) return NotFound();
            if (order.TrangThai == "Đã thanh toán" || order.TrangThai == "Đã hủy")
            {
                TempData["Error"] = "Order đã kết thúc, không thể thêm món.";
                return RedirectToAction(nameof(Details), new { id });
            }
            ViewBag.MaOrder = id;
            await NapMon();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMon(int id, int maMon, int soLuong, string? ghiChu)
        {
            var order = await _context.Set<Order>().FirstOrDefaultAsync(o => o.MaOrder == id);
            if (order == null) return NotFound();
            if (order.TrangThai == "Đã thanh toán" || order.TrangThai == "Đã hủy")
            {
                TempData["Error"] = "Order đã kết thúc, không thể thêm món.";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (soLuong <= 0) ModelState.AddModelError(nameof(soLuong), "Số lượng phải lớn hơn 0.");
            var monDb = await _context.Set<Mon>().FirstOrDefaultAsync(m => m.MaMon == maMon && m.TrangThai == "Còn bán");
            if (monDb == null) ModelState.AddModelError(nameof(maMon), "Món không tồn tại hoặc đã ngừng bán.");
            if (!ModelState.IsValid)
            {
                ViewBag.MaOrder = id;
                await NapMon(maMon);
                return View();
            }
            var mon = monDb!;
            decimal giaApDung = mon.GiaBan;
            var thoiDiem = DateTime.Now;
            if (mon.PhanTramGiam > 0
                && (!mon.NgayBatDauKhuyenMai.HasValue || mon.NgayBatDauKhuyenMai.Value <= thoiDiem)
                && (!mon.NgayKetThucKhuyenMai.HasValue || mon.NgayKetThucKhuyenMai.Value >= thoiDiem))
            {
                giaApDung = decimal.Round(mon.GiaBan * (100m - mon.PhanTramGiam) / 100m, 0, MidpointRounding.AwayFromZero);
            }
            var dsChiTiet = await _context.Set<ChiTietOrder>().Where(ct => ct.MaOrder == id).ToListAsync();
            var ctCu = dsChiTiet.FirstOrDefault(ct => ct.MaMon == maMon);
            if (ctCu != null)
            {
                ctCu.SoLuong += soLuong;
                ctCu.DonGia = giaApDung;
                if (!string.IsNullOrWhiteSpace(ghiChu)) ctCu.GhiChu = ghiChu.Trim();
            }
            else
            {
                var ctMoi = new ChiTietOrder
                {
                    MaOrder = id,
                    MaMon = maMon,
                    SoLuong = soLuong,
                    DonGia = giaApDung,
                    GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? null : ghiChu.Trim()
                };
                _context.Set<ChiTietOrder>().Add(ctMoi);
                dsChiTiet.Add(ctMoi);
            }
            order.TongTien = dsChiTiet.Sum(ct => ct.SoLuong * ct.DonGia);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm món vào Order thành công.";
            return RedirectToAction(nameof(Details), new { id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatSoLuong(int id, int maMon, int soLuong)
        {
            var order = await _context.Set<Order>().FirstOrDefaultAsync(o => o.MaOrder == id);
            if (order == null) return NotFound();
            if (order.TrangThai == "Đã thanh toán" || order.TrangThai == "Đã hủy")
            {
                TempData["Error"] = "Order đã kết thúc, không thể sửa số lượng món.";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (soLuong <= 0)
            {
                TempData["Error"] = "Số lượng phải lớn hơn 0.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var dsChiTiet = await _context.Set<ChiTietOrder>().Where(ct => ct.MaOrder == id).ToListAsync();
            var ct = dsChiTiet.FirstOrDefault(ct => ct.MaMon == maMon);
            if (ct == null) return NotFound();
            ct.SoLuong = soLuong;
            order.TongTien = dsChiTiet.Sum(ct => ct.SoLuong * ct.DonGia);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật số lượng thành công.";
            return RedirectToAction(nameof(Details), new { id });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaMon(int id, int maMon)
        {
            var order = await _context.Set<Order>().FirstOrDefaultAsync(o => o.MaOrder == id);
            if (order == null) return NotFound();
            if (order.TrangThai == "Đã thanh toán" || order.TrangThai == "Đã hủy")
            {
                TempData["Error"] = "Order đã kết thúc, không thể xóa món.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var dsChiTiet = await _context.Set<ChiTietOrder>().Where(ct => ct.MaOrder == id).ToListAsync();
            var ct = dsChiTiet.FirstOrDefault(ct => ct.MaMon == maMon);
            if (ct == null) return NotFound();
            dsChiTiet.Remove(ct);
            _context.Set<ChiTietOrder>().Remove(ct);
            order.TongTien = dsChiTiet.Sum(ct => ct.SoLuong * ct.DonGia);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa món khỏi Order.";
            return RedirectToAction(nameof(Details), new { id });
        }
        [HttpGet]
        public async Task<IActionResult> ChuyenBan(int id)
        {
            var order = await _context.Set<Order>().AsNoTracking().FirstOrDefaultAsync(o => o.MaOrder == id);
            if (order == null) return NotFound();
            if (order.LoaiOrder != "Tại chỗ" || !order.MaBan.HasValue || order.TrangThai == "Đã thanh toán" || order.TrangThai == "Đã hủy")
            {
                TempData["Error"] = "Order này không thể chuyển bàn.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var banHienTai = await _context.Set<Ban>().AsNoTracking().FirstOrDefaultAsync(b => b.MaBan == order.MaBan);
            if (banHienTai == null) return NotFound();
            var dsBan = await _context.Set<Ban>().AsNoTracking().Where(b => b.MaBan != order.MaBan && b.TrangThai == "Trống" && !_context.Set<Order>().Any(o => o.MaBan == b.MaBan && o.GioDatBan == null && o.TrangThai != "Đã hủy" && o.TrangThai != "Đã thanh toán")).OrderBy(b => b.TenBan).ToListAsync();
            ViewBag.BanList = new SelectList(dsBan, "MaBan", "TenBan");
            ViewBag.MaOrder = id;
            ViewBag.TenBanHienTai = banHienTai.TenBan;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChuyenBan(int id, int maBanMoi)
        {
            var order = await _context.Set<Order>().FirstOrDefaultAsync(o => o.MaOrder == id);
            if (order == null) return NotFound();
            if (order.LoaiOrder != "Tại chỗ" || !order.MaBan.HasValue || order.TrangThai == "Đã thanh toán" || order.TrangThai == "Đã hủy")
            {
                TempData["Error"] = "Order này không thể chuyển bàn.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var banCu = await _context.Set<Ban>().FirstOrDefaultAsync(b => b.MaBan == order.MaBan);
            var banMoi = await _context.Set<Ban>().FirstOrDefaultAsync(b => b.MaBan == maBanMoi);
            bool banMoiDangCoOrder = await _context.Set<Order>().AnyAsync(o => o.MaBan == maBanMoi && o.GioDatBan == null && o.TrangThai != "Đã hủy" && o.TrangThai != "Đã thanh toán");
            if (banCu == null || banMoi == null || banMoi.MaBan == banCu.MaBan || banMoi.TrangThai != "Trống" || banMoiDangCoOrder)
            {
                TempData["Error"] = "Bàn mới không hợp lệ hoặc không còn trống.";
                return RedirectToAction(nameof(Details), new { id });
            }
            banCu.TrangThai = "Trống";
            banMoi.TrangThai = "Có khách";
            order.MaBan = maBanMoi;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã chuyển Order #{id} sang {banMoi.TenBan}.";
            return RedirectToAction(nameof(Details), new { id });
        }
        [HttpGet]
        public async Task<IActionResult> GopBan(int id)
        {
            var nguon = await _context.Set<Order>().Include(o => o.MaBanNavigation).AsNoTracking().FirstOrDefaultAsync(o => o.MaOrder == id);
            if (nguon == null) return NotFound();
            if (nguon.LoaiOrder != "Tại chỗ" || !nguon.MaBan.HasValue || nguon.GioDatBan.HasValue || nguon.TrangThai == "Đã thanh toán" || nguon.TrangThai == "Đã hủy")
            {
                TempData["Error"] = "Order nguồn không thể gộp bàn.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var dsOrder = await (from o in _context.Set<Order>().AsNoTracking()
                                 join b in _context.Set<Ban>().AsNoTracking() on o.MaBan equals (int?)b.MaBan
                                 where o.MaOrder != id && o.LoaiOrder == "Tại chỗ" && o.GioDatBan == null && (o.TrangThai == "Đang tạo" || o.TrangThai == "Chờ thanh toán")
                                 select new { o.MaOrder, TenHienThi = "Order #" + o.MaOrder + " - " + b.TenBan }).ToListAsync();
            if (dsOrder.Count == 0)
            {
                TempData["Error"] = "Không có Order tại chỗ khác đang hoạt động để gộp.";
                return RedirectToAction(nameof(Details), new { id });
            }
            ViewBag.MaOrderNguon = id;
            ViewBag.TenBanNguon = nguon.MaBanNavigation?.TenBan;
            ViewBag.OrderDichList = new SelectList(dsOrder, "MaOrder", "TenHienThi");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GopBan(int id, int maOrderDich)
        {
            if (id == maOrderDich)
            {
                TempData["Error"] = "Không thể gộp Order vào chính nó.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var nguon = await _context.Set<Order>().FirstOrDefaultAsync(o => o.MaOrder == id);
            var dich = await _context.Set<Order>().FirstOrDefaultAsync(o => o.MaOrder == maOrderDich);
            if (nguon == null || dich == null) return NotFound();
            if (nguon.LoaiOrder != "Tại chỗ" || !nguon.MaBan.HasValue || nguon.GioDatBan.HasValue || (nguon.TrangThai != "Đang tạo" && nguon.TrangThai != "Chờ thanh toán") || dich.LoaiOrder != "Tại chỗ" || !dich.MaBan.HasValue || dich.GioDatBan.HasValue || (dich.TrangThai != "Đang tạo" && dich.TrangThai != "Chờ thanh toán"))
            {
                TempData["Error"] = "Hai Order phải là Order tại chỗ đang hoạt động và chưa thanh toán.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var banNguon = await _context.Set<Ban>().FirstOrDefaultAsync(b => b.MaBan == nguon.MaBan);
            var banDich = await _context.Set<Ban>().FirstOrDefaultAsync(b => b.MaBan == dich.MaBan);
            if (banNguon == null || banDich == null || banNguon.MaBan == banDich.MaBan) return BadRequest();
            var dsNguon = await _context.Set<ChiTietOrder>().Where(ct => ct.MaOrder == id).ToListAsync();
            var dsDich = await _context.Set<ChiTietOrder>().Where(ct => ct.MaOrder == maOrderDich).ToListAsync();
            if (dsNguon.Count == 0)
            {
                TempData["Error"] = "Order nguồn chưa có món để gộp.";
                return RedirectToAction(nameof(Details), new { id });
            }
            bool giaKhac = dsNguon.Any(src => dsDich.Any(dst => dst.MaMon == src.MaMon && dst.DonGia != src.DonGia));
            if (giaKhac)
            {
                TempData["Error"] = "Hai Order có cùng món nhưng giá lưu khác nhau. Không thể gộp tự động để tránh sai tổng tiền.";
                return RedirectToAction(nameof(Details), new { id });
            }
            await using var transaction = await _context.Database.BeginTransactionAsync();
            foreach (var src in dsNguon)
            {
                var dst = dsDich.FirstOrDefault(ct => ct.MaMon == src.MaMon);
                if (dst != null)
                {
                    dst.SoLuong += src.SoLuong;
                    var ghiChu = new[] { dst.GhiChu, src.GhiChu }.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray();
                    if (ghiChu.Length > 0)
                    {
                        var noiDung = string.Join(" | ", ghiChu);
                        dst.GhiChu = noiDung.Length > 255 ? noiDung[..255] : noiDung;
                    }
                }
                else
                {
                    var ctMoi = new ChiTietOrder { MaOrder = maOrderDich, MaMon = src.MaMon, SoLuong = src.SoLuong, DonGia = src.DonGia, GhiChu = src.GhiChu };
                    _context.Set<ChiTietOrder>().Add(ctMoi);
                    dsDich.Add(ctMoi);
                }
                _context.Set<ChiTietOrder>().Remove(src);
            }
            dich.TongTien = dsDich.Sum(ct => ct.SoLuong * ct.DonGia);
            nguon.TongTien = 0;
            nguon.TrangThai = "Đã hủy";
            var ghiChuGop = $"Đã gộp vào Order #{maOrderDich}";
            var ghiChuNguon = string.IsNullOrWhiteSpace(nguon.GhiChu) ? ghiChuGop : nguon.GhiChu + " | " + ghiChuGop;
            nguon.GhiChu = ghiChuNguon.Length > 500 ? ghiChuNguon[..500] : ghiChuNguon;
            banNguon.TrangThai = "Trống";
            banDich.TrangThai = "Có khách";
            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Không thể gộp bàn do dữ liệu đã thay đổi hoặc có ràng buộc không hợp lệ.";
                return RedirectToAction(nameof(Details), new { id });
            }
            TempData["Success"] = $"Đã gộp Order #{id} vào Order #{maOrderDich}.";
            return RedirectToAction(nameof(Details), new { id = maOrderDich });
        }

        [HttpGet]
        public async Task<IActionResult> ThanhToan(int id)
        {
            var order = await _context.Set<Order>().Include(o => o.MaBanNavigation).Include(o => o.MaKhNavigation).Include(o => o.ChiTietOrders).ThenInclude(ct => ct.MaMonNavigation).AsNoTracking().FirstOrDefaultAsync(o => o.MaOrder == id);
            if (order == null) return NotFound();
            if (order.TrangThai == "Đã thanh toán") return RedirectToAction(nameof(HoaDon), new { id });
            if (order.TrangThai == "Đã hủy")
            {
                TempData["Error"] = "Order đã hủy, không thể thanh toán.";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (!order.ChiTietOrders.Any())
            {
                TempData["Error"] = "Order chưa có món, không thể thanh toán.";
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(order);
        }
        [HttpPost]
        [ActionName("ThanhToan")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToanConfirmed(int id)
        {
            var order = await _context.Set<Order>().FirstOrDefaultAsync(o => o.MaOrder == id);
            if (order == null) return NotFound();
            if (order.TrangThai == "Đã thanh toán") return RedirectToAction(nameof(HoaDon), new { id });
            if (order.TrangThai == "Đã hủy")    
            {
                TempData["Error"] = "Order đã hủy, không thể thanh toán.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var dsChiTiet = await _context.Set<ChiTietOrder>().Where(ct => ct.MaOrder == id).ToListAsync();
            if (dsChiTiet.Count == 0)
            {
                TempData["Error"] = "Order chưa có món, không thể thanh toán.";
                return RedirectToAction(nameof(Details), new { id });
            }
            await using var transaction = await _context.Database.BeginTransactionAsync();
            order.TongTien = dsChiTiet.Sum(ct => ct.SoLuong * ct.DonGia);
            if (order.MaKh.HasValue)
            {
                var tk = await _context.Set<TaiKhoan>().FirstOrDefaultAsync(t => t.MaTk == order.MaKh.Value && t.VaiTro == "KhachHang" && t.TrangThai);
                if (tk != null)
                {
                    int diemCong = (int)(order.TongTien / 10000m);
                    tk.DiemTichLuy += diemCong;
                }
            }
            if (order.MaKh.HasValue)
            {
                var tk = await _context.Set<TaiKhoan>().FirstOrDefaultAsync(t => t.MaTk == order.MaKh.Value && t.VaiTro == "KhachHang" && t.TrangThai);
                if (tk != null)
                {
                    int diemCong = (int)(order.TongTien / 10000m);
                    tk.DiemTichLuy += diemCong;
                }
            }
            order.TrangThai = "Đã thanh toán";
            order.GioThanhToan = DateTime.Now;
            if (order.MaBan.HasValue)
            {
                var ban = await _context.Set<Ban>().FirstOrDefaultAsync(b => b.MaBan == order.MaBan.Value);
                if (ban != null)
                {
                    bool conOrderHoatDong = await _context.Set<Order>().AnyAsync(o => o.MaOrder != id && o.MaBan == ban.MaBan && o.GioDatBan == null && (o.TrangThai == "Đang tạo" || o.TrangThai == "Chờ thanh toán"));
                    bool coDatBanTuongLai = await _context.Set<Order>().AnyAsync(o => o.MaOrder != id && o.MaBan == ban.MaBan && o.GioDatBan > DateTime.Now && (o.TrangThai == "Đang tạo" || o.TrangThai == "Chờ thanh toán"));
                    ban.TrangThai = conOrderHoatDong ? "Có khách" : coDatBanTuongLai ? "Đã đặt" : "Trống";
                }
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Success"] = $"Thanh toán Order #{id} thành công.";
            return RedirectToAction(nameof(HoaDon), new { id });
        }
        [HttpGet]
        public async Task<IActionResult> HoaDon(int id)
        {
            var order = await _context.Set<Order>().Include(o => o.MaBanNavigation).Include(o => o.MaKhNavigation).Include(o => o.ChiTietOrders).ThenInclude(ct => ct.MaMonNavigation).AsNoTracking().FirstOrDefaultAsync(o => o.MaOrder == id);
            if (order == null) return NotFound();
            if (order.TrangThai != "Đã thanh toán")
            {
                TempData["Error"] = "Order chưa thanh toán nên chưa có hóa đơn hoàn tất.";
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(order);
        }
        private async Task NapKhachHang(int? maKhDuocChon = null)
        {
            var dsKhach = await _context.Set<TaiKhoan>().AsNoTracking().Where(t => t.VaiTro == "KhachHang" && t.TrangThai).OrderBy(t => t.HoTen).ToListAsync();
            ViewBag.KhachHangList = new SelectList(dsKhach, "MaTk", "HoTen", maKhDuocChon);
        }
        private decimal TinhGiaBan(Mon mon, DateTime thoiDiem)
        {
            bool dangKhuyenMai = mon.PhanTramGiam > 0
                && (!mon.NgayBatDauKhuyenMai.HasValue || mon.NgayBatDauKhuyenMai.Value <= thoiDiem)
                && (!mon.NgayKetThucKhuyenMai.HasValue || mon.NgayKetThucKhuyenMai.Value >= thoiDiem);
            if (!dangKhuyenMai)
                return mon.GiaBan;
            return decimal.Round(mon.GiaBan * (100m - mon.PhanTramGiam) / 100m, 0, MidpointRounding.AwayFromZero);
        }
    }
}