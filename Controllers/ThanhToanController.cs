using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using VAYTIEN.Models;
using VAYTIEN.Services;

[Authorize]
public class ThanhToanController : Controller
{
    private readonly QlvayTienContext _context;
    private readonly MoMoService _momoService;
    private readonly VnpayService _vnpayService;

    public ThanhToanController(QlvayTienContext context, MoMoService momoService, VnpayService vnpayService)
    {
        _context = context;
        _momoService = momoService;
        _vnpayService = vnpayService;
    }

    // GET: /ThanhToan/ChiTiet?maHopDong=11&kyHanThu=1
    public async Task<IActionResult> ChiTiet(int maHopDong, int kyHanThu)
    {
        var lichTra = await _context.LichTraNos
            .Include(l => l.MaHopDongNavigation.MaKhNavigation)
            .FirstOrDefaultAsync(x => x.MaHopDong == maHopDong && x.KyHanThu == kyHanThu);

        if (lichTra?.MaHopDongNavigation?.MaKhNavigation == null)
        {
            return NotFound();
        }

        if (lichTra.MaHopDongNavigation.MaKhNavigation.Email != User.Identity.Name)
        {
            return Forbid();
        }

        var viewModel = new ThanhToanViewModel
        {
            MaHopDong = maHopDong,
            KyHan = kyHanThu,
            TenKhachHang = lichTra.MaHopDongNavigation.MaKhNavigation.HoTen,
            NgayTra = lichTra.NgayTra?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Today,
            SoTienPhaiTra = lichTra.SoTienPhaiTra ?? 0,
            TrangThai = lichTra.TrangThai ?? "Chưa trả"
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThucHien(ThanhToanViewModel model)
    {
        var lichTra = await _context.LichTraNos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaHopDong == model.MaHopDong && x.KyHanThu == model.KyHan);

        if (lichTra == null) return NotFound();
        if (lichTra.TrangThai == "Đã trả")
        {
            TempData["Error"] = "Kỳ hạn này đã được thanh toán.";
            return RedirectToAction("ChiTiet", new { maHopDong = model.MaHopDong, kyHanThu = model.KyHan });
        }

        if (model.PhuongThuc == "Momo")
        {
            try
            {

                string orderId = $"{model.MaHopDong}_{model.KyHan}_{Guid.NewGuid().ToString().Substring(0, 8)}";

                string orderInfo = $"Thanh toan HD#{model.MaHopDong} Ky#{model.KyHan}";
                string returnUrl = Url.Action("MoMoReturn", "ThanhToan", null, Request.Scheme)!;
                string notifyUrl = Url.Action("MoMoNotify", "ThanhToan", null, Request.Scheme)!;

                var payUrl = await _momoService.CreatePaymentAsync(orderId, (long)model.SoTienPhaiTra, orderInfo, returnUrl, notifyUrl);
                return Redirect(payUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể kết nối với MoMo. Vui lòng thử lại sau. Lỗi: " + ex.Message;
                return RedirectToAction("ChiTiet", new { maHopDong = model.MaHopDong, kyHanThu = model.KyHan });
            }
        }
        if (model.PhuongThuc == "VNPAY")
        {
            // Tạo orderId duy nhất giống như MoMo
            string orderId = $"{model.MaHopDong}_{model.KyHan}_{Guid.NewGuid().ToString().Substring(0, 8)}";
            string orderInfo = $"Thanh toan HD#{model.MaHopDong} Ky#{model.KyHan}";
            string returnUrl = Url.Action("VnpayReturn", "ThanhToan", null, Request.Scheme)!;

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var payUrl = _vnpayService.CreatePaymentUrl((long)model.SoTienPhaiTra, orderId, orderInfo, clientIp);
            return Redirect(payUrl);
        }


        TempData["Error"] = "Vui lòng chọn phương thức thanh toán.";
        return RedirectToAction("ChiTiet", new { maHopDong = model.MaHopDong, kyHanThu = model.KyHan });
    }

    // Action MoMo chuyển về sau khi thanh toán
    public async Task<IActionResult> MoMoReturn()
    {
        var queryCollection = Request.Query;
        if (queryCollection.Count > 0 && queryCollection.ContainsKey("resultCode") && queryCollection["resultCode"] == "0")
        {
            var orderId = queryCollection["orderId"].ToString();

            var success = await ProcessSuccessfulPaymentAndUpdateDbAsync(orderId);
            if (success)
            {
                TempData["Success"] = "Thanh toán thành công! Dư nợ của bạn đã được cập nhật.";
            }
            else
            {
                TempData["Error"] = "Giao dịch đã được xử lý trước đó hoặc có lỗi xảy ra.";
            }
        }
        else
        {
            TempData["Error"] = "Thanh toán không thành công hoặc đã bị hủy.";
        }

        return RedirectToAction("ThongTinVay", "KhachHang");
    }

    // Action nhận webhook MoMo (IPN)
    [HttpPost]
    public async Task<IActionResult> MoMoNotify()
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();
        dynamic? notifyData = JsonConvert.DeserializeObject(body);

        if (notifyData != null && notifyData.resultCode == "0")
        {
            string orderId = notifyData.orderId;
            await ProcessSuccessfulPaymentAndUpdateDbAsync(orderId);
        }

        return NoContent();
    }
    public async Task<IActionResult> VnpayReturn()
    {
        var queryCollection = Request.Query;
        if (queryCollection.Count > 0 && queryCollection.ContainsKey("vnp_ResponseCode") && queryCollection["vnp_ResponseCode"] == "00")
        {
            var orderId = queryCollection["vnp_TxnRef"].ToString();

            var success = await ProcessSuccessfulPaymentAndUpdateDbAsync(orderId);
            if (success)
            {
                TempData["Success"] = "Thanh toán VNPay thành công! Dư nợ của bạn đã được cập nhật.";
            }
            else
            {
                TempData["Error"] = "Giao dịch đã được xử lý trước đó hoặc có lỗi xảy ra.";
            }
        }
        else
        {
            TempData["Error"] = "Thanh toán VNPay không thành công hoặc đã bị hủy.";
        }

        return RedirectToAction("ThongTinVay", "KhachHang");
    }


    // Hàm dùng chung để xử lý và cập nhật database
    private async Task<bool> ProcessSuccessfulPaymentAndUpdateDbAsync(string orderId)
    {
        try
        {
            var ids = orderId.Split('_');
            if (ids.Length < 2) return false;

            int maHopDong = int.Parse(ids[0]);
            int kyHan = int.Parse(ids[1]);

            var lichTra = await _context.LichTraNos
                .FirstOrDefaultAsync(x => x.MaHopDong == maHopDong && x.KyHanThu == kyHan);

            // Rất quan trọng: Kiểm tra xem đã xử lý chưa để tránh cập nhật 2 lần
            if (lichTra != null && lichTra.TrangThai != "Đã trả")
            {
                lichTra.TrangThai = "Đã trả";
                lichTra.NgayTra = DateOnly.FromDateTime(DateTime.Now);

                // --- Cập nhật số tiền còn lại ---
                var hopDong = await _context.HopDongVays.FirstOrDefaultAsync(h => h.MaHopDong == maHopDong);
                if (hopDong != null && lichTra.SoTienPhaiTra.HasValue)
                {
                    // Nếu SoTienConLai null thì mặc định bằng SoTienVay
                    if (!hopDong.SoTienConLai.HasValue && hopDong.SoTienVay.HasValue)
                    {
                        hopDong.SoTienConLai = hopDong.SoTienVay;
                    }
                    // Trừ tiền kỳ hạn vừa trả (không âm)
                    hopDong.SoTienConLai = Math.Max(0, (hopDong.SoTienConLai ?? 0) - lichTra.SoTienPhaiTra.Value);
                }

                // Lưu thay đổi trạng thái kỳ trả nợ + hợp đồng
                await _context.SaveChangesAsync();

                return true;
            }
        }
        catch
        {
            // Ghi log lỗi ở đây nếu cần
            return false;
        }

        return false;
    }

}
