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
    /// <summary>
    /// Представляет собой процесс создания результата успешного входа на сайт через внешнего OAuth2-провайдера.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class CreateSuccessResult : HandlerBase
    {
        private IdentityUserValidator _identityUserValidator;
        private UserInfoValidator _userInfoValidator;
        private IJwtHelper _jwtHelper;

        #region Constuctors
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
        #endregion

        protected override bool CanHandle(IContext context)
        {
            bool canUseIdentityUser = _identityUserValidator.Validate(context, _errors);
            bool canUseUserInfo = _userInfoValidator.Validate(context, _errors);
            return canUseIdentityUser && canUseUserInfo;
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста информацию о пользователе.
            var identityUser = context.Get(IdentityUserValidator.ContextKey) as IdentityUser;
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;

            // На основе данных о пользователе создать коллекцию клаймов:
            // - идентификатор пользователя (identity id),
            // - идентификационное имя пользователя (identity username);
            // - полное имя пользователя;
            // - аватар (веб-путь к изображению профайла пользователя внешнего провайдера);
            // - имя внешнего провайдера.
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, identityUser.Id),
                new Claim(ClaimTypes.Name, userInfo.Email),
                new Claim(CustomClaimTypes.FullName, userInfo.Name),
                new Claim(CustomClaimTypes.Avatar, userInfo.Picture),
                new Claim(CustomClaimTypes.Provider, userInfo.Provider)
            };

            // Создать JWT на основе коллекции клаймов.
            string encodedJwt = _jwtHelper.CreateEncodedJwt(userClaims);

            // Прервать цепочку обработчиков и вернуть пользователю JWT для дальнейшей работы с сайтом.
            // Обработчик является терминальным.
            return Task.FromResult<IAuthResult>(new SignInSuccessResult(encodedJwt));
        }
    }
}
