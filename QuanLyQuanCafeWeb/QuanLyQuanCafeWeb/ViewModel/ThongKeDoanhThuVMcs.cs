namespace QuanLyQuanCafeWeb.ViewModels
{
    public class ThongKeDoanhThuVM
    {
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int SoDonThanhToan { get; set; }
        public List<DoanhThuTheoGioVM> DoanhThuTheoGio { get; set; } = new();
    }
    public class DoanhThuTheoGioVM
    {
        public int Gio { get; set; }
        public int SoDon { get; set; }
        public decimal DoanhThu { get; set; }
    }
}