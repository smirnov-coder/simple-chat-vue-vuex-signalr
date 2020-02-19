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
    /// Представляет собой процесс проверки наличия e-mail адреса в даннх пользователя, полученных от внешнего
    /// OAuth2-провайдера.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class ValidateEmailAddress : HandlerBase
    {
        private UserInfoValidator _validator;

        public ValidateEmailAddress(UserInfoValidator userInfoValidator) : this(userInfoValidator, null)
        {
        }

        public ValidateEmailAddress(UserInfoValidator userInfoValidator, IGuard guard) : base(guard)
        {
            _validator = _guard.EnsureObjectParamIsNotNull(userInfoValidator, nameof(userInfoValidator));
        }

        protected override bool CanHandle(IContext context)
        {
            return _validator.Validate(context, _errors);
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста данные пользователя.
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;

            // Если внешний провайдер по каким-то причинам не предоставил e-mail адрес пользователя, то прервать цепочку
            // обработчиков и вернуть сообщение об ошибке, т.к. e-mail адрес обязателен для регистрации нового
            // пользователя на нашем сайте.
            IAuthResult result = null;
            if (string.IsNullOrWhiteSpace(userInfo.Email))
                result = new EmailRequiredResult(userInfo.Provider);

            // Иначе передать управление следующему обработчику, вернув null.
            return Task.FromResult(result);
        }
    }
}
