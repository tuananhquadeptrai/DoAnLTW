using Microsoft.AspNetCore.Mvc;
using VAYTIEN.Models; // nếu cần DbContext

namespace VAYTIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly QlvayTienContext _context;

        public DashboardController(QlvayTienContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.TotalCustomers = _context.KhachHangs.Count();
            ViewBag.TotalContracts = _context.HopDongVays.Count();
            ViewBag.TotalPending = _context.HopDongVays.Count(h => h.TinhTrang == "Chờ phê duyệt");
            return View();
        }
        public IActionResult TimKiem(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                TempData["Error"] = "Vui lòng nhập từ khóa tìm kiếm.";
                return RedirectToAction("Index");
            }

            var ketQua = _context.KhachHangs
                .Where(kh => kh.HoTen != null && kh.HoTen.Contains(keyword))
                .ToList();

            ViewBag.Keyword = keyword;
            return View("TongHopDong", ketQua); 
        }

    }
}
