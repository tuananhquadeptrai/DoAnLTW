using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAYTIEN.Models
{
    /// <summary>
    /// Đại diện cho một ảnh đính kèm cụ thể của một khách hàng.
    /// </summary>
    public class KhachHangAnh
    {
        [Key]
        public int MaAnh { get; set; }

        [Required]
        public int MaKh { get; set; } // Khóa ngoại đến KhachHang

        [Required(ErrorMessage = "Đường dẫn ảnh không được để trống.")]
        [StringLength(500)]
        [Display(Name = "URL Ảnh")]
        public string Url { get; set; } // URL của ảnh từ Cloudinary

        [StringLength(255)]
        [Display(Name = "Public ID Cloudinary")]
        public string? PublicId { get; set; } // ID duy nhất của ảnh trên Cloudinary (để xóa)

        [StringLength(100)]
        [Display(Name = "Loại ảnh")] // Ví dụ: "CMND Mặt trước", "CMND Mặt sau", "Chân dung"
        public string? LoaiAnh { get; set; }

        [Display(Name = "Ngày tải lên")]
        public DateTime NgayTaiLen { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [ForeignKey("MaKh")]
        public virtual KhachHang? KhachHang { get; set; } // Một ảnh thuộc về một KhachHang
    }
}