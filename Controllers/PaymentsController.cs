using Microsoft.AspNetCore.Mvc;
using VAYTIEN.Models;

public class PaymentsController : Controller
{
    private readonly MoMoService _momoService;
    // private readonly AppDbContext _db; // Nếu muốn cập nhật DB

    public PaymentsController(MoMoService momoService /*, AppDbContext db */)
    {
        _momoService = momoService;
        // _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> ThanhToanMomo(ThanhToanViewModel model)
    {
        string orderId = $"{model.MaHopDong}_{model.KyHan}";
        string orderInfo = $"Thanh toán lãi kỳ {model.KyHan} hợp đồng {model.MaHopDong}";
        string returnUrl = Url.Action("MoMoReturn", "Payments", null, Request.Scheme);
        string notifyUrl = Url.Action("MoMoNotify", "Payments", null, Request.Scheme);

        string payUrl = await _momoService.CreatePaymentAsync(orderId, (long)model.SoTienPhaiTra, orderInfo, returnUrl, notifyUrl);
        return Redirect(payUrl);
    }

    // Khi khách được redirect về sau khi thanh toán (có thể kiểm tra trạng thái, hiển thị thông báo)
    public IActionResult MoMoReturn()
    {
        // Kiểm tra resultCode, orderId, update trạng thái đơn lãi
        // var resultCode = Request.Query["resultCode"];
        // if (resultCode == "0") => Thanh toán thành công
        return View();
    }

    // Webhook IPN từ MoMo (nhận trạng thái thực sự)
    [HttpPost]
    public IActionResult MoMoNotify()
    {
        // Đọc body json, kiểm tra signature, cập nhật trạng thái DB (model.TrangThai = "Đã thanh toán")
        // Gợi ý: log lại toàn bộ dữ liệu nhận được để tra cứu khi cần
        return Ok();
    }
    [HttpPost]
    public async Task<IActionResult> ThucHien(ThanhToanViewModel model)
    {
        if (model.PhuongThuc == "Momo")
        {
            // Tạo orderId duy nhất cho kỳ trả
            string orderId = $"{model.MaHopDong}_{model.KyHan}";
            string orderInfo = $"Thanh toán lãi kỳ {model.KyHan} hợp đồng {model.MaHopDong}";
            string returnUrl = Url.Action("MoMoReturn", "Payments", null, Request.Scheme);
            string notifyUrl = Url.Action("MoMoNotify", "Payments", null, Request.Scheme);

            // Gọi service MoMo
            var payUrl = await _momoService.CreatePaymentAsync(orderId, (long)model.SoTienPhaiTra, orderInfo, returnUrl, notifyUrl);
            return Redirect(payUrl);
        }
        else if (model.PhuongThuc == "VNPAY")
        {
            // Tương tự, gọi qua service VNPAY
            // return Redirect(vnpayUrl);
            return Content("Tích hợp VNPAY sẽ bổ sung sau.");
        }
        else
        {
            return BadRequest();
        }
    }

}
