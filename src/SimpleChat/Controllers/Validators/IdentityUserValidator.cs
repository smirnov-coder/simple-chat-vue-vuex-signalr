using Microsoft.AspNetCore.Identity;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Controllers.Core;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования информации о Identity-пользователе, хранящейся в
    /// контексте <see cref="IContext"/>. Валидатор не допускает значение null.
    /// </summary>
    public class IdentityUserValidator : ObjectValidator<IdentityUser>
    {
        /// <inheritdoc cref="ValidatorBase._key"/>
        public const string ContextKey = "identityUser";

        public IdentityUserValidator() : this(null)
        {
        }

        public IdentityUserValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
