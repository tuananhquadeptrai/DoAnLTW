// Đặt ở đầu file
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity.UI.Services // <--- QUAN TRỌNG: Namespace này phải đúng
{
    // Giao diện này được định nghĩa bởi ASP.NET Core Identity
    // và là cách mà Identity yêu cầu dịch vụ gửi email của bạn phải tuân thủ.
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}