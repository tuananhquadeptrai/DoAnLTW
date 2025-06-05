using System;
using System.Collections.Generic;

namespace VAYTIEN.Models;

public partial class KhachHang
{
    public int MaKh { get; set; }

    public string? HoTen { get; set; }

    public string? CmndCccd { get; set; }

    public DateOnly? NgaySinh { get; set; }

    public string? DiaChi { get; set; }

    public string? Sdt { get; set; }

    public string? Email { get; set; }

    public string? NgheNghiep { get; set; }

    public string? TinhTrangHonNhan { get; set; }

    // ✅ Thêm trường ảnh đính kèm
    public string? AnhDinhKem { get; set; }

    public virtual ICollection<HopDongVay> HopDongVays { get; set; } = new List<HopDongVay>();

    public virtual ICollection<TaiKhoanNganHang> TaiKhoanNganHangs { get; set; } = new List<TaiKhoanNganHang>();
}
