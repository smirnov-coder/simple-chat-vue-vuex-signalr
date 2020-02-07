using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using SimpleChat.Infrastructure.Helpers;

namespace SimpleChat.Services
{
    public class EmailService : IEmailService, IDisposable
    {
        private IConfiguration _configuration;
        private IGuard _guard;

        private SmtpClient _smtpClient = new SmtpClient();
        public virtual SmtpClient SmtpClient
        {
            get => _smtpClient;
            set => _smtpClient = _guard.EnsureObjectParamIsNotNull(value, nameof(SmtpClient));
        }

        private readonly string _address;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _password;

        public EmailService(IConfiguration configuration, IGuard guard = null)
        {
            _guard = guard ?? new Guard();
            _configuration = _guard.EnsureObjectParamIsNotNull(configuration, nameof(configuration));

            _address = EnsureValueExists("Email:Address");
            _userName = EnsureValueExists("Email:UserName");
            _password = EnsureValueExists("Email:Password");
            string portKey = "Email:Port";
            int.TryParse(EnsureValueExists(portKey), out _port);
            if (_port <= 0)
            {
                throw new InvalidOperationException($"Значение для ключа '{portKey}' должно быть целым " +
                    $"положительным числом.");
            }
        }

        private string EnsureValueExists(string key)
        {
            string value = _configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Отсутствует значение для ключа '{key}' в файле настроек " +
                    $"приложения (appsettings.json).");
            }
            return value;
        }

        public void Dispose()
        {
            ((IDisposable)_smtpClient)?.Dispose();
        }

        public virtual string CreateSignInConfirmationEmail(string name, string provider, string appUrl, string code)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(name, nameof(name));
            _guard.EnsureStringParamIsNotNullOrEmpty(provider, nameof(provider));
            _guard.EnsureStringParamIsNotNullOrEmpty(appUrl, nameof(appUrl));
            _guard.EnsureStringParamIsNotNullOrEmpty(code, nameof(code));

            return $"Здравствуйте, {name}.{Environment.NewLine}Ваш код для добавления входа на сайт " +
                $"<a href={appUrl}>SimpleChat</a> через Ваш аккаунт в {provider}: {code}";
        }

        public virtual async Task SendEmailAsync(string recipientName, string recipientAddress, string subject,
            string text)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(recipientName, nameof(recipientName));
            _guard.EnsureStringParamIsNotNullOrEmpty(recipientAddress, nameof(recipientAddress));
            _guard.EnsureStringParamIsNotNullOrEmpty(subject, nameof(subject));
            _guard.EnsureStringParamIsNotNullOrEmpty(text, nameof(text));

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("SimpleChat", _address));
            message.To.Add(new MailboxAddress(recipientName, recipientAddress));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Plain)
            {
                Text = text
            };

            await _smtpClient.ConnectAsync(_address, _port, SecureSocketOptions.Auto, default);
            await _smtpClient.AuthenticateAsync(Encoding.UTF8, new NetworkCredential(_userName, _password), default);
            await _smtpClient.SendAsync(message, default, default);
            await _smtpClient.DisconnectAsync(true, default);
        }
    }
}
