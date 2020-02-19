using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Controllers.Core;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования типа клайма полного имени пользователя, хранящегося
    /// в контексте <see cref="IContext"/>.
    /// </summary>
    public class NameClaimTypeValidator : StringValidator
    {
        /// <inheritdoc cref="ValidatorBase._key"/>
        public const string ContextKey = "nameClaimType";

        public NameClaimTypeValidator() : this(null)
        {
        }

        public NameClaimTypeValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
