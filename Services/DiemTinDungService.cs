using Microsoft.EntityFrameworkCore;
using VAYTIEN.Models;

namespace VAYTIEN.Services
{
    public class DiemTinDungService : IDiemTinDungService
    {
        private readonly QlvayTienContext _context;

        public DiemTinDungService(QlvayTienContext context)
        {
            _context = context;
        }

        public async Task CapNhatDiemVaHanMucAsync(int maKh)
        {
            var khachHang = await _context.KhachHangs
                .Include(kh => kh.HopDongVays)
                    .ThenInclude(hd => hd.LichTraNos)
                .FirstOrDefaultAsync(kh => kh.MaKh == maKh);

            if (khachHang == null) return;

            int diem = 500; // Điểm cơ bản

            // 1. Dựa vào Nghề nghiệp
            if (khachHang.NgheNghiep?.Contains("Công chức") == true || khachHang.NgheNghiep?.Contains("Viên chức") == true)
            {
                diem += 100;
            }
            else if (khachHang.NgheNghiep?.Contains("Kinh doanh") == true)
            {
                diem += 50;
            }

            // 2. Dựa vào Tình trạng hôn nhân
            if (khachHang.TinhTrangHonNhan == "Đã kết hôn")
            {
                diem += 50;
            }

            // 3. Dựa vào Lịch sử trả nợ
            var homNay = DateOnly.FromDateTime(DateTime.Now);
            int soKyTreHan = khachHang.HopDongVays
                                      .SelectMany(hd => hd.LichTraNos)
                                      .Count(l => l.TrangThai != "Đã trả" && l.NgayTra < homNay);

            // Trừ 50 điểm cho mỗi kỳ trả trễ
            diem -= (soKyTreHan * 50);

            // 4. Dựa vào số hợp đồng đã hoàn thành
            int soHopDongHoanThanh = khachHang.HopDongVays
                                        .Count(hd => hd.TinhTrang == "Đã duyệt" && !hd.LichTraNos.Any(l => l.TrangThai != "Đã trả"));

            // Cộng 30 điểm cho mỗi hợp đồng đã trả xong
            diem += (soHopDongHoanThanh * 30);


            // Cập nhật điểm và hạn mức
            khachHang.DiemTinDung = Math.Max(300, diem); // Điểm thấp nhất là 300
            khachHang.HanMucVay = khachHang.DiemTinDung * 150000; // Ví dụ: Hạn mức = Điểm * 150,000

            _context.Update(khachHang);
            await _context.SaveChangesAsync();
        }
    }
}
