using Microsoft.AspNetCore.Identity;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Extensions;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    /// <summary>
    /// Представляет собой процесс извлечения информации о Identity-пользователе из хранилища.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class FetchIdentityUser : HandlerBase
    {
        private UserManager<IdentityUser> _userManager;
        private UserNameValidator _userNameValidator;

        /// <summary>
        /// Показывает, допустимо ли в контексте пустое значение (null) для данных о пользователе.
        /// </summary>
        public bool IsNullValid { get; set; }

        #region Constructors
        public FetchIdentityUser(
            UserManager<IdentityUser> userManager,
            UserNameValidator userNameValidator,
            IGuard guard = null) : base(guard)
        {
            _userManager = _guard.EnsureObjectParamIsNotNull(userManager, nameof(userManager));
            _userNameValidator = _guard.EnsureObjectParamIsNotNull(userNameValidator, nameof(userNameValidator));
        }

        public FetchIdentityUser(UserManager<IdentityUser> userManager, UserNameValidator userNameValidator)
            : this(userManager, userNameValidator, null)
        {
        }
        #endregion

        protected override bool CanHandle(IContext context)
        {
            return _userNameValidator.Validate(context, _errors);
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста идентификационное имя пользователя.
            var userName = context.Get(UserNameValidator.ContextKey) as string;

            // Извлечь из хранилища информацию о пользователе
            IdentityUser identityUser = await _userManager.FindByNameAsync(userName);

            // Если данные пусты, а пустое значение не допускается, то прервать цепочку обработчиков и вернуть сообщение
            // об ошибке.
            IAuthResult result = null;
            if (!IsNullValid && identityUser == null)
                result = new ErrorResult("Пользователь не найден.");

            // Иначе сохранить данные Identity-пользователя в контексте.
            context.Set(IdentityUserValidator.ContextKey, identityUser);

            return result;
        }
    }
}
