using SimpleChat.Infrastructure.Constants;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleChat.Tests.Unit
{
    public class RequestUserBuilder
    {
        private List<Claim> _claims;

        public RequestUserBuilder()
        {
            _claims = new List<Claim>();
        }

        public ClaimsPrincipal Build()
        {
            return new ClaimsPrincipal(new ClaimsIdentity(_claims));
        }

        public RequestUserBuilder WithProviderClaim(string value)
        {
            _claims.Add(new Claim(CustomClaimTypes.Provider, value));
            return this;
        }

        public RequestUserBuilder WithNameClaim(string value)
        {
            _claims.Add(new Claim(ClaimTypes.Name, value));
            return this;
        }
    }
}
