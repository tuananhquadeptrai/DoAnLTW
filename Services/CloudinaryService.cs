using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Thêm để log lỗi

namespace VAYTIEN.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger; // Khai báo logger

        public CloudinaryService(IConfiguration config, ILogger<CloudinaryService> logger) // Inject ILogger
        {
            _logger = logger;
            var cloudinarySettings = config.GetSection("CloudinarySettings");
            var account = new Account(
                cloudinarySettings["CloudName"],
                cloudinarySettings["ApiKey"],
                cloudinarySettings["ApiSecret"]
            );

            _cloudinary = new CloudinaryDotNet.Cloudinary(account);
        }

        public async Task<string?> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            try
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    // Tùy chỉnh (ví dụ: tối ưu hóa, resize)
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face").Quality("auto"),
                    Folder = "vaytien_app_uploads" // Thư mục trên Cloudinary để tổ chức file
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation($"Successfully uploaded image to Cloudinary: {uploadResult.Url}");
                    return uploadResult.SecureUrl.ToString(); // Trả về URL an toàn (HTTPS)
                }
                else
                {
                    _logger.LogError($"Failed to upload image to Cloudinary. Status Code: {uploadResult.StatusCode}, Error: {uploadResult.Error?.Message}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while uploading image to Cloudinary.");
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
            {
                return false;
            }

            try
            {
                // Logic lấy publicId từ URL đã được CloudinaryService xử lý nội bộ hoặc từ Controller gọi
                // Bạn cần đảm bảo publicId được truyền vào đây là chính xác
                // Nếu publicId truyền vào đây vẫn là full URL, bạn sẽ cần logic phân tích nó ở đây hoặc đảm bảo caller đã parse nó.
                // Dựa trên KhachHangController, bạn gọi ParsePublicIdFromCloudinaryUrl(newImageUrl) TRONG CONTROLLER.
                // Vậy nên publicId truyền vào DeleteImageAsync NÊN LÀ ĐÚNG PublicId đã parse.

                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Image // Hoặc ResourceType.Raw nếu là PDF/other files
                };

                var result = await _cloudinary.DestroyAsync(deleteParams);

                if (result.StatusCode == System.Net.HttpStatusCode.OK && result.Result == "ok")
                {
                    _logger.LogInformation($"Successfully deleted image from Cloudinary: {publicId}");
                    return true;
                }
                else
                {
                    _logger.LogError($"Failed to delete image from Cloudinary: {publicId}. Status: {result.StatusCode}, Error: {result.Error?.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting image from Cloudinary: {publicId}.");
                return false;
            }
        }

        // BỔ SUNG DÒNG NÀY: TRIỂN KHAI PHƯƠNG THỨC ParsePublicIdFromCloudinaryUrl TỪ GIAO DIỆN ICloudinaryService
        public string? ParsePublicIdFromCloudinaryUrl(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return null;

            try
            {
                var uri = new Uri(imageUrl);
                var path = uri.AbsolutePath; // Kết quả: /your_cloud_name/image/upload/v12345/folder/image_name.png

                var uploadIndex = path.IndexOf("/upload/", StringComparison.OrdinalIgnoreCase);
                if (uploadIndex == -1) return null;

                var publicIdWithVersion = path.Substring(uploadIndex + "/upload/".Length);

                // Loại bỏ phần version (nếu có, dạng v12345/)
                var versionIndex = publicIdWithVersion.IndexOf("/v", StringComparison.OrdinalIgnoreCase);
                if (versionIndex != -1 && (publicIdWithVersion.Length > versionIndex + 1 && Char.IsDigit(publicIdWithVersion[versionIndex + 1])))
                {
                    var nextSlashIndex = publicIdWithVersion.IndexOf('/', versionIndex + 1);
                    if (nextSlashIndex != -1)
                    {
                        publicIdWithVersion = publicIdWithVersion.Substring(nextSlashIndex + 1);
                    }
                }

                // Loại bỏ phần mở rộng file (.png, .jpg, ...)
                var lastDotIndex = publicIdWithVersion.LastIndexOf('.');
                if (lastDotIndex != -1)
                {
                    publicIdWithVersion = publicIdWithVersion.Substring(0, lastDotIndex);
                }
                _logger.LogDebug($"Parsed publicId from {imageUrl}: {publicIdWithVersion}");
                return publicIdWithVersion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to parse publicId from URL: {imageUrl}");
                return null;
            }
        }
    }
}