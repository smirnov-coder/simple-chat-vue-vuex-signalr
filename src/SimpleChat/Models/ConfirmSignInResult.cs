namespace SimpleChat.Models
{
    /// <summary>
    /// Результат, информирующий пользователя о необходимости подтвердить первый вход на сайт через внешний
    /// OAuth2-провайдер.
    /// </summary>
    public class ConfirmSignInResult : IAuthResult
    {
        public string Type { get; } = "confirm_sign_in";

        /// <summary>
        /// Идентификатор сессии (ключ, по которому доступны данные о пользователе на сервере).
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// E-mail адрес пользователя.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Имя внешнего OAuth2-провайдера.
        /// </summary>
        public string Provider { get; set; }

        public ConfirmSignInResult(string sessionId, string email, string provider)
        {
            SessionId = sessionId;
            Email = email;
            Provider = provider;
        }
    }
}
