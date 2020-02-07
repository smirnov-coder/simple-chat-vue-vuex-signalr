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
    public class ConfigureOAuth2ServiceForAuthentication : Handler
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
            bool isOAuth2ServiceValid = _oauth2ServiceValidator.Validate(context, _errors);
            bool isAccessTokenClaimTypeValid = _accessTokenClaimTypeValidator.Validate(context, _errors);
            bool isUserClaimsValid = _userClaimsValidator.Validate(context, _errors);
            return isOAuth2ServiceValid && isAccessTokenClaimTypeValid && isUserClaimsValid;
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            var oauth2Service = context.Get(OAuth2ServiceValidator.ContextKey) as IOAuth2Service;
            var accessTokenClaimType = context.Get(AccessTokenClaimTypeValidator.ContextKey) as string;
            var userClaims = context.Get(UserClaimsValidator.ContextKey) as IList<Claim>;

            string accessToken = userClaims.FirstOrDefault(claim => claim.Type == accessTokenClaimType)?.Value;
            oauth2Service.AccessToken = accessToken;
            return Task.FromResult(default(IAuthResult));
        }
    }
}
