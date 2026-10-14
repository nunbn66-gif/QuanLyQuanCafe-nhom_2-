using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeWeb.TempModels;

public partial class CaLamViec
{
    public int MaCa { get; set; }

    public int MaTk { get; set; }

    public DateOnly NgayLam { get; set; }

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; }
}
