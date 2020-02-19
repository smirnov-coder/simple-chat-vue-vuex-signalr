using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Controllers.Core;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования типа клайма аватара пользователя, хранящегося в
    /// контексте <see cref="IContext"/>.
    /// </summary>
    public class AvatarClaimTypeValidator : StringValidator
    {
        /// <inheritdoc cref="ValidatorBase._key"/>
        public const string ContextKey = "avatarClaimType";

        public AvatarClaimTypeValidator() : this(null)
        {
        }

        public AvatarClaimTypeValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
