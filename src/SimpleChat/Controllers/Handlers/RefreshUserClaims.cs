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
    /// <summary>
    /// Представляет собой процесс обновления полного имени и аватара пользователя, которые хранятся в виде клаймов в
    /// хранилище.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class RefreshUserClaims : HandlerBase
    {
        private UserManager<IdentityUser> _userManager;
        private IdentityUserValidator _identityUserValidator;
        private UserClaimsValidator _userClaimsValidator;
        private UserInfoValidator _userInfoValidator;
        private NameClaimTypeValidator _nameClaimTypeValidator;
        private AvatarClaimTypeValidator _avatarClaimTypeValidator;

        #region Constructors
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
        #endregion

        protected override bool CanHandle(IContext context)
        {
            bool canUseIdentityUser = _identityUserValidator.Validate(context, _errors);
            bool canUseUserClaims = _userClaimsValidator.Validate(context, _errors);
            bool canUseUserInfo = _userInfoValidator.Validate(context, _errors);
            bool canUseNameClaimType = _nameClaimTypeValidator.Validate(context, _errors);
            bool canUseAvatarClaimType = _avatarClaimTypeValidator.Validate(context, _errors);
            return canUseIdentityUser
                && canUseUserClaims
                && canUseUserInfo
                && canUseNameClaimType
                && canUseAvatarClaimType;
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста информацию о пользователе, коллекцию клаймов пользователя, тип клаймов полного имени
            // и аватара пользователя.
            var identityUser = context.Get(IdentityUserValidator.ContextKey) as IdentityUser;
            var userClaims = context.Get(UserClaimsValidator.ContextKey) as IList<Claim>;
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;
            var nameClaimType = context.Get(NameClaimTypeValidator.ContextKey) as string;
            var avatarClaimType = context.Get(AvatarClaimTypeValidator.ContextKey) as string;

            // Обновить полное имя пользователя, полученное от внешнего провайдера.
            await _userManager.ReplaceClaimAsync(identityUser, userClaims.First(claim => claim.Type == nameClaimType),
                new Claim(nameClaimType, userInfo.Name));

            // Обновить аватар пользователя.
            await _userManager.ReplaceClaimAsync(identityUser, userClaims.First(claim => claim.Type == avatarClaimType),
                new Claim(avatarClaimType, userInfo.Picture));

            // Передать управление следующему обработчику, вернув null.
            return default(IAuthResult);
        }
    }
}
