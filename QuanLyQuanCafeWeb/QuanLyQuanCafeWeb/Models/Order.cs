using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyQuanCafeWeb.Models;

[Table("Order")]
public partial class Order
{
    [Key]
    public int MaOrder { get; set; }

    public int? MaBan { get; set; }

    [Column("MaKH")]
    public int? MaKh { get; set; }

    [StringLength(20)]
    public string LoaiOrder { get; set; } = null!;

    [Precision(0)]
    public DateTime GioVao { get; set; }

    [Precision(0)]
    public DateTime? GioDatBan { get; set; }

    public int? SoNguoi { get; set; }

    [Precision(0)]
    public DateTime? GioThanhToan { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TongTien { get; set; }

    [StringLength(20)]
    public string TrangThai { get; set; } = null!;

    [StringLength(500)]
    public string? GhiChu { get; set; }

    [InverseProperty("MaOrderNavigation")]
    public virtual ICollection<ChiTietOrder> ChiTietOrders { get; set; } = new List<ChiTietOrder>();

    [ForeignKey("MaBan")]
    [InverseProperty("Orders")]
    public virtual Ban? MaBanNavigation { get; set; }

    [ForeignKey("MaKh")]
    [InverseProperty("Orders")]
    public virtual TaiKhoan? MaKhNavigation { get; set; }
}
