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
    public class CreateAuthenticatedResult : Handler
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
            bool isIdentityUserValid = _identityUserValidator.Validate(context, _errors);
            bool isUserInfoValid = _userInfoValidator.Validate(context, _errors);
            bool isPoviderValid = _providerValidator.Validate(context, _errors);
            return isIdentityUserValid && isUserInfoValid && isPoviderValid;
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            var identityUser = context.Get(IdentityUserValidator.ContextKey) as IdentityUser;
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;
            var provider = context.Get(ProviderValidator.ContextKey) as string;

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
