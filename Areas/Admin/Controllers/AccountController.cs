using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VAYTIEN.Models;

namespace VAYTIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly QlvayTienContext _context;

        public AccountController(QlvayTienContext context)
        {
            _context = context;
        }

        // GET: /Admin/KhachHang
        public IActionResult TaiKhoan()
        {
            var danhSach = _context.KhachHangs
                .Include(kh => kh.TaiKhoanNganHangs)
                .ToList();
            return View(danhSach);
        }
    }
}
