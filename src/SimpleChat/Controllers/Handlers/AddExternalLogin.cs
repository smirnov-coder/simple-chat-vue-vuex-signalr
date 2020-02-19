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
    /// Представляет собой операцию добавления данных о внешнем провайдере к Identity-данным пользователя.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class AddExternalLogin : HandlerBase
    {
        private UserManager<IdentityUser> _userManager;
        private IdentityUserValidator _identityUserValidator;
        private UserInfoValidator _userInfoValidator;

        public AddExternalLogin(
            UserManager<IdentityUser> userManager,
            IdentityUserValidator identityUserValidator,
            UserInfoValidator userInfoValidator)
            : this(userManager, identityUserValidator, userInfoValidator, null)
        {
        }

        public AddExternalLogin(
            UserManager<IdentityUser> userManager,
            IdentityUserValidator identityUserValidator,
            UserInfoValidator userInfoValidator,
            IGuard guard)
            : base(guard)
        {
            _userManager = _guard.EnsureObjectParamIsNotNull(userManager, nameof(userManager));
            _identityUserValidator = _guard.EnsureObjectParamIsNotNull(identityUserValidator,
                nameof(identityUserValidator));
            _userInfoValidator = _guard.EnsureObjectParamIsNotNull(userInfoValidator, nameof(userInfoValidator));
        }

        protected override bool CanHandle(IContext context)
        {
            bool canUseIdentityUser = _identityUserValidator.Validate(context, _errors);
            bool canUseUserInfo = _userInfoValidator.Validate(context, _errors);
            return canUseIdentityUser && canUseUserInfo;
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста информацию о пользователе.
            var identityUser = context.Get(IdentityUserValidator.ContextKey) as IdentityUser;
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;

            // Выполнить попытку добавить логин через внешнего провайдера.
            IAuthResult result = null;
            IdentityResult addLoginResult = await _userManager.AddLoginAsync(identityUser,
                new UserLoginInfo(userInfo.Provider, userInfo.Id, null));

            // Если попытка неудачна, то прервать цепочку обработчиков и вернуть сообщение об ошибке.
            if (!addLoginResult.Succeeded)
            {
                return new ErrorResult($"Не удалось добавить вход через '{userInfo.Provider}' для пользователя " +
                    $"'{userInfo.Name}'.", addLoginResult.Errors.Select(error => error.Description));
            }

            // Иначе продолжить выполнение цепочки без добавления каких-либо данных в контекст.
            return result;
        }
    }
}
