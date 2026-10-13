using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyQuanCafeWeb.Models;

[PrimaryKey("MaOrder", "MaMon")]
[Table("ChiTietOrder")]
public partial class ChiTietOrder
{
    [Key]
    public int MaOrder { get; set; }

    [Key]
    public int MaMon { get; set; }

    public int SoLuong { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DonGia { get; set; }

    [StringLength(255)]
    public string? GhiChu { get; set; }

    [ForeignKey("MaMon")]
    [InverseProperty("ChiTietOrders")]
    public virtual Mon MaMonNavigation { get; set; } = null!;

    [ForeignKey("MaOrder")]
    [InverseProperty("ChiTietOrders")]
    public virtual Order MaOrderNavigation { get; set; } = null!;
}
