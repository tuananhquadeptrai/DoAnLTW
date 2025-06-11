namespace VAYTIEN.Models
{
    public class ThongTinVayViewModel
    {
        public decimal TongSoTienVay { get; set; }
        public List<ThongTinHopDongViewModel> HopDongs { get; set; }
    }

    public class ThongTinHopDongViewModel
    {
        public int MaHopDong { get; set; }
        public decimal? SoTienVay { get; set; }
        public DateOnly? NgayVay { get; set; }
            public DateOnly? NgayHetHan { get; set; }

        public int? KyHanThang { get; set; }     
        public double? LaiSuat { get; set; }     

        public List<LichTraViewModel> LichTra { get; set; }
    }

    public class LichTraViewModel
    {
        public int KyHanThu { get; set; }
        public DateOnly? NgayTra { get; set; }
        public decimal? SoTienPhaiTra { get; set; }
        public string? TrangThai { get; set; }
    }
}
