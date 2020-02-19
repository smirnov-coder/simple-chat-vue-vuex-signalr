using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Services;
using SimpleChat.Controllers.Core;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования OAuth2-сервиса, хранящегося в контексте
    /// <see cref="IContext"/>.
    /// </summary>
    public class OAuth2ServiceValidator : ObjectValidator<IOAuth2Service>
    {
        /// <inheritdoc cref="ValidatorBase._key"/>
        public const string ContextKey = "oauth2Service";

        public OAuth2ServiceValidator() : this(null)
        {
        }

        public OAuth2ServiceValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
