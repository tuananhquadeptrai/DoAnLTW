using System;
using System.Collections.Generic;

namespace VAYTIEN.Models;

public partial class LoaiVay
{
    public int MaLoaiVay { get; set; }

    public string? TenLoaiVay { get; set; }

    public double? LaiSuat { get; set; }

    public int? KyHan { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<HopDongVay> HopDongVays { get; set; } = new List<HopDongVay>();
}
