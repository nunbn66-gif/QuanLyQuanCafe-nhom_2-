
namespace QuanLyQuanCafeWeb.ViewModels
{
    public class XemDanhGiaVM
    {
        public int MaMon { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public decimal DiemTrungBinh { get; set; }
        public int SoLuotDanhGia { get; set; }
        public List<DanhGiaChiTietItemVM> DanhGias { get; set; } = new();
    }

    public class DanhGiaChiTietItemVM
    {
        public string HoTen { get; set; } = string.Empty;
        public int SoSao { get; set; }
        public string? NoiDung { get; set; }
        public DateTime NgayDanhGia { get; set; }
    }
}
