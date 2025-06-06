using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using VAYTIEN.Models;

namespace VAYTIEN.Controllers
{
    [Authorize]
    public class KhachHangController : Controller
    {
        private readonly QlvayTienContext _context;

        public KhachHangController(QlvayTienContext context)
        {
            _context = context;
        }

        // Bước 1 - Form thông tin cá nhân
        public IActionResult CreateStep1()
        {
            ViewBag.DoiTuongVayList = _context.DoiTuongVays.ToList();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep1(KhachHang kh, IFormFile? anhFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.DoiTuongVayList = _context.DoiTuongVays.ToList(); // Đảm bảo form không bị mất dropdown
                return View(kh);
            }

            if (kh.DoiTuongVayMaDoiTuongVay == null || kh.DoiTuongVayMaDoiTuongVay == 0)
            {
                ModelState.AddModelError("DoiTuongVayMaDoiTuongVay", "Vui lòng chọn đối tượng vay");
                ViewBag.DoiTuongVayList = _context.DoiTuongVays.ToList();
                return View(kh);
            }

            // Upload ảnh
            if (anhFile != null && anhFile.Length > 0)
            {
                var fileName = Path.GetFileName(anhFile.FileName);
                var filePath = Path.Combine("wwwroot/uploads", fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await anhFile.CopyToAsync(stream);
                kh.AnhDinhKem = "/uploads/" + fileName;
            }

            // Lưu tạm vào TempData
            TempData["KhachHang"] = JsonSerializer.Serialize(kh);
            return RedirectToAction("CreateStep2");
        }


        // Bước 2 - Form thông tin vay
        public IActionResult CreateStep2()
        {
            ViewBag.LoaiVayList = _context.LoaiVays.ToList();
            ViewBag.LoaiTienTeList = _context.LoaiTienTes.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep2(HopDongVay hopDong, TaiSanTheChap taiSan)
        {
            if (TempData["KhachHang"] == null)
                return RedirectToAction("CreateStep1");

            var khachHang = JsonSerializer.Deserialize<KhachHang>(TempData["KhachHang"]!.ToString()!);

            // Lưu khách hàng
            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            hopDong.MaKh = khachHang.MaKh;
            hopDong.TinhTrang = "Chờ phê duyệt";

            _context.HopDongVays.Add(hopDong);
            await _context.SaveChangesAsync();

            taiSan.MaHopDong = hopDong.MaHopDong;
            _context.TaiSanTheChaps.Add(taiSan);
            await _context.SaveChangesAsync();

            return RedirectToAction("XacNhan");
        }

        // ✅ Trang xác nhận thành công
        public IActionResult XacNhan()
        {
            return View();
        }
    }
}
