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
    /// Представляет собой процесс извлечения коллекции клаймов пользователя из хранилища.
    /// </summary>
    public class FetchUserClaims : HandlerBase
    {
        private UserManager<IdentityUser> _userManager;
        private IdentityUserValidator _validator;

        #region Constructors
        public FetchUserClaims(
            UserManager<IdentityUser> userManager,
            IdentityUserValidator identityUserValidator)
            : this(userManager, identityUserValidator, null)
        {
        }

        public FetchUserClaims(
            UserManager<IdentityUser> userManager,
            IdentityUserValidator identityUserValidator,
            IGuard guard)
            : base(guard)
        {
            _userManager = _guard.EnsureObjectParamIsNotNull(userManager, nameof(userManager));
            _validator = _guard.EnsureObjectParamIsNotNull(identityUserValidator, nameof(identityUserValidator));
        }
        #endregion

        protected override bool CanHandle(IContext context)
        {
            return _validator.Validate(context, _errors);
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста информацию о пользователе.
            var identityUser = context.Get(IdentityUserValidator.ContextKey) as IdentityUser;

            // Извлечь коллекию клаймов пользователя из хранилища.
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(identityUser);

            // Если по каким-то причинам не удалось извлечь коллекцию клаймов пользователя или коллекци пуста, то
            // прервать цепочку обработчиков и вернуть сообщение об ошибке.
            if (userClaims == null || !userClaims.Any())
            {
                return await Task.FromResult<IAuthResult>(new ErrorResult("Не удалось извлечь из хранилища " +
                    "информацию, необходимую для аутентификации пользователя."));
            }

            // Иначе сохранить коллекцию клаймов пользователя в контексте.
            context.Set(UserClaimsValidator.ContextKey, userClaims);

            // Передать управление следующему обработчику, вернув null.
            return default(IAuthResult);
        }
    }
}
