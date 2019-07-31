using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace SimpleChat.Services
{
    public class EmailService : IEmailService, IDisposable
    {
        private IConfiguration _configuration;
        private SmtpClient _smtpClient = new SmtpClient();

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Dispose()
        {
            ((IDisposable)_smtpClient).Dispose();
        }

        public virtual string CreateSignInConfirmationEmail(string name, string provider, string appUrl, string code)
        {
            return $"Здравствуйте, {name}. {Environment.NewLine}" +
                $"Ваш код для добавления входа на сайт <a href={appUrl}>SimpleChat</a> через Ваш аккаунт в {provider}: {code}";
        }

        public virtual async Task SendEmailAsync(
            string recipientName,
            string recipientEmail,
            string subject,
            string text)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("SimpleChat", _configuration["Email:Address"]));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Plain)
            {
                Text = text
            };

            await _smtpClient.ConnectAsync(_configuration["Email:Address"], int.Parse(_configuration["Email:Port"]), false);
            await _smtpClient.AuthenticateAsync(_configuration["Email:UserName"], _configuration["Email:Password"]);
            await _smtpClient.SendAsync(message);
            await _smtpClient.DisconnectAsync(true);
        }
    }
}
