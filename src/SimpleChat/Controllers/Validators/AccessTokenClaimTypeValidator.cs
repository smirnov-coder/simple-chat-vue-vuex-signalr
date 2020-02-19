using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Controllers.Core;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования типа клайма маркера доступа, хранящегося в контексте
    /// <see cref="IContext"/>.
    /// </summary>
    public class AccessTokenClaimTypeValidator : StringValidator
    {
        /// <inheritdoc cref="ValidatorBase._key"/>
        public const string ContextKey = "accessTokenClaimType";

        public AccessTokenClaimTypeValidator() : this(null)
        {
        }

        public AccessTokenClaimTypeValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
