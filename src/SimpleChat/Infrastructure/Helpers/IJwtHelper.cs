using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    public interface IJwtHelper
    {
        string CreateEncodedJwt(IEnumerable<Claim> userClaims);

        bool ValidateToken(string encodedJwt);

        TokenValidationParameters GetValidationParameters();
    }
}
