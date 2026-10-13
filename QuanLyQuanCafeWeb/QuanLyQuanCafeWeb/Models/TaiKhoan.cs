using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyQuanCafeWeb.Models;

[Table("TaiKhoan")]
[Index("TenDangNhap", Name = "UQ__TaiKhoan__55F68FC06D34AD83", IsUnique = true)]
public partial class TaiKhoan
{
    [Key]
    [Column("MaTK")]
    public int MaTk { get; set; }

    [StringLength(50)]
    public string TenDangNhap { get; set; } = null!;

    [StringLength(255)]
    public string MatKhauHash { get; set; } = null!;

    [StringLength(100)]
    public string HoTen { get; set; } = null!;

    [StringLength(20)]
    public string VaiTro { get; set; } = null!;

    public int DiemTichLuy { get; set; }

    public bool TrangThai { get; set; }

    [InverseProperty("MaKhNavigation")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
