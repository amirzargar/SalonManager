using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace SalonManager.EmailServices
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine($"[FAKE EMAIL] To: {email}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {htmlMessage}");
            return Task.CompletedTask;
        }
    }
}
