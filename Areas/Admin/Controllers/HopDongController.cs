using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VAYTIEN.Models;

namespace VAYTIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HopDongController : Controller
    {
        private readonly QlvayTienContext _context;

        public HopDongController(QlvayTienContext context)
        {
            _context = context;
        }

        public IActionResult ChoPheDuyet()
        {
            var list = _context.HopDongVays
                .Include(h => h.MaKhNavigation)
                .Where(h => h.TinhTrang == "Chờ phê duyệt")
                .ToList();

            return View(list);
        }

        // POST: Duyệt hợp đồng
        [HttpPost]
        public IActionResult PheDuyet(int id)
        {
            var hd = _context.HopDongVays.Find(id);
            if (hd == null) return NotFound();

            hd.TinhTrang = "Đã duyệt";
            _context.SaveChanges();

            return RedirectToAction("ChoPheDuyet");
        }
    }
}
