namespace QuanLyQuanCafeWeb.ViewModels
{
    public class OrderIndexVM
    {
        public int MaOrder { get; set; }
        public string LoaiOrder { get; set; } = string.Empty;
        public string? TenBan { get; set; }
        public string? TenKhachHang { get; set; }
        public DateTime GioVao { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }
}