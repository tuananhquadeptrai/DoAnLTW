using System;
using System.Collections.Generic;

namespace VAYTIEN.Models;

public partial class LichTraNo
{
    public int MaLich { get; set; }

    public int? MaHopDong { get; set; }

    public int? KyHanThu { get; set; }

    public DateOnly? NgayTra { get; set; }

    public decimal? SoTienPhaiTra { get; set; }

    public string? TrangThai { get; set; }

    public virtual HopDongVay? MaHopDongNavigation { get; set; }

    public virtual ICollection<ThanhToanLichTra> ThanhToanLichTras { get; set; } = new List<ThanhToanLichTra>();
}
