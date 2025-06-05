using System;
using System.Collections.Generic;

namespace VAYTIEN.Models;

public partial class NhanVien
{
    public int MaNv { get; set; }

    public string? TenNv { get; set; }

    public string? Email { get; set; }

    public string? Sdt { get; set; }

    public int? MaChiNhanh { get; set; }

    public string? MatKhau { get; set; }

    public string? VaiTro { get; set; }

    public virtual ICollection<HopDongVay> HopDongVays { get; set; } = new List<HopDongVay>();

    public virtual ChiNhanhNganHang? MaChiNhanhNavigation { get; set; }
}
