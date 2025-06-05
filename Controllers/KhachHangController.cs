// Controller quản lý khách hàng vay tiền
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using VAYTIEN.Models;

namespace VAYTIEN.Controllers
{
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStep1(KhachHang kh, IFormFile? anhFile)
        {
            if (!ModelState.IsValid) return View(kh);

            // Upload ảnh
            if (anhFile != null && anhFile.Length > 0)
            {
                var fileName = Path.GetFileName(anhFile.FileName);
                var filePath = Path.Combine("wwwroot/uploads", fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await anhFile.CopyToAsync(stream);
                kh.AnhDinhKem = "/uploads/" + fileName;
            }

            // Lưu tạm thông tin vào TempData (serialize JSON)
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

            // Lưu khách hàng mới
            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            // Gắn khách hàng vào hợp đồng
            hopDong.MaKh = khachHang.MaKh;

            // ✅ Gán trạng thái chờ phê duyệt
            hopDong.TinhTrang = "Chờ phê duyệt"; // ✅ Đúng tên property


            _context.HopDongVays.Add(hopDong);
            await _context.SaveChangesAsync();

            // Gắn tài sản thế chấp vào hợp đồng
            taiSan.MaHopDong = hopDong.MaHopDong;
            _context.TaiSanTheChaps.Add(taiSan);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home"); // hoặc trang cảm ơn
        }

    }

}
