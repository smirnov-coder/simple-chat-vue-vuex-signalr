using Microsoft.AspNetCore.Identity;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    public class AddUserClaims : Handler
    {
        private UserManager<IdentityUser> _userManager;
        private IdentityUserValidator _identityUserValidator;
        private UserInfoValidator _userInfoValidator;
        private NameClaimTypeValidator _nameClaimTypeValidator;
        private AvatarClaimTypeValidator _avatarClaimTypeValidator;
        private AccessTokenClaimTypeValidator _accessTokenClaimTypeValidator;

        public AddUserClaims(
            UserManager<IdentityUser> userManager,
            IdentityUserValidator identityUserValidator,
            UserInfoValidator userInfoValidator,
            NameClaimTypeValidator nameClaimTypeValidator,
            AvatarClaimTypeValidator avatarClaimTypeValidator,
            AccessTokenClaimTypeValidator accessTokenClaimTypeValidator)
            : this(userManager, identityUserValidator, userInfoValidator, nameClaimTypeValidator,
                  avatarClaimTypeValidator, accessTokenClaimTypeValidator, null)
        {
        }

        public AddUserClaims(
            UserManager<IdentityUser> userManager,
            IdentityUserValidator identityUserValidator,
            UserInfoValidator userInfoValidator,
            NameClaimTypeValidator nameClaimTypeValidator,
            AvatarClaimTypeValidator avatarClaimTypeValidator,
            AccessTokenClaimTypeValidator accessTokenClaimTypeValidator,
            IGuard guard)
            : base(guard)
        {
            _userManager = _guard.EnsureObjectParamIsNotNull(userManager, nameof(userManager));
            _identityUserValidator = _guard.EnsureObjectParamIsNotNull(identityUserValidator,
                nameof(identityUserValidator));
            _userInfoValidator = _guard.EnsureObjectParamIsNotNull(userInfoValidator, nameof(userInfoValidator));
            _nameClaimTypeValidator = _guard.EnsureObjectParamIsNotNull(nameClaimTypeValidator,
                nameof(nameClaimTypeValidator));
            _avatarClaimTypeValidator = _guard.EnsureObjectParamIsNotNull(avatarClaimTypeValidator,
                nameof(avatarClaimTypeValidator));
            _accessTokenClaimTypeValidator = _guard.EnsureObjectParamIsNotNull(accessTokenClaimTypeValidator,
                nameof(accessTokenClaimTypeValidator));
        }

        protected override bool CanHandle(IContext context)
        {
            bool isIdentityUserValid = _identityUserValidator.Validate(context, _errors);
            bool isUserInfoValid = _userInfoValidator.Validate(context, _errors);
            bool isNameClaimTypeValid = _nameClaimTypeValidator.Validate(context, _errors);
            bool isAvatarClaimTypeValid = _avatarClaimTypeValidator.Validate(context, _errors);
            bool isAccessTokeClaimTypeValid = _accessTokenClaimTypeValidator.Validate(context, _errors);
            return isIdentityUserValid
                && isUserInfoValid
                && isNameClaimTypeValid
                && isAvatarClaimTypeValid
                && isAccessTokeClaimTypeValid;
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            var identityUser = context.Get(IdentityUserValidator.ContextKey) as IdentityUser;
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;
            string nameClaimType = context.Get(NameClaimTypeValidator.ContextKey) as string;
            string avatarClaimType = context.Get(AvatarClaimTypeValidator.ContextKey) as string;
            string accessTokenClaimType = context.Get(AccessTokenClaimTypeValidator.ContextKey) as string;

            IAuthResult result = null;
            IdentityResult addClaimsResult = await _userManager.AddClaimsAsync(identityUser, new List<Claim>
            {
                new Claim(nameClaimType, userInfo.Name),
                new Claim(avatarClaimType, userInfo.Picture),
                new Claim(accessTokenClaimType, userInfo.AccessToken)
            });
            if (!addClaimsResult.Succeeded)
            {
                result = new ErrorResult($"Не удалось сохранить данные '{userInfo.Provider}' профайла пользователя " +
                    $"'{userInfo.Name}'.", addClaimsResult.Errors.Select(error => error.Description));
            }
            return result;
        }
    }
}
