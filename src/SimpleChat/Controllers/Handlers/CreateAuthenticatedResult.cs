using Microsoft.AspNetCore.Identity;
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
    /// Представляет собой процесс создания результата успешной аутентификации пользователя.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class CreateAuthenticatedResult : HandlerBase
    {
        private IdentityUserValidator _identityUserValidator;
        private UserInfoValidator _userInfoValidator;
        private ProviderValidator _providerValidator;

        public CreateAuthenticatedResult(
            IdentityUserValidator identityUserValidator,
            UserInfoValidator userInfoValidator,
            ProviderValidator providerValidator,
            IGuard guard)
            : base(guard)
        {
            _identityUserValidator = _guard.EnsureObjectParamIsNotNull(identityUserValidator,
                nameof(identityUserValidator));
            _userInfoValidator = _guard.EnsureObjectParamIsNotNull(userInfoValidator, nameof(userInfoValidator));
            _providerValidator = _guard.EnsureObjectParamIsNotNull(providerValidator, nameof(providerValidator));
        }

        public CreateAuthenticatedResult(
            IdentityUserValidator identityUserValidator,
            UserInfoValidator userInfoValidator,
            ProviderValidator providerValidator)
            : this(identityUserValidator, userInfoValidator, providerValidator, null)
        {
        }


        protected override bool CanHandle(IContext context)
        {
            bool canUseIdentityUser = _identityUserValidator.Validate(context, _errors);
            bool canUseUserInfo = _userInfoValidator.Validate(context, _errors);
            bool canUsePovider = _providerValidator.Validate(context, _errors);
            return canUseIdentityUser && canUseUserInfo && canUsePovider;
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста информацию о пользователе и имя внешнего OAuth2-провайдера.
            var identityUser = context.Get(IdentityUserValidator.ContextKey) as IdentityUser;
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;
            var provider = context.Get(ProviderValidator.ContextKey) as string;

            // Сформировать результат успешной аутентификации пользователя. Обработчик в любом случае прерывает цепочку
            // обработчиков и не передаёт управление дальше, т.е. является терминальным.
            return Task.FromResult<IAuthResult>(new AuthenticatedResult
            {
                User = new UserInfo
                {
                    Id = identityUser.Id,
                    Name = userInfo.Name,
                    Avatar = userInfo.Picture,
                    Provider = provider
                }
            });
        }
    }
}
