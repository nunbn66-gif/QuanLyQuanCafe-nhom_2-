using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuanLyQuanCafeWeb.Models;

[Table("DanhMucMon")]
[Index("TenDm", Name = "UQ__DanhMucM__4CF9655900AEBF20", IsUnique = true)]
public partial class DanhMucMon
{
    [Key]
    [Column("MaDM")]
    public int MaDm { get; set; }

    [Column("TenDM")]
    [StringLength(100)]
    public string TenDm { get; set; } = null!;

    public int ThuTu { get; set; }

    public bool TrangThai { get; set; }

    [InverseProperty("MaDmNavigation")]
    public virtual ICollection<Mon> Mons { get; set; } = new List<Mon>();
}
