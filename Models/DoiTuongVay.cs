using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAYTIEN.Models
{
    public class DoiTuongVay
    {
        [Key]
        public int MaDoiTuongVay { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenDoiTuong { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal LaiSuat { get; set; }

        // Navigation property: Một đối tượng vay có thể có nhiều khách hàng
        public ICollection<KhachHang> KhachHangs { get; set; }
    }
}
