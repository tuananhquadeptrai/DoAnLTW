using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VAYTIEN.Models;
using VAYTIEN.Services;

namespace VAYTIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ có Admin cấp cao nhất mới có quyền này
    public class CauHinhTinDungController : Controller
    {
        private readonly QlvayTienContext _context;
        private readonly IDiemTinDungService _diemTinDungService;

        public CauHinhTinDungController(QlvayTienContext context, IDiemTinDungService diemTinDungService)
        {
            _context = context;
            _diemTinDungService = diemTinDungService;
        }

        // GET: /Admin/CauHinhTinDung
        // Hiển thị danh sách tất cả khách hàng và điểm tín dụng của họ
        public async Task<IActionResult> Index()
        {
            var allCustomers = await _context.KhachHangs
                                        .OrderBy(kh => kh.HoTen)
                                        .ToListAsync();
            return View(allCustomers);
        }

        // POST: /Admin/CauHinhTinDung/ChamDiemLai/5
        // Xử lý khi Admin bấm nút "Chấm điểm lại"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamDiemLai(int maKh)
        {
            var khachHang = await _context.KhachHangs.FindAsync(maKh);
            if (khachHang == null)
            {
                TempData["Error"] = "Không tìm thấy khách hàng để chấm điểm.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Gọi service để thực hiện chấm điểm
                await _diemTinDungService.CapNhatDiemVaHanMucAsync(maKh);
                TempData["Success"] = $"Đã chấm điểm lại thành công cho khách hàng: {khachHang.HoTen}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Đã có lỗi xảy ra trong quá trình chấm điểm: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
