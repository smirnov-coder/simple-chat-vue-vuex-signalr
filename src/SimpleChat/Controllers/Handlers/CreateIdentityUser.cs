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
    public class CreateIdentityUserIfNotExist : Handler
    {
        private UserManager<IdentityUser> _userManager;
        private NullableIdentityUserValidator _nullableIdentityUserValidator;
        private UserInfoValidator _userInfoValidator;

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

        protected override bool CanHandle(IContext context)
        {
            bool isNullableIdentityUserValid = _nullableIdentityUserValidator.Validate(context, _errors);
            bool isUserInfoValid = _userInfoValidator.Validate(context, _errors);
            return isNullableIdentityUserValid && isUserInfoValid;
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            var identityUser = context.Get(NullableIdentityUserValidator.ContextKey) as IdentityUser;
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;

            IAuthResult result = null;
            if (identityUser == null)
            {
                var newUser = new IdentityUser
                {
                    UserName = userInfo.Email,
                    Email = userInfo.Email,
                    EmailConfirmed = true
                };
                IdentityResult createUserResult = await _userManager.CreateAsync(newUser);
                if (createUserResult.Succeeded)
                {
                    identityUser = await _userManager.FindByNameAsync(userInfo.Email);
                    context.Set(IdentityUserValidator.ContextKey, identityUser);
                }
                else
                {
                    result = new ErrorResult($"Не удалось зарегистрировать пользователя '{userInfo.Name}'.",
                        createUserResult.Errors.Select(error => error.Description));
                }
            }
            return result;
        }
    }
}
