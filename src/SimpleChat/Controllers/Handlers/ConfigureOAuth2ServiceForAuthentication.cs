using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Extensions;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using SimpleChat.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    /// <summary>
    /// Представляет собой процесс предварительной настройки OAuth2-сервиса для выполнения запросов к API внешнего
    /// OAuth2-провайдера в ходе выполнения аутентификации пользователя.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class ConfigureOAuth2ServiceForAuthentication : HandlerBase
    {
        private OAuth2ServiceValidator _oauth2ServiceValidator;
        private AccessTokenClaimTypeValidator _accessTokenClaimTypeValidator;
        private UserClaimsValidator _userClaimsValidator;

        public ConfigureOAuth2ServiceForAuthentication(
            OAuth2ServiceValidator oauth2ServiceValidator,
            AccessTokenClaimTypeValidator accessTokenClaimTypeValidator,
            UserClaimsValidator userClaimsValidator,
            IGuard guard) : base(guard)
        {
            _oauth2ServiceValidator = _guard.EnsureObjectParamIsNotNull(oauth2ServiceValidator,
                nameof(oauth2ServiceValidator));
            _accessTokenClaimTypeValidator = _guard.EnsureObjectParamIsNotNull(accessTokenClaimTypeValidator,
                nameof(accessTokenClaimTypeValidator));
            _userClaimsValidator = _guard.EnsureObjectParamIsNotNull(userClaimsValidator, nameof(userClaimsValidator));
        }

        public ConfigureOAuth2ServiceForAuthentication(
            OAuth2ServiceValidator oauth2ServiceValidator,
            AccessTokenClaimTypeValidator accessTokenClaimTypeValidator,
            UserClaimsValidator userClaimsValidator)
            : this(oauth2ServiceValidator, accessTokenClaimTypeValidator, userClaimsValidator, null)
        {
        }

        protected override bool CanHandle(IContext context)
        {
            bool canUseOAuth2Service = _oauth2ServiceValidator.Validate(context, _errors);
            bool canUseAccessTokenClaimType = _accessTokenClaimTypeValidator.Validate(context, _errors);
            bool canUseUserClaims = _userClaimsValidator.Validate(context, _errors);
            return canUseOAuth2Service && canUseAccessTokenClaimType && canUseUserClaims;
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста OAuth2-сервис, клаймы пользователя и тип клайма маркера доступа.
            var oauth2Service = context.Get(OAuth2ServiceValidator.ContextKey) as IOAuth2Service;
            var accessTokenClaimType = context.Get(AccessTokenClaimTypeValidator.ContextKey) as string;
            var userClaims = context.Get(UserClaimsValidator.ContextKey) as IList<Claim>;

            // Извлечь из коллекции клаймов пользователя маркер доступа к API внешнего OAuth2-провайдера.
            string accessToken = userClaims.FirstOrDefault(claim => claim.Type == accessTokenClaimType)?.Value;

            // Установить маркер доступа OAuth2-сервиса для доступа к API внешнего OAuth2-провайдера.
            oauth2Service.AccessToken = accessToken;

            // Передать управление следующему обработчику в цепочке, вернув null.
            return Task.FromResult(default(IAuthResult));
        }
    }
}
