namespace SimpleChat.Models
{
    /// <summary>
    /// Результат неудачной аутентификации пользователя.
    /// </summary>
    public class NotAuthenticatedResult : AuthenticationResultBase
    {
        public override bool IsAuthenticated { get; } = false;
    }
}
