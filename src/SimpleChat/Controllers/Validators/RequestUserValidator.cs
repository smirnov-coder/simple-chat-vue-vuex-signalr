using SimpleChat.Infrastructure.Helpers;
using System.Security.Claims;
using SimpleChat.Controllers.Core;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования данных аутентификации запроса к
    /// <see cref="AuthController"/>, хранящихся в контексте <see cref="IContext"/>.
    /// </summary>
    public class RequestUserValidator : ObjectValidator<ClaimsPrincipal>
    {
        /// <inheritdoc cref="ValidatorBase._key"/>
        public const string ContextKey = "requestUser";

        public RequestUserValidator() : this(null)
        {
        }

        public RequestUserValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
