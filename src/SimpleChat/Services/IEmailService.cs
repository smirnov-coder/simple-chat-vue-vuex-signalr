using System.Threading.Tasks;

namespace SimpleChat.Services
{
    /// <summary>
    /// Представляет собой сервис для работы с электронной почтой.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Асинхронно отправляет e-mail.
        /// </summary>
        /// <param name="recipientName">Имя получателя e-mail.</param>
        /// <param name="recipientEmail">E-mail адрес получателя.</param>
        /// <param name="subject">Тема сообщения.</param>
        /// <param name="text">Текст сообщения.</param>
        Task SendEmailAsync(string recipientName, string recipientEmail, string subject, string text);

        /// <summary>
        /// Создаёт текст e-mail с кодом подтверждения первого входа на сайт через внешний OAuth2-провайдер.
        /// </summary>
        /// <param name="name">Полное имя пользователя.</param>
        /// <param name="provider">
        /// Имя внешнего OAuth2-провайдера, через который пользователь осуществляет вход на сайт.
        /// </param>
        /// <param name="appUrl">Полный адрес приложения чата для генерации ссылки в письме.</param>
        /// <param name="code">Код подтверждения.</param>
        string CreateSignInConfirmationEmail(string name, string provider, string appUrl, string code);
    }
}
