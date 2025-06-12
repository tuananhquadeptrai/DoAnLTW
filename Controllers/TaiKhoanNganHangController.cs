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
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound();
            }

            // Kiểm tra bảo mật: người dùng chỉ có thể sửa thông tin của chính họ
            var userEmail = User.Identity?.Name;
            if (khachHang.Email != userEmail && !User.IsInRole("Admin"))
            {
                return Forbid(); // Trả về lỗi 403 Forbidden
            }

            return View(khachHang);
        }


        // POST: /KhachHang/Edit/5
        // Action này sẽ được gọi khi người dùng bấm nút "Lưu thay đổi" trong form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaKh,HoTen,CmndCccd,NgaySinh,DiaChi,Sdt,Email,NgheNghiep,TinhTrangHonNhan,AnhDinhKem")] KhachHang khachHang, IFormFile? anhFile)
        {
            if (id != khachHang.MaKh)
            {
                return NotFound();
            }

            // Kiểm tra bảo mật lại một lần nữa ở phía server
            var userEmail = User.Identity?.Name;
            if (khachHang.Email != userEmail && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Xóa lỗi của trường DoiTuongVayMaDoiTuongVay vì nó không có trong form Edit
            ModelState.Remove("DoiTuongVayMaDoiTuongVay");

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload ảnh mới nếu người dùng có chọn file
                    if (anhFile != null && anhFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(anhFile.FileName);
                        var filePath = Path.Combine("wwwroot", "uploads", fileName);

                        // Đảm bảo thư mục "uploads" tồn tại
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await anhFile.CopyToAsync(stream);
                        }
                        // Cập nhật đường dẫn ảnh mới cho khách hàng
                        khachHang.AnhDinhKem = "/uploads/" + fileName;
                    }

                    _context.Update(khachHang);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Cập nhật thông tin cá nhân thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.KhachHangs.Any(e => e.MaKh == khachHang.MaKh))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Chuyển hướng về trang thông tin cá nhân với ID của khách hàng
                return RedirectToAction(nameof(ThongTinCaNhan), new { id = khachHang.MaKh });
            }

            // Nếu dữ liệu nhập không hợp lệ, quay lại form Edit để hiển thị lỗi
            return View(khachHang);
        }



    }
}
