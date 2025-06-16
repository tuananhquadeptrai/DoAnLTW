using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VAYTIEN.Models; // Đảm bảo namespace này đúng

namespace VAYTIEN.Models
{
    public class HopDongChiTietViewModel
    {
        // Thông tin Hợp đồng
        public int MaHopDong { get; set; }
        public string TenKhachHang { get; set; } = string.Empty;
        public decimal SoTienVay { get; set; }
        public decimal SoTienConLai { get; set; } // Tổng tiền gốc còn lại
        public DateOnly NgayVay { get; set; }
        public DateOnly NgayHetHan { get; set; }
        public decimal LaiSuat { get; set; }
        public int KyHanThang { get; set; }
        public string? HinhThucTra { get; set; }
        public string? TinhTrangHopDong { get; set; }

        // Thông tin tài sản thế chấp (nếu có)
        public string? TenTaiSan { get; set; }
        public decimal? GiaTriTaiSan { get; set; }
        public string? MoTaTaiSan { get; set; }
        public string? TinhTrangTaiSan { get; set; }

        // Danh sách lịch trả nợ chi tiết
        public List<LichTraNoChiTietViewModel> LichTraNo { get; set; } = new List<LichTraNoChiTietViewModel>();

        // Các thống kê nhanh (tính toán trong Controller hoặc ViewModel)
        public decimal TongGocDaTra { get; set; }
        public decimal TongLaiDaTra { get; set; }
        public decimal TongPhatDaTra { get; set; }
        public int SoKyDaTra { get; set; }
        public int SoKyConLai { get; set; }
        public int SoKyQuaHan { get; set; }
    }

    public class LichTraNoChiTietViewModel
    {
        public int MaLich { get; set; }
        public int KyHanThu { get; set; }
        public DateOnly NgayDenHan { get; set; } // Ngày phải trả
        public decimal SoTienGoc { get; set; }
        public decimal SoTienLai { get; set; }
        public decimal SoTienPhat { get; set; } // Tiền phạt thực tế đã trả (nếu có)
        public decimal SoTienPhaiTra { get; set; } // Tổng gốc + lãi ban đầu của kỳ
        public string TrangThaiKyTra { get; set; } = string.Empty;
        public DateOnly? NgayThanhToanThucTe { get; set; } // Ngày thực tế đã thanh toán (nếu có)
        public string? HinhThucThanhToan { get; set; } // Phương thức thanh toán (MoMo, VNPAY...)
        public string? MaGiaoDichThanhToan { get; set; } // Mã GD của lần thanh toán
    }
}