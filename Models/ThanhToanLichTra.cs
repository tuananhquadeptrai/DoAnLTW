using System;
using System.Collections.Generic;

namespace VAYTIEN.Models;

public partial class ThanhToanLichTra
{
    public int MaThanhToan { get; set; }

    public int? MaLich { get; set; }

    public DateOnly? NgayThanhToan { get; set; }

    public decimal? SoTienThanhToan { get; set; }

    public string? HinhThucThanhToan { get; set; }

    public string? SoTaiKhoanGd { get; set; }

    public virtual LichTraNo? MaLichNavigation { get; set; }
}
