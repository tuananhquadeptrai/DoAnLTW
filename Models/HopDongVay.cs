using System;
using System.Collections.Generic;

namespace VAYTIEN.Models;

public partial class HopDongVay
{
    public int MaHopDong { get; set; }

    public int? MaKh { get; set; }

    public int? MaLoaiVay { get; set; }

    public int? MaLoaiTienTe { get; set; }

    public decimal? SoTienVay { get; set; }

    public DateOnly? NgayVay { get; set; }

    public DateOnly? NgayHetHan { get; set; }

    public double? LaiSuat { get; set; }

    public string? HinhThucTra { get; set; }

    public int? MaNv { get; set; }

    public string? TinhTrang { get; set; }

    public virtual ICollection<LichTraNo> LichTraNos { get; set; } = new List<LichTraNo>();

    public virtual KhachHang? MaKhNavigation { get; set; }

    public virtual LoaiTienTe? MaLoaiTienTeNavigation { get; set; }

    public virtual LoaiVay? MaLoaiVayNavigation { get; set; }

    public virtual NhanVien? MaNvNavigation { get; set; }

    public virtual ICollection<TaiSanTheChap> TaiSanTheChaps { get; set; } = new List<TaiSanTheChap>();
}
