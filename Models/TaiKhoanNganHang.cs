using System;
using System.Collections.Generic;

namespace VAYTIEN.Models;

public partial class TaiKhoanNganHang
{
    public int MaTaiKhoan { get; set; }

    public int? MaKh { get; set; }

    public string? SoTaiKhoan { get; set; }

    public string? LoaiTaiKhoan { get; set; }

    public decimal? SoDu { get; set; }

    public string? TrangThai { get; set; }

    public virtual ICollection<GiaoDich> GiaoDiches { get; set; } = new List<GiaoDich>();

    public virtual KhachHang? MaKhNavigation { get; set; }
}
