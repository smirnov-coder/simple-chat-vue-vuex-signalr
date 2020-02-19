using Microsoft.AspNetCore.Identity;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Controllers.Core;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования результата попытки входа на сайт через внешний
    /// OAuth2-провайдер, хранящегося в контексте <see cref="IContext"/>.
    /// </summary>
    public class SignInResultValidator : ObjectValidator<SignInResult>
    {
        /// <inheritdoc cref="ValidatorBase._key"/>
        public const string ContextKey = "signInResult";

        public SignInResultValidator() : this(null)
        {
        }

        public SignInResultValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
