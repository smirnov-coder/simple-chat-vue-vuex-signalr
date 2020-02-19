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
    /// Представляет собой процесс проверки возможности входа на сайт через внешнего OAuth2-провайдера.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class ExternalLoginSignIn : HandlerBase
    {
        private SignInManager<IdentityUser> _signInManager;
        private UserInfoValidator _validator;

        #region Constructors
        public ExternalLoginSignIn(SignInManager<IdentityUser> signInManager, UserInfoValidator userInfoValidator)
            : this(signInManager, userInfoValidator, null)
        {
        }

        public ExternalLoginSignIn(SignInManager<IdentityUser> signInManager, UserInfoValidator userInfoValidator,
            IGuard guard) : base(guard)
        {
            _signInManager = _guard.EnsureObjectParamIsNotNull(signInManager, nameof(signInManager));
            _validator = _guard.EnsureObjectParamIsNotNull(userInfoValidator, nameof(userInfoValidator));
        }
        #endregion

        protected override bool CanHandle(IContext context)
        {
            return _validator.Validate(context, _errors);
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста информацию о пользователе внешнего OAuth2-провайдера.
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;

            // Выполнить попытку войти на сайт через внешнего провайдера. Данная операция, в случае успеха, добавляет
            // cookie к ответу от сервера.
            SignInResult signInResult = await _signInManager.ExternalLoginSignInAsync(userInfo.Provider, userInfo.Id,
                false, true);

            // Сохранить результат попытки входа в контексте, независимо, положительный он или отрицательный.
            context.Set(SignInResultValidator.ContextKey, signInResult);

            // Перадать управление следующему обработчику, вернув null.
            return default(IAuthResult);
        }
    }
}
