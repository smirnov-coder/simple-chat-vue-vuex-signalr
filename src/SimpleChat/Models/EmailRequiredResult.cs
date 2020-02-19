namespace SimpleChat.Models
{
    /// <summary>
    /// Результат ошибки отсутствия информации о e-mail адресе пользователя внешнего OAuth2-провайдера.
    /// </summary>
    public class EmailRequiredResult : IAuthResult
    {
        public string Type { get; } = "email_required";

        /// <summary>
        /// Дружественное к пользователю сообщение об ошибке.
        /// </summary>
        public string Message { get; set; }

        public EmailRequiredResult(string provider)
        {
            Message = $"Для того, чтобы войти на наш сайт, необходимо предоставить доступ к адресу электронной " +
                $"почты Вашего аккаунта в социальной сети '{provider}'.";
        }
    }
}
