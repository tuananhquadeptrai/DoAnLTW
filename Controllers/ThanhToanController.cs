using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VAYTIEN.Models;

public class ThanhToanController : Controller
{
    private readonly QlvayTienContext _context;

    public ThanhToanController(QlvayTienContext context)
    {
        _context = context;
    }

    // GET: /ThanhToan/ChiTiet?maHopDong=11&kyHanThu=1
    public async Task<IActionResult> ChiTiet(int maHopDong, int kyHanThu)
    {
        var hopDong = await _context.HopDongVays.FindAsync(maHopDong);
        if (hopDong == null) return NotFound();

        var kh = await _context.KhachHangs
            .FirstOrDefaultAsync(k => k.MaKh == hopDong.MaKh);
        if (kh == null) return NotFound();

        var lichTra = await _context.LichTraNos
            .FirstOrDefaultAsync(x => x.MaHopDong == maHopDong && x.KyHanThu == kyHanThu);

        var viewModel = new ThanhToanViewModel
        {
            MaHopDong = maHopDong,
            KyHan = kyHanThu,
            TenKhachHang = kh.HoTen,
            NgayTra = lichTra?.NgayTra?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today,
            SoTienPhaiTra = lichTra?.SoTienPhaiTra ?? 0,
            TrangThai = lichTra?.TrangThai ?? "Chưa trả"
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ThucHien(ThanhToanViewModel model)
    {
        var lichTra = await _context.LichTraNos
            .FirstOrDefaultAsync(x => x.MaHopDong == model.MaHopDong && x.KyHanThu == model.KyHan);

        if (lichTra != null && lichTra.TrangThai != "Đã trả")
        {
            // 👉 Xử lý theo phương thức thanh toán
            if (model.PhuongThuc == "Momo")
            {
                // Chuyển hướng giả lập Momo (thay bằng API thật nếu có)
                return Redirect($"https://momo.vn/pay?amount={model.SoTienPhaiTra}&ref={model.MaHopDong}-{model.KyHan}");
            }
            else if (model.PhuongThuc == "VNPAY")
            {
                return Redirect($"https://vnpay.vn/pay?amount={model.SoTienPhaiTra}&ref={model.MaHopDong}-{model.KyHan}");
            }

            // ❗ Nếu không chọn cổng nào, xử lý mặc định nội bộ
            lichTra.TrangThai = "Đã trả";
            lichTra.NgayTra = DateOnly.FromDateTime(DateTime.Now);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("ThongTinVay", "KhachHang");
    }
}
