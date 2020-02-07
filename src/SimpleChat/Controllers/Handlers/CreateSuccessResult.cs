using Microsoft.AspNetCore.Identity;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    public class CreateSuccessResult : Handler
    {
        private IdentityUserValidator _identityUserValidator;
        private UserInfoValidator _userInfoValidator;
        private IJwtHelper _jwtHelper;

        public CreateSuccessResult(
            IdentityUserValidator identityUserValidator,
            UserInfoValidator userInfoValidator,
            IJwtHelper jwtHelper)
            : this(identityUserValidator, userInfoValidator, jwtHelper, null)
        {
        }

        public CreateSuccessResult(
            IdentityUserValidator identityUserValidator,
            UserInfoValidator userInfoValidator,
            IJwtHelper jwtHelper,
            IGuard guard)
            : base(guard)
        {
            _identityUserValidator = _guard.EnsureObjectParamIsNotNull(identityUserValidator,
                nameof(identityUserValidator));
            _userInfoValidator = _guard.EnsureObjectParamIsNotNull(userInfoValidator, nameof(userInfoValidator));
            _jwtHelper = _guard.EnsureObjectParamIsNotNull(jwtHelper, nameof(jwtHelper));
        }

        protected override bool CanHandle(IContext context)
        {
            bool isIdentityUserValid = _identityUserValidator.Validate(context, _errors);
            bool isUserInfoValid = _userInfoValidator.Validate(context, _errors);
            return isIdentityUserValid && isUserInfoValid;
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            var identityUser = context.Get(IdentityUserValidator.ContextKey) as IdentityUser;
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, identityUser.Id), // Будет использовано в ChatHub.
                new Claim(ClaimTypes.Name, userInfo.Email),
                new Claim(CustomClaimTypes.FullName, userInfo.Name),
                new Claim(CustomClaimTypes.Avatar, userInfo.Picture),
                new Claim(CustomClaimTypes.Provider, userInfo.Provider)
            };
            string encodedJwt = _jwtHelper.CreateEncodedJwt(userClaims);
            return Task.FromResult<IAuthResult>(new SignInSuccessResult(encodedJwt));
        }
    }
}
