using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Moq;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Services
{
    public class EmailServiceTests
    {
        private Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private Mock<IGuard> _mockGuard = new Mock<IGuard>();
        private Mock<SmtpClient> _mockSmtpClient = new Mock<SmtpClient>();
        private EmailService _target;

        private const string AddressKey = "Email:Address";
        private const string PortKey = "Email:Port";
        private const string UserNameKey = "Email:UserName";
        private const string PasswordKey = "Email:Password";
        private const string TestSenderAddress = "admin@example.com";

        public EmailServiceTests()
        {
            _mockConfiguration.Setup(x => x[AddressKey]).Returns(TestSenderAddress);
            _mockConfiguration.Setup(x => x[PortKey]).Returns("465"); // TLS smtp-port
            _mockConfiguration.Setup(x => x[UserNameKey]).Returns(TestConstants.TestUserName);
            _mockConfiguration.Setup(x => x[PasswordKey]).Returns(TestConstants.TestPassword);
            _mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockConfiguration.Object, "configuration"))
                .Returns(_mockConfiguration.Object);
            _mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockSmtpClient.Object, "SmtpClient"))
                .Returns(_mockSmtpClient.Object);
            
            _target = new EmailService(_mockConfiguration.Object, _mockGuard.Object)
            {
                SmtpClient = _mockSmtpClient.Object
            };
        }

        [Fact]
        public void Constructor_Good()
        {
            // arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupSequence(x => x[It.IsAny<string>()])
                .Returns(TestSenderAddress)
                .Returns(TestConstants.TestUserName)
                .Returns(TestConstants.TestPassword)
                .Returns("25");
            var mockGuard = new Mock<IGuard>();
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(It.IsAny<IConfiguration>(), It.IsAny<string>()))
                .Returns(mockConfiguration.Object);

            // act
            var target = new EmailService(mockConfiguration.Object, mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(mockConfiguration.Object, "configuration"), Times.Once);
            mockConfiguration.Verify(x => x[AddressKey], Times.Once());
            mockConfiguration.Verify(x => x[PortKey], Times.Once());
            mockConfiguration.Verify(x => x[UserNameKey], Times.Once());
            mockConfiguration.Verify(x => x[PasswordKey], Times.Once());
            Assert.NotNull(_target.SmtpClient);
        }

        [Theory]
        [InlineData(null, "a", "a", "1", AddressKey)]
        [InlineData("a", null, "a", "1", UserNameKey)]
        [InlineData("a", "a", null, "1", PasswordKey)]
        [InlineData("a", "a", "a", null, PortKey)]
        public void Constructor_Bad_InvalidOperationException_ConfigurationValueNotProvided(string address,
            string userName, string password, string port, string key)
        {
            // arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupSequence(x => x[It.IsAny<string>()])
                .Returns(address)
                .Returns(userName)
                .Returns(password)
                .Returns(port);
            var mockGuard = new Mock<IGuard>();
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(It.IsAny<IConfiguration>(), It.IsAny<string>()))
                .Returns(mockConfiguration.Object);

            // act
            var ex = Assert.Throws<InvalidOperationException>(
                () => new EmailService(mockConfiguration.Object, mockGuard.Object));

            // assert
            Assert.Equal($"Отсутствует значение для ключа '{key}' в файле настроек приложения (appsettings.json).",
                ex.Message);
        }

        [Theory]
        [InlineData("-101"), InlineData("0"), InlineData("abc")]
        public void Constructor_Bad_InvalidOperationException_InvalidPort(string port)
        {
            // arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupSequence(x => x[It.IsAny<string>()])
                .Returns(TestSenderAddress)
                .Returns(TestConstants.TestUserName)
                .Returns(TestConstants.TestPassword)
                .Returns(port);
            var mockGuard = new Mock<IGuard>();
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(It.IsAny<IConfiguration>(), It.IsAny<string>()))
                .Returns(mockConfiguration.Object);

            // act
            var ex = Assert.Throws<InvalidOperationException>(
                () => new EmailService(mockConfiguration.Object, mockGuard.Object));

            // assert
            Assert.Equal("Значение для ключа 'Email:Port' должно быть целым положительным числом.", ex.Message);
        }

        [Fact]
        public void CreateSignInConfirmationEmail_Good()
        {
            // act
            string result = _target.CreateSignInConfirmationEmail(TestConstants.TestName, TestConstants.TestProvider,
                TestConstants.TestUrl, TestConstants.TestCode);

            // assert
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(TestConstants.TestName, "name"), Times.Once());
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(TestConstants.TestProvider, "provider"),
                Times.Once());
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(TestConstants.TestUrl, "appUrl"), Times.Once());
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(TestConstants.TestCode, "code"), Times.Once());
            Assert.Equal($"Здравствуйте, test_name.{Environment.NewLine}Ваш код для добавления входа на сайт " +
                $"<a href=test_url>SimpleChat</a> через Ваш аккаунт в test_provider: test_code", result);
        }

        private const string TestRecipientAddress = "test@example.com";
        private const string TestRecipientName = "test_name";
        private const string TestSubject = "test_subject";
        private const string TestText = "test_text";

        [Fact]
        public async Task SendEmailAsync_Good()
        {
            // act
            await _target.SendEmailAsync(TestRecipientName, TestRecipientAddress, TestSubject, TestText);

            // assert
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(TestRecipientName, "recipientName"),
                Times.Once());
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(TestRecipientAddress, "recipientAddress"),
                Times.Once());
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(TestSubject, "subject"), Times.Once());
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(TestText, "text"), Times.Once());
            _mockSmtpClient.Verify(x => x.ConnectAsync(TestSenderAddress, 465, SecureSocketOptions.Auto, default),
                Times.Once());
            _mockSmtpClient.Verify(x => x.AuthenticateAsync(Encoding.UTF8, It.Is<NetworkCredential>(
                credential => VerifyCredential(credential)), default), Times.Once());
            _mockSmtpClient.Verify(x => x.SendAsync(It.Is<MimeMessage>(message => VerifyMessage(message)),
                default, default), Times.Once());
            _mockSmtpClient.Verify(x => x.DisconnectAsync(true, default), Times.Once());
        }

        private bool VerifyMessage(MimeMessage message)
        {
            return message.From.Count == 1
                && message.From.Contains(new MailboxAddress("SimpleChat", TestSenderAddress))
                && message.To.Count == 1
                && message.To.Contains(new MailboxAddress(TestRecipientName, TestRecipientAddress))
                && message.Subject == TestSubject
                && message.TextBody == TestText;
        }

        private bool VerifyCredential(NetworkCredential credential)
        {
            return credential.UserName == TestConstants.TestUserName
                && credential.Password == TestConstants.TestPassword;
        }
    }
}
