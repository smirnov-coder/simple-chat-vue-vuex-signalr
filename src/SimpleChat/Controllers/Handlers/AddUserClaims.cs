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
    /// Представляет собой процесс сохранения информации о пользователе внешнего OAuth2-провайдера (полное имя,
    /// фотограция профайла, маркер доступа к API) в виде клаймов Identity-пользователя.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class AddUserClaims : HandlerBase
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
            bool canUseIdentityUser = _identityUserValidator.Validate(context, _errors);
            bool canUseUserInfo = _userInfoValidator.Validate(context, _errors);
            bool canUseNameClaimType = _nameClaimTypeValidator.Validate(context, _errors);
            bool canUseAvatarClaimType = _avatarClaimTypeValidator.Validate(context, _errors);
            bool canUseAccessTokeClaimType = _accessTokenClaimTypeValidator.Validate(context, _errors);
            return canUseIdentityUser
                && canUseUserInfo
                && canUseNameClaimType
                && canUseAvatarClaimType
                && canUseAccessTokeClaimType;
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста информацию о пользователе, типы клаймов имени пользователя, аватара и маркера
            // доступа.
            var identityUser = context.Get(IdentityUserValidator.ContextKey) as IdentityUser;
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;
            string nameClaimType = context.Get(NameClaimTypeValidator.ContextKey) as string;
            string avatarClaimType = context.Get(AvatarClaimTypeValidator.ContextKey) as string;
            string accessTokenClaimType = context.Get(AccessTokenClaimTypeValidator.ContextKey) as string;

            // Создать новые клаймы (полное имя пользователя, аватара, маркер доступа) для внешнего
            // OAuth2-провайдера и выполнить попытку добавить клаймы в коллекцию клаймов.
            IAuthResult result = null;
            IdentityResult addClaimsResult = await _userManager.AddClaimsAsync(identityUser, new List<Claim>
            {
                new Claim(nameClaimType, userInfo.Name),
                new Claim(avatarClaimType, userInfo.Picture),
                new Claim(accessTokenClaimType, userInfo.AccessToken)
            });

            // Если попытка неудачна, то прервать цепочку обработчиков и вернуть сообщение об ошибке.
            if (!addClaimsResult.Succeeded)
            {
                result = new ErrorResult($"Не удалось сохранить данные '{userInfo.Provider}' профайла пользователя " +
                    $"'{userInfo.Name}'.", addClaimsResult.Errors.Select(error => error.Description));
            }

            // Иначе продолжить выполнение цепочки без добавления каких-либо данных в контекст.
            return result;
        }
    }
}
