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
    private readonly IConfiguration _configuration; // Inject IConfiguration để đọc appsettings.json

    public ThanhToanController(QlvayTienContext context, MoMoService momoService, IConfiguration configuration)
    {
        _context = context;
        _momoService = momoService;
        _configuration = configuration; // Gán giá trị
    }

    // GET: /ThanhToan/ChiTiet?maHopDong=11&kyHanThu=1
    public async Task<IActionResult> ChiTiet(int maHopDong, int kyHanThu)
    {
        var lichTra = await _context.LichTraNos
            .Include(l => l.MaHopDongNavigation.MaKhNavigation)
            .FirstOrDefaultAsync(x => x.MaHopDong == maHopDong && x.KyHanThu == kyHanThu);

        if (lichTra?.MaHopDongNavigation?.MaKhNavigation == null) return NotFound();
        if (lichTra.MaHopDongNavigation.MaKhNavigation.Email != User.Identity.Name) return Forbid();

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
        var lichTra = await _context.LichTraNos.AsNoTracking()
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
                // Sửa lỗi trùng lặp: Thêm một chuỗi ngẫu nhiên để orderId luôn là duy nhất
                string orderId = $"{model.MaHopDong}_{model.KyHan}_{Guid.NewGuid().ToString().Substring(0, 8)}";

                string orderInfo = $"Thanh toan HD#{model.MaHopDong} Ky#{model.KyHan}";
                string returnUrl = Url.Action("MoMoReturn", "ThanhToan", null, Request.Scheme)!;
                string notifyUrl = Url.Action("MoMoNotify", "ThanhToan", null, Request.Scheme)!;
                var payUrl = await _momoService.CreatePaymentAsync(orderId, (long)model.SoTienPhaiTra, orderInfo, returnUrl, notifyUrl);
                return Redirect(payUrl);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể kết nối với MoMo: " + ex.Message;
                return RedirectToAction("ChiTiet", new { maHopDong = model.MaHopDong, kyHanThu = model.KyHan });
            }
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
                TempData["Success"] = "Thanh toán thành công! Dư nợ và số dư tài khoản của bạn đã được cập nhật.";
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


    // =========================================================================
    // HÀM DÙNG CHUNG: Nơi tập trung toàn bộ logic xử lý sau khi thanh toán thành công
    // =========================================================================
    private async Task<bool> ProcessSuccessfulPaymentAndUpdateDbAsync(string orderId)
    {
        // Bắt đầu một transaction để đảm bảo tất cả các thao tác đều thành công
        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var ids = orderId.Split('_');
            if (ids.Length < 2) return false;

            int maHopDong = int.Parse(ids[0]);
            int kyHan = int.Parse(ids[1]);

            var lichTra = await _context.LichTraNos
                .FirstOrDefaultAsync(x => x.MaHopDong == maHopDong && x.KyHanThu == kyHan);

            // Kiểm tra xem đã xử lý chưa để tránh cập nhật 2 lần
            if (lichTra != null && lichTra.TrangThai != "Đã trả")
            {
                // 1. CẬP NHẬT TRẠNG THÁI KỲ TRẢ NỢ CỦA KHÁCH HÀNG
                lichTra.TrangThai = "Đã trả";
                lichTra.NgayTra = DateOnly.FromDateTime(DateTime.Now);

                // 2. TRỪ NỢ GỐC CÒN LẠI CỦA KHÁCH HÀNG
                var hopDong = await _context.HopDongVays.FindAsync(maHopDong);
                if (hopDong != null && lichTra.SoTienPhaiTra.HasValue) // Chỉ trừ đi số tiền GỐC
                {
                    // Lần đầu thanh toán, gán SoTienConLai bằng SoTienVay ban đầu
                    if (hopDong.SoTienConLai == null)
                    {
                        hopDong.SoTienConLai = hopDong.SoTienVay;
                    }
                    // Trừ đi số tiền GỐC của kỳ này
                    hopDong.SoTienConLai -= lichTra.SoTienPhaiTra.Value;
                }

                // 3. CỘNG TIỀN VÀO TÀI KHOẢN CỦA ADMIN/CÔNG TY
                var companyAccountNumber = _configuration["AppSettings:CompanyBankAccountNumber"];
                var companyAccount = await _context.TaiKhoanNganHangs
                                           .FirstOrDefaultAsync(tk => tk.SoTaiKhoan == companyAccountNumber);

                if (companyAccount != null && lichTra.SoTienPhaiTra.HasValue)
                {
                    companyAccount.SoDu += lichTra.SoTienPhaiTra.Value;
                }

                // 4. TẠO GHI NHẬN GIAO DỊCH VÀO LỊCH SỬ
                var newTransaction = new GiaoDich
                {
                    MaTaiKhoan = companyAccount?.MaTaiKhoan,
                    NgayGd = DateOnly.FromDateTime(DateTime.Now),
                    SoTienGd = lichTra.SoTienPhaiTra,
                    LoaiGd = "Thu nợ",
                    NoiDungGd = $"Khách hàng thanh toán kỳ {kyHan} cho HĐ #{maHopDong}"
                };
                _context.GiaoDiches.Add(newTransaction);

                // Lưu tất cả các thay đổi vào DB
                await _context.SaveChangesAsync();

                // Nếu mọi thứ ổn, commit transaction
                await dbTransaction.CommitAsync();

                return true;
            }
        }
        catch (Exception)
        {
            // Nếu có bất kỳ lỗi nào, rollback tất cả các thay đổi
            await dbTransaction.RollbackAsync();
            return false;
        }
        // Trả về false nếu kỳ trả nợ đã được xử lý từ trước
        return false;
    }
}
