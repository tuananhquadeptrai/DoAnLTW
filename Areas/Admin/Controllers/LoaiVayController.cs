using Microsoft.AspNetCore.Mvc;
using VAYTIEN.Models;

namespace VAYTIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoaiVayController : Controller
    {
        private readonly QlvayTienContext _context;

        public LoaiVayController(QlvayTienContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var list = _context.LoaiVays.ToList();
            return View(list);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LoaiVay loai)
        {
            if (!ModelState.IsValid) return View(loai);
            _context.LoaiVays.Add(loai);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var item = _context.LoaiVays.Find(id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(LoaiVay loai)
        {
            if (!ModelState.IsValid) return View(loai);
            _context.LoaiVays.Update(loai);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var loai = _context.LoaiVays.Find(id);
            if (loai != null)
            {
                _context.LoaiVays.Remove(loai);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
