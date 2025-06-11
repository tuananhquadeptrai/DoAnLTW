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
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaiKhoanNganHang model)
        {
            var userId = User.Identity?.Name;
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.Email == userId);

            if (khachHang == null)
                return RedirectToAction("CreateStep1", "KhachHang");

            model.MaKh = khachHang.MaKh;
            _context.TaiKhoanNganHangs.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.Identity?.Name;

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(kh => kh.Email == userId);

            if (khachHang == null)
            {
                // Không tìm thấy khách hàng → có thể là nhân viên, hoặc chưa đăng ký thông tin
                return RedirectToAction("ThongTinCaNhan", "KhachHang");
            }

            // Lấy các thẻ ngân hàng thuộc khách hàng
            var danhSachThe = await _context.TaiKhoanNganHangs
                .Where(tk => tk.MaKh == khachHang.MaKh)
                .Include(tk => tk.MaKhNavigation)
                .ToListAsync();

            if (danhSachThe.Count == 0)
            {
                return RedirectToAction("Create", "TaiKhoanNganHang");
            }

            return View(danhSachThe);
        }
        public async Task<IActionResult> ThongTinCaNhan()
        {
            var userId = User.Identity?.Name;

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(kh => kh.Email == userId);

            return View(khachHang); // dù khachHang là null cũng trả về view
        }


    }
}
