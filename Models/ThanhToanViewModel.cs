namespace VAYTIEN.Models
{
    public class ThanhToanViewModel
    {
        public int MaHopDong { get; set; }
        public int KyHan { get; set; }
        public DateTime NgayTra { get; set; }
        public decimal SoTienPhaiTra { get; set; }
        public string TenKhachHang { get; set; }
        public string TrangThai { get; set; }
        public string PhuongThuc { get; set; } // "Momo" hoặc "VNPAY"
    
    }

}
