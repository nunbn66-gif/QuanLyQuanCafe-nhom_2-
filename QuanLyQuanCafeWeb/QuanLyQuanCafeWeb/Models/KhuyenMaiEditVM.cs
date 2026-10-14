namespace QuanLyQuanCafeWeb.ViewModels
{
    public class KhuyenMaiEditVM
    {
        public int MaMon { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public decimal PhanTramGiam { get; set; }
        public DateTime? NgayBatDauKhuyenMai { get; set; }
        public DateTime? NgayKetThucKhuyenMai { get; set; }
    }
}