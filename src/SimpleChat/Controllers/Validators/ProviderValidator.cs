using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Controllers.Core;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования имени внешнего OAuth2-провайдера, хранящегося в
    /// контексте <see cref="IContext"/>.
    /// </summary>
    public class ProviderValidator : StringValidator
    {
        /// <inheritdoc cref="ValidatorBase._key"/>
        public const string ContextKey = "provider";

        public ProviderValidator() : this(null)
        {
        }

        public ProviderValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
