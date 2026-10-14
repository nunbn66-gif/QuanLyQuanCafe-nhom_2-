namespace QuanLyQuanCafeWeb.ViewModels
{
    public class DanhGiaMonItemVM
    {
        public int MaMon { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public int? SoSao { get; set; }
        public string? NoiDung { get; set; }
        public DateTime? NgayDanhGia { get; set; }
    }
}