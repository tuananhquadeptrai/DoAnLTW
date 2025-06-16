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
                // PublicId thường là phần cuối của URL sau /upload/ hoặc /v.../
                // Ví dụ: nếu URL là https://res.cloudinary.com/your_cloud_name/image/upload/v12345/folder/image_name.png
                // thì publicId là folder/image_name
                // QuestPDF.GeneratePaymentReceiptPdf trả về full path, bạn cần parse publicId từ đó.

                // Để lấy publicId từ SecureUrl: Cần Cloudinary URL Parser hoặc đơn giản là cắt chuỗi.
                // Nếu URL có dạng .../upload/publicId.ext thì publicId là phần sau /upload/
                var parts = publicId.Split(new[] { "/upload/" }, StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    string fullPublicId = parts[1];
                    // Loại bỏ phần mở rộng .ext và version number (v12345/) nếu có
                    int lastDotIndex = fullPublicId.LastIndexOf('.');
                    if (lastDotIndex != -1)
                    {
                        fullPublicId = fullPublicId.Substring(0, lastDotIndex);
                    }
                    if (fullPublicId.Contains("/v")) // Remove version (e.g., v12345/)
                    {
                        fullPublicId = fullPublicId.Substring(fullPublicId.LastIndexOf('/') + 1);
                    }
                    else if (fullPublicId.Contains("vaytien_app_uploads/")) // remove folder name
                    {
                        fullPublicId = fullPublicId.Replace("vaytien_app_uploads/", "");
                    }
                    publicId = fullPublicId;
                }
                else
                {
                    _logger.LogWarning($"Could not parse publicId from URL: {publicId}. Assuming it's already a publicId.");
                }


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
    }
}