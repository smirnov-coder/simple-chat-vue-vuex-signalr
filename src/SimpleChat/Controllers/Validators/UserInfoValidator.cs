using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using SimpleChat.Controllers.Core;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования информации о пользователе внешнего
    /// OAuth2-провайдера, хранящейся в контексте <see cref="IContext"/>.
    /// </summary>
    public class UserInfoValidator : ObjectValidator<ExternalUserInfo>
    {
        /// <inheritdoc cref="ValidatorBase._key"/>
        public const string ContextKey = "userInfo";

        public UserInfoValidator() : this(null)
        {
        }

        public UserInfoValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
