using System.ComponentModel.DataAnnotations;
namespace QuanLyQuanCafeWeb.ViewModels
{
    public class CaLamViecFormVM
    {
        public int MaCa { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn nhân viên.")]
        public int MaTk { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn ngày làm.")]
        public DateOnly NgayLam { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        [Required(ErrorMessage = "Vui lòng chọn giờ bắt đầu.")]
        public TimeOnly GioBatDau { get; set; } = new TimeOnly(8, 0);
        [Required(ErrorMessage = "Vui lòng chọn giờ kết thúc.")]
        public TimeOnly GioKetThuc { get; set; } = new TimeOnly(16, 0);
        [StringLength(300, ErrorMessage = "Ghi chú không được vượt quá 300 ký tự.")]
        public string? GhiChu { get; set; }
    }
}