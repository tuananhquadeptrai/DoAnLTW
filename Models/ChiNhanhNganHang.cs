using System;
using System.Collections.Generic;

namespace VAYTIEN.Models;

public partial class ChiNhanhNganHang
{
    public int MaChiNhanh { get; set; }

    public string? TenChiNhanh { get; set; }

    public string? DiaChi { get; set; }

    public string? Sdt { get; set; }

    public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();
}
