using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VAYTIEN.Models;

namespace VAYTIEN.Controllers
{
    public class TaiKhoanNganHangController : Controller
    {
        private readonly QlvayTienContext _context;

        public TaiKhoanNganHangController(QlvayTienContext context)
        {
            _context = context;
        }

        // GET: /TaiKhoanNganHang/
        public async Task<IActionResult> Index()
        {
            var danhSachThe = await _context.TaiKhoanNganHangs
                .Include(tk => tk.MaKhNavigation) // load cả thông tin khách hàng
                .ToListAsync();

            return View(danhSachThe);
        }
    }
}
