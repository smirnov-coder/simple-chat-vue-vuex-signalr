namespace SimpleChat.Models
{
    /// <summary>
    /// Абстрактный базовый класс результата аутентификации пользователя.
    /// </summary>
    public abstract class AuthenticationResultBase : IAuthResult
    {
        public string Type { get; } = "auth_check";

        /// <summary>
        /// Показывает, авторизован пользователь или нет.
        /// </summary>
        public abstract bool IsAuthenticated { get; }
    }
}
