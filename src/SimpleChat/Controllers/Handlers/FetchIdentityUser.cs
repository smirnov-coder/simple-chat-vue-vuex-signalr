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
    public class FetchIdentityUser : Handler
    {
        private UserManager<IdentityUser> _userManager;
        private UserNameValidator _userNameValidator;

        public bool IsNullValid { get; set; }

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

        protected override bool CanHandle(IContext context)
        {
            return _userNameValidator.Validate(context, _errors);
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            var userName = context.Get(UserNameValidator.ContextKey) as string;

            IdentityUser identityUser = await _userManager.FindByNameAsync(userName);
            IAuthResult result = null;
            if (!IsNullValid && identityUser == null)
                result = new ErrorResult("Пользователь не найден.");
            context.Set(IdentityUserValidator.ContextKey, identityUser);
            return result;
        }
    }
}
