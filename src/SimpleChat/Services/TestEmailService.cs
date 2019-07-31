using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SimpleChat.Services
{
    public class TestEmailService : EmailService
    {
        public TestEmailService(IConfiguration configuration) : base(configuration) { }

        public override async Task SendEmailAsync(string recipientName, string recipientEmail, string subject, string text)
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
                    smtpClient.PickupDirectoryLocation = @"D:\Emails";
                    await smtpClient.SendMailAsync(message);
                }
            }
        }
    }
}
