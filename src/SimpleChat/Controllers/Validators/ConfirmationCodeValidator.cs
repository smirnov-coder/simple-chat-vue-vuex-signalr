using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Controllers.Core;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования кода подтверждения первого входа на сайт через
    /// внешнего OAuth2-провайдера, хранящегося в контексте <see cref="IContext"/>.
    /// </summary>
    public class ConfirmationCodeValidator : StringValidator
    {
        /// <inheritdoc cref="ValidatorBase._key"/>
        public const string ContexKey = "confirmationCode";

        public ConfirmationCodeValidator() : this(null)
        {
        }

        public ConfirmationCodeValidator(IGuard guard) : base(ContexKey, guard)
        {
        }
    }
}
