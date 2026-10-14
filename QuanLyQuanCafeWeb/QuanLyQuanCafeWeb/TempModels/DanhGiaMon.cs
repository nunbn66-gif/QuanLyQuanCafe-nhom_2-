using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeWeb.TempModels;

public partial class DanhGiaMon
{
    public int MaDanhGia { get; set; }

    public int MaTk { get; set; }

    public int MaMon { get; set; }

    public byte SoSao { get; set; }

    public string? NoiDung { get; set; }

    public DateTime NgayDanhGia { get; set; }
}
