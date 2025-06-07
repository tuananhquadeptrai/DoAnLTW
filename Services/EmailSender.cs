using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;

namespace VAYTIEN.Services
{
    public class EmailSender
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body, string? attachmentPath = null)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("Son111333na@gmail.com")); 
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };

            if (!string.IsNullOrEmpty(attachmentPath))
            {
                builder.Attachments.Add(attachmentPath);
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync("son111333na@gmail.com", "dizc wdmo vzlc tbwp"); 
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
