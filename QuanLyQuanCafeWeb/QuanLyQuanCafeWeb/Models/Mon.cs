using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyQuanCafeWeb.Models;

[Table("Mon")]
public partial class Mon
{
    [Key]
    public int MaMon { get; set; }

    [StringLength(150)]
    public string TenMon { get; set; } = null!;

    [Column("MaDM")]
    public int MaDm { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal GiaBan { get; set; }

    [StringLength(255)]
    public string? AnhMon { get; set; }

    [StringLength(1000)]
    public string? MoTa { get; set; }

    [StringLength(20)]
    public string TrangThai { get; set; } = null!;

    [InverseProperty("MaMonNavigation")]
    public virtual ICollection<ChiTietOrder> ChiTietOrders { get; set; } = new List<ChiTietOrder>();

    [ForeignKey("MaDm")]
    [InverseProperty("Mons")]
    public virtual DanhMucMon MaDmNavigation { get; set; } = null!;
}
