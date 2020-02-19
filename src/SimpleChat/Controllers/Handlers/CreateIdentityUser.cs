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
    /// Представляет собой процесс создания нового Identity-пользователя.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class CreateIdentityUserIfNotExist : HandlerBase
    {
        private UserManager<IdentityUser> _userManager;
        private NullableIdentityUserValidator _nullableIdentityUserValidator;
        private UserInfoValidator _userInfoValidator;

        #region Constructors
        public CreateIdentityUserIfNotExist(
            UserManager<IdentityUser> userManager,
            NullableIdentityUserValidator nullableIdentityUserValidator,
            UserInfoValidator userInfoValidator)
            : this(userManager, nullableIdentityUserValidator, userInfoValidator, null)
        {
        }

        public CreateIdentityUserIfNotExist(
            UserManager<IdentityUser> userManager,
            NullableIdentityUserValidator nullableIdentityUserValidator,
            UserInfoValidator userInfoValidator,
            IGuard guard) : base(guard)
        {
            _userManager = _guard.EnsureObjectParamIsNotNull(userManager, nameof(userManager));
            _nullableIdentityUserValidator = _guard.EnsureObjectParamIsNotNull(nullableIdentityUserValidator,
                nameof(nullableIdentityUserValidator));
            _userInfoValidator = _guard.EnsureObjectParamIsNotNull(userInfoValidator, nameof(userInfoValidator));
        }
        #endregion

        protected override bool CanHandle(IContext context)
        {
            bool canUseNullableIdentityUser = _nullableIdentityUserValidator.Validate(context, _errors);
            bool canUseUserInfo = _userInfoValidator.Validate(context, _errors);
            return canUseNullableIdentityUser && canUseUserInfo;
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста информацию о пользователе.
            var identityUser = context.Get(NullableIdentityUserValidator.ContextKey) as IdentityUser;
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;

            // Если информация о текущем Identity-пользователе пуста, то...
            IAuthResult result = null;
            if (identityUser == null)
            {
                // ... попытаться создать нового Identity-пользователя на основе данных о пользователе внешнего
                // OAuth2-провайдера.
                var newUser = new IdentityUser
                {
                    UserName = userInfo.Email,
                    Email = userInfo.Email,
                    EmailConfirmed = true
                };
                IdentityResult createUserResult = await _userManager.CreateAsync(newUser);

                // Если попытка создания удачна, то сохранить данные о Identity-пользователе в контексте.
                if (createUserResult.Succeeded)
                {
                    identityUser = await _userManager.FindByNameAsync(userInfo.Email);
                    context.Set(IdentityUserValidator.ContextKey, identityUser);
                }
                // Иначе прервать цепочку обработчиков и вернуть сообщение об ошибке.
                else
                {
                    result = new ErrorResult($"Не удалось зарегистрировать пользователя '{userInfo.Name}'.",
                        createUserResult.Errors.Select(error => error.Description));
                }
            }

            // Передать управление следующему обработчику, вернув null.
            return result;
        }
    }
}
