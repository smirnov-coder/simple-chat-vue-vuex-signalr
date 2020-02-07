using Microsoft.AspNetCore.Identity;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Extensions;
using SimpleChat.Models;
using SimpleChat.Services;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleChat.Controllers.Core
{
    public class ContextBuilder : IContextBuilder
    {
        private IContext _context;

        public ContextBuilder()
        {
            _context = new Context();
        }

        public IContext Build() => _context;

        public IContextBuilder WithRequestUser(ClaimsPrincipal requestUser)
        {
            _context.Set(RequestUserValidator.ContextKey, requestUser);
            return this;
        }

        public IContextBuilder WithOAuth2Service(IOAuth2Service oauth2Service)
        {
            _context.Set(OAuth2ServiceValidator.ContextKey, oauth2Service);
            return this;
        }

        public IContextBuilder WithAccessTokenClaimType(string accessTokenClaimType)
        {
            _context.Set(AccessTokenClaimTypeValidator.ContextKey, accessTokenClaimType);
            return this;
        }

        public IContextBuilder WithNameClaimType(string nameClaimType)
        {
            _context.Set(NameClaimTypeValidator.ContextKey, nameClaimType);
            return this;
        }

        public IContextBuilder WithAvatarClaimType(string avatarClaimType)
        {
            _context.Set(AvatarClaimTypeValidator.ContextKey, avatarClaimType);
            return this;
        }

        public IContextBuilder WithUserClaims(IList<Claim> userClaims)
        {
            _context.Set(UserClaimsValidator.ContextKey, userClaims);
            return this;
        }

        public IContextBuilder WithIdentityUser(IdentityUser identityUser)
        {
            _context.Set(IdentityUserValidator.ContextKey, identityUser);
            return this;
        }

        public IContextBuilder WithExternalUserInfo(ExternalUserInfo userInfo)
        {
            _context.Set(UserInfoValidator.ContextKey, userInfo);
            return this;
        }

        public IContextBuilder WithProvider(string provider)
        {
            _context.Set(ProviderValidator.ContextKey, provider);
            return this;
        }

        public IContextBuilder WithAuthorizationCode(string authorizationCode)
        {
            _context.Set(AuthorizationCodeValidator.ContextKey, authorizationCode);
            return this;
        }

        public IContextBuilder WithState(string state)
        {
            _context.Set(StateValidator.ContextKey, state);
            return this;
        }

        public IContextBuilder WithSessionId(string sessionId)
        {
            _context.Set(SessionIdValidator.ContextKey, sessionId);
            return this;
        }

        public IContextBuilder WithConfirmationCode(string confirmationCode)
        {
            _context.Set(ConfirmationCodeValidator.ContexKey, confirmationCode);
            return this;
        }

        public IContextBuilder WithUserName(string userName)
        {
            _context.Set(UserNameValidator.ContextKey, userName);
            return this;
        }
    }
}
