using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VAYTIEN.Models;
using System.Linq;
using System.Threading.Tasks;

namespace VAYTIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,NhanVien")]
    public class TaiKhoanController : Controller
    {
        private readonly QlvayTienContext _context;

        public TaiKhoanController(QlvayTienContext context)
        {
            _context = context;
        }

        // GET: Admin/TaiKhoan
        // Hiển thị trang danh sách tất cả tài khoản của khách hàng
        public async Task<IActionResult> Index(
            string? searchString, // Tên khách hàng / Số tài khoản
            string? statusFilter, // Lọc theo trạng thái
            string? typeFilter,   // Lọc theo loại tài khoản
            decimal? minBalance    // Lọc theo số dư tối thiểu
        )
        {
            var customerAccounts = _context.TaiKhoanNganHangs
                                           .Where(tk => tk.MaKh != null) // Chỉ lấy tài khoản của khách hàng
                                           .Include(tk => tk.MaKhNavigation) // Lấy kèm thông tin khách hàng
                                           .AsQueryable(); // Bắt đầu với IQueryable để xây dựng truy vấn

            // Áp dụng tìm kiếm theo searchString (HoTen hoặc SoTaiKhoan)
            if (!string.IsNullOrEmpty(searchString))
            {
                customerAccounts = customerAccounts.Where(tk =>
                    tk.MaKhNavigation!.HoTen.Contains(searchString) || // HoTen của khách hàng
                    tk.SoTaiKhoan.Contains(searchString)                // Hoặc số tài khoản
                );
            }

            // Áp dụng lọc theo trạng thái
            if (!string.IsNullOrEmpty(statusFilter))
            {
                customerAccounts = customerAccounts.Where(tk => tk.TrangThai == statusFilter);
            }

            // Áp dụng lọc theo loại tài khoản
            if (!string.IsNullOrEmpty(typeFilter))
            {
                customerAccounts = customerAccounts.Where(tk => tk.LoaiTaiKhoan == typeFilter);
            }

            // Áp dụng lọc theo số dư tối thiểu
            if (minBalance.HasValue)
            {
                customerAccounts = customerAccounts.Where(tk => tk.SoDu >= minBalance.Value);
            }

            // Sắp xếp kết quả (nên sắp xếp cuối cùng sau khi lọc)
            customerAccounts = customerAccounts.OrderBy(tk => tk.MaKhNavigation!.HoTen); // Sắp xếp theo tên khách hàng

            // Thực thi truy vấn và lấy dữ liệu
            var finalAccounts = await customerAccounts.ToListAsync();

            // Truyền các giá trị lọc hiện tại về View để giữ trạng thái trên form
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentStatusFilter = statusFilter;
            ViewBag.CurrentTypeFilter = typeFilter;
            ViewBag.CurrentMinBalance = minBalance;

            return View(finalAccounts);
        }
    }
}