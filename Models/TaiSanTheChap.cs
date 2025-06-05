using System;
using System.Collections.Generic;

namespace VAYTIEN.Models;

public partial class TaiSanTheChap
{
    public int MaTs { get; set; }

    public int? MaHopDong { get; set; }

    public string? TenTaiSan { get; set; }

    public decimal? GiaTri { get; set; }

    public string? MoTa { get; set; }

    public string? TinhTrang { get; set; }

    public virtual HopDongVay? MaHopDongNavigation { get; set; }
}
