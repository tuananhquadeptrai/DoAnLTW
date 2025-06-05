using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VAYTIEN.Models // 💡 namespace phải giống với DbContext của bạn
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public string? Address { get; set; }
        public string? Age { get; set; }
        public string? CCCD { get; set; }
        public string? VaiTro { get; set; } // Admin, NhanVien, KhachHang
    }
}
