using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using VAYTIEN.Models;

public class ThanhToanController : Controller
{
    private readonly QlvayTienContext _context;
    private readonly MoMoService _momoService;
    public ThanhToanController(QlvayTienContext context, MoMoService momoService)
    {
        _context = context;
        _momoService = momoService;
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

        if (lichTra == null) return NotFound();

        if (lichTra.TrangThai != "Đã trả")
        {
            if (model.PhuongThuc == "Momo")
            {
                try
                {
                    string orderId = $"{model.MaHopDong}_{model.KyHan}";
                    string orderInfo = $"Thanh toán lãi kỳ {model.KyHan} hợp đồng {model.MaHopDong}";
                    string returnUrl = Url.Action("MoMoReturn", "ThanhToan", null, Request.Scheme);
                    string notifyUrl = Url.Action("MoMoNotify", "ThanhToan", null, Request.Scheme);

                    var payUrl = await _momoService.CreatePaymentAsync(orderId, (long)model.SoTienPhaiTra, orderInfo, returnUrl, notifyUrl);
                    return Redirect(payUrl);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Không kết nối được MoMo: " + ex.Message;
                    return RedirectToAction("ChiTiet", new { maHopDong = model.MaHopDong, kyHanThu = model.KyHan });
                }
            }

            else if (model.PhuongThuc == "VNPAY")
            {
                // Tích hợp tương tự cho VNPAY nếu có
                return Redirect($"https://vnpay.vn/pay?amount={model.SoTienPhaiTra}&ref={model.MaHopDong}-{model.KyHan}");
            }

            // Nếu không chọn cổng nào, xử lý nội bộ (không khuyến nghị)
            lichTra.TrangThai = "Đã trả";
            lichTra.NgayTra = DateOnly.FromDateTime(DateTime.Now);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("ThongTinVay", "KhachHang");
    }
    // Action MoMo chuyển về sau khi thanh toán (redirect khách)
    public async Task<IActionResult> MoMoReturn()
    {
        var orderId = Request.Query["orderId"];
        var resultCode = Request.Query["resultCode"];

        if (resultCode == "0")
        {
            var ids = orderId.ToString().Split('_');
            int maHopDong = int.Parse(ids[0]);
            int kyHan = int.Parse(ids[1]);

            var lichTra = await _context.LichTraNos
                .FirstOrDefaultAsync(x => x.MaHopDong == maHopDong && x.KyHanThu == kyHan);

            if (lichTra != null)
            {
                lichTra.TrangThai = "Đã trả";
                lichTra.NgayTra = DateOnly.FromDateTime(DateTime.Now);

                var hopDong = await _context.HopDongVays.FindAsync(maHopDong);
                if (hopDong != null && lichTra.SoTienPhaiTra.HasValue)
                {
                    hopDong.SoTienConLai = (hopDong.SoTienConLai ?? hopDong.SoTienVay) - lichTra.SoTienPhaiTra.Value;
                }

                await _context.SaveChangesAsync();
            }

            ViewBag.Message = "Thanh toán thành công!";
        }
        else
        {
            ViewBag.Message = "Thanh toán không thành công hoặc bị hủy.";
        }

        return View();
    }


    // Action nhận webhook MoMo (IPN, hệ thống gọi tự động)
    [HttpPost]
    public async Task<IActionResult> MoMoNotify()
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();

        dynamic notifyData = Newtonsoft.Json.JsonConvert.DeserializeObject(body);
        string orderId = notifyData.orderId;
        string resultCode = notifyData.resultCode;

        if (resultCode == "0")
        {
            var ids = orderId.ToString().Split('_');
            int maHopDong = int.Parse(ids[0]);
            int kyHan = int.Parse(ids[1]);

            var lichTra = await _context.LichTraNos
                .FirstOrDefaultAsync(x => x.MaHopDong == maHopDong && x.KyHanThu == kyHan);

            if (lichTra != null)
            {
                lichTra.TrangThai = "Đã trả";
                lichTra.NgayTra = DateOnly.FromDateTime(DateTime.Now);

                var hopDong = await _context.HopDongVays.FindAsync(maHopDong);
                if (hopDong != null && lichTra.SoTienPhaiTra.HasValue)
                {
                    hopDong.SoTienConLai = (hopDong.SoTienConLai ?? hopDong.SoTienVay) - lichTra.SoTienPhaiTra.Value;
                }

                await _context.SaveChangesAsync();
            }
        }

        // MoMo yêu cầu trả về "200 OK" nếu đã xử lý xong
        return Ok();
    }


}