using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Controllers.Core;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования идентификационного имени пользователя, хранящегося
    /// в контексте <see cref="IContext"/>.
    /// </summary>
    public class UserNameValidator : StringValidator
    {
        /// <inheritdoc cref="ValidatorBase._key"/>
        public const string ContextKey = "userName";

        public UserNameValidator() : this(null)
        {
        }

        public UserNameValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
