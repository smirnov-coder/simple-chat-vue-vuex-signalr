using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SimpleChat.Services
{
    public class MockEmailService : EmailService
    {
        private readonly string _emailFolder = @"D:\Emails";

        public MockEmailService(IConfiguration configuration) : base(configuration)
        {
            if (!Directory.Exists(_emailFolder))
                Directory.CreateDirectory(_emailFolder);
        }

        public override async Task SendEmailAsync(string recipientName, string recipientEmail, string subject,
            string text)
        {
            using (var message = new MailMessage())
            {
                message.From = new MailAddress("admin@example.com");
                message.To.Add(new MailAddress(recipientEmail, recipientName));
                message.Subject = subject;
                message.SubjectEncoding = Encoding.UTF8;
                message.Body = text;
                message.BodyTransferEncoding = System.Net.Mime.TransferEncoding.EightBit;

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    smtpClient.PickupDirectoryLocation = _emailFolder;
                    await smtpClient.SendMailAsync(message);
                }
            }
        }
    }
}
