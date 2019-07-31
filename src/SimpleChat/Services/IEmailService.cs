using System.Threading.Tasks;

namespace SimpleChat.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string recipientName, string recipientEmail, string subject, string text);

        string CreateSignInConfirmationEmail(string name, string provider, string appUrl, string code);
    }
}
