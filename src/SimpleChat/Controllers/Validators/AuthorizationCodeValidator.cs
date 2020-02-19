using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Controllers.Core;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования кода авторизации, хранящегося в контексте
    /// <see cref="IContext"/>.
    /// </summary>
    public class AuthorizationCodeValidator : StringValidator
    {
        /// <inheritdoc cref="ValidatorBase._key"/>
        public const string ContextKey = "authorizationCode";

        public AuthorizationCodeValidator() : this(null)
        {
        }

        public AuthorizationCodeValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
