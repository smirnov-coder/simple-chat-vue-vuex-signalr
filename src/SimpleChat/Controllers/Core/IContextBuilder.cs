using Microsoft.AspNetCore.Identity;
using SimpleChat.Models;
using SimpleChat.Services;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleChat.Controllers.Core
{
    public interface IContextBuilder
    {
        IContext Build();

        IContextBuilder WithAccessTokenClaimType(string accessTokenClaimType);

        IContextBuilder WithAuthorizationCode(string authorizationCode);

        IContextBuilder WithAvatarClaimType(string avatarClaimType);

        IContextBuilder WithConfirmationCode(string confirmationCode);

        IContextBuilder WithExternalUserInfo(ExternalUserInfo userInfo);

        IContextBuilder WithIdentityUser(IdentityUser identityUser);

        IContextBuilder WithNameClaimType(string nameClaimType);

        IContextBuilder WithOAuth2Service(IOAuth2Service oauth2Service);

        IContextBuilder WithProvider(string provider);

        IContextBuilder WithRequestUser(ClaimsPrincipal requestUser);

        IContextBuilder WithSessionId(string sessionId);

        IContextBuilder WithState(string state);

        IContextBuilder WithUserClaims(IList<Claim> userClaims);

        IContextBuilder WithUserName(string userName);
    }
}
