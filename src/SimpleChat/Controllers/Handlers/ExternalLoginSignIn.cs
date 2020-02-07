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
    public class ExternalLoginSignIn : Handler
    {
        private SignInManager<IdentityUser> _signInManager;
        private UserInfoValidator _validator;

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

        protected override bool CanHandle(IContext context)
        {
            return _validator.Validate(context, _errors);
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;

            SignInResult signInResult = await _signInManager.ExternalLoginSignInAsync(userInfo.Provider, userInfo.Id,
                false, true);

            context.Set(SignInResultValidator.ContextKey, signInResult);
            return default(IAuthResult);
        }
    }
}
