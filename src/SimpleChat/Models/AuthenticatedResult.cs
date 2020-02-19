namespace SimpleChat.Models
{
    /// <summary>
    /// Результат успешной аутентификации пользователя.
    /// </summary>
    public class AuthenticatedResult : AuthenticationResultBase
    {
        public override bool IsAuthenticated { get; } = true;

        /// <inheritdoc cref="UserInfo"/>
        public UserInfo User { get; set; }
    }
}
