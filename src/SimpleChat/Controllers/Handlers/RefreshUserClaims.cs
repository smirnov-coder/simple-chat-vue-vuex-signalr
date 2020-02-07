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
    public class RefreshUserClaims : Handler
    {
        private UserManager<IdentityUser> _userManager;
        private IdentityUserValidator _identityUserValidator;
        private UserClaimsValidator _userClaimsValidator;
        private UserInfoValidator _userInfoValidator;
        private NameClaimTypeValidator _nameClaimTypeValidator;
        private AvatarClaimTypeValidator _avatarClaimTypeValidator;

        public RefreshUserClaims(
            UserManager<IdentityUser> userManager,
            IdentityUserValidator identityUserValidator,
            UserClaimsValidator userClaimsValidator,
            UserInfoValidator userInfoValidator,
            NameClaimTypeValidator nameClaimTypeValidator,
            AvatarClaimTypeValidator avatarClaimTypeValidator,
            IGuard guard)
            : base(guard)
        {
            _userManager = _guard.EnsureObjectParamIsNotNull(userManager, nameof(userManager));
            _identityUserValidator = _guard.EnsureObjectParamIsNotNull(identityUserValidator,
                nameof(identityUserValidator));
            _userClaimsValidator = _guard.EnsureObjectParamIsNotNull(userClaimsValidator,
                nameof(userClaimsValidator));
            _userInfoValidator = _guard.EnsureObjectParamIsNotNull(userInfoValidator, nameof(userInfoValidator));
            _nameClaimTypeValidator = _guard.EnsureObjectParamIsNotNull(nameClaimTypeValidator,
                nameof(nameClaimTypeValidator));
            _avatarClaimTypeValidator = _guard.EnsureObjectParamIsNotNull(avatarClaimTypeValidator,
                nameof(avatarClaimTypeValidator));
        }

        public RefreshUserClaims(
            UserManager<IdentityUser> userManager,
            IdentityUserValidator identityUserValidator,
            UserClaimsValidator userClaimsValidator,
            UserInfoValidator userInfoValidator,
            NameClaimTypeValidator nameClaimTypeValidator,
            AvatarClaimTypeValidator avatarClaimTypeValidator)
            : this(userManager, identityUserValidator, userClaimsValidator, userInfoValidator, nameClaimTypeValidator,
                  avatarClaimTypeValidator, null)
        {
        }

        protected override bool CanHandle(IContext context)
        {
            bool isIdentityUserValid = _identityUserValidator.Validate(context, _errors);
            bool isUserClaimsValid = _userClaimsValidator.Validate(context, _errors);
            bool isUserInfoValid = _userInfoValidator.Validate(context, _errors);
            bool isNameClaimTypeValid = _nameClaimTypeValidator.Validate(context, _errors);
            bool isAvatarClaimTypeValid = _avatarClaimTypeValidator.Validate(context, _errors);
            return isIdentityUserValid
                && isUserClaimsValid
                && isUserInfoValid
                && isNameClaimTypeValid
                && isAvatarClaimTypeValid;
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            var identityUser = context.Get(IdentityUserValidator.ContextKey) as IdentityUser;
            var userClaims = context.Get(UserClaimsValidator.ContextKey) as IList<Claim>;
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;
            var nameClaimType = context.Get(NameClaimTypeValidator.ContextKey) as string;
            var avatarClaimType = context.Get(AvatarClaimTypeValidator.ContextKey) as string;

            await _userManager.ReplaceClaimAsync(identityUser, userClaims.First(claim => claim.Type == nameClaimType),
                new Claim(nameClaimType, userInfo.Name));
            await _userManager.ReplaceClaimAsync(identityUser, userClaims.First(claim => claim.Type == avatarClaimType),
                new Claim(avatarClaimType, userInfo.Picture));

            return default(IAuthResult);
        }
    }
}
