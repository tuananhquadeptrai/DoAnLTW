using System;
using System.Collections.Generic;

namespace VAYTIEN.Models;

public partial class GiaoDich
{
    public int MaGiaoDich { get; set; }

    public int? MaTaiKhoan { get; set; }

    public DateOnly? NgayGd { get; set; }

    public decimal? SoTienGd { get; set; }

    public string? LoaiGd { get; set; }

    public string? NoiDungGd { get; set; }

    public virtual TaiKhoanNganHang? MaTaiKhoanNavigation { get; set; }
}
