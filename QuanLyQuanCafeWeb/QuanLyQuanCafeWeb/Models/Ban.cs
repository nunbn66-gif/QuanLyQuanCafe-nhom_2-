using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyQuanCafeWeb.Models;

[Table("Ban")]
[Index("TenBan", Name = "UQ__Ban__31C246CA239564D8", IsUnique = true)]
public partial class Ban
{
    [Key]
    public int MaBan { get; set; }

    [StringLength(50)]
    public string TenBan { get; set; } = null!;

    [StringLength(100)]
    public string KhuVuc { get; set; } = null!;

    public int SoGhe { get; set; }

    [StringLength(20)]
    public string TrangThai { get; set; } = null!;

    [InverseProperty("MaBanNavigation")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
