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
    public class FetchUserClaims : Handler
    {
        private UserManager<IdentityUser> _userManager;
        private IdentityUserValidator _validator;

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

        protected override bool CanHandle(IContext context)
        {
            return _validator.Validate(context, _errors);
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            var identityUser = context.Get(IdentityUserValidator.ContextKey) as IdentityUser;

            IList<Claim> userClaims = await _userManager.GetClaimsAsync(identityUser);
            if (userClaims == null || !userClaims.Any())
            {
                return await Task.FromResult<IAuthResult>(new ErrorResult("Не удалось извлечь из хранилища " +
                    "информацию, необходимую для аутентификации пользователя."));
            }
            context.Set(UserClaimsValidator.ContextKey, userClaims);
            return default(IAuthResult);
        }
    }
}
