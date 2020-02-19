using Microsoft.AspNetCore.Hosting;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    /// <summary>
    /// Представляет собой процесс проверки правильности кода подтверждения
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class ValidateConfirmationCode : HandlerBase
    {
        private ConfirmationCodeValidator _confirmationCodeValidator;
        private UserInfoValidator _userInfoValidator;
        private IHostingEnvironment _environment;

        #region Constructors
        public ValidateConfirmationCode(
            ConfirmationCodeValidator confirmationCodeValidator,
            UserInfoValidator userInfoValidator,
            IHostingEnvironment environment)
            : this(confirmationCodeValidator, userInfoValidator, environment, null)
        {
        }

        public ValidateConfirmationCode(
            ConfirmationCodeValidator confirmationCodeValidator,
            UserInfoValidator userInfoValidator,
            IHostingEnvironment environment,
            IGuard guard) : base(guard)
        {
            _confirmationCodeValidator = _guard.EnsureObjectParamIsNotNull(confirmationCodeValidator,
                nameof(confirmationCodeValidator));
            _userInfoValidator = _guard.EnsureObjectParamIsNotNull(userInfoValidator, nameof(userInfoValidator));
            _environment = _guard.EnsureObjectParamIsNotNull(environment, nameof(environment));
        }
        #endregion

        protected override bool CanHandle(IContext context)
        {
            bool canUseConfirmationCode = _confirmationCodeValidator.Validate(context, _errors);
            bool canUseUserInfo = _userInfoValidator.Validate(context, _errors);
            return canUseConfirmationCode && canUseUserInfo;
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста код подтверждения и информацию о пользователе.
            string confirmationCode = context.Get(ConfirmationCodeValidator.ContexKey) as string;
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;

            // Если код подтверждения неверен, то прервать цепочку обработчиков и вернуть сообщения об ошибке.
            IAuthResult result = null;
            if (!IsValidCode(confirmationCode, GenerateConfirmationCode(userInfo.Id)))
                result = new ErrorResult("Неверный код подтверждения.");

            // Иначе передать управление следующему обработчику, вернув null.
            return Task.FromResult(result);
        }

        protected virtual bool IsValidCode(string actual, string expected)
        {
            // Для облегчения отладки, в режиме разработки используется один и тот же код подтверждения "test".
            return _environment.IsDevelopment() ? actual == "test" : actual == expected;
        }

        // Сгенерировать под подтверждения. Код должен быть воспроизводимым на серверной стороне.
        /// TODO: надо бы заменить на какой-нить генератор в виде зависимости
        protected virtual string GenerateConfirmationCode(string userId) => userId.GetHashCode().ToString("x8");
    }
}
