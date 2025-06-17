using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace VAYTIEN.Services
{
    public interface ICloudinaryService
    {
        Task<string?> UploadImageAsync(IFormFile file); // Trả về URL của ảnh đã upload
        Task<bool> DeleteImageAsync(string publicId); // Xóa ảnh bằng publicId (thường là một phần của URL)
        string? ParsePublicIdFromCloudinaryUrl(string? imageUrl);
    }
}