using Microsoft.EntityFrameworkCore;
namespace QuanLyQuanCafeWeb.Models
{
    public partial class Mon
    {
        [Precision(5, 2)]
        public decimal PhanTramGiam { get; set; }
        [Precision(0)]
        public DateTime? NgayBatDauKhuyenMai { get; set; }
        [Precision(0)]
        public DateTime? NgayKetThucKhuyenMai { get; set; }
    }
}