using System;
using System.Collections.Generic;

namespace VAYTIEN.Models;

public partial class LoaiTienTe
{
    public int MaLoaiTienTe { get; set; }

    public string? TenLoaiTienTe { get; set; }

    public virtual ICollection<HopDongVay> HopDongVays { get; set; } = new List<HopDongVay>();
}
