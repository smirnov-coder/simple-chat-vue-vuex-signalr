using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Controllers.Core;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования идентификатора сессии пользователя, хранящегося в
    /// контексте <see cref="IContext"/>.
    /// </summary>
    public class SessionIdValidator : StringValidator
    {
        /// <inheritdoc cref="ValidatorBase._key"/>
        public const string ContextKey = "sessionId";

        public SessionIdValidator() : this(null)
        {
        }

        public SessionIdValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
