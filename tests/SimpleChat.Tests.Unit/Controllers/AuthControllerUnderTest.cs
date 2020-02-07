using SimpleChat.Controllers;
using SimpleChat.Controllers.Core;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace SimpleChat.Tests.Unit.Controllers
{
    internal class AuthControllerUnderTest : AuthController
    {
        public AuthControllerUnderTest(
            IAuthenticationFlow authenticationFlow,
            ISignInFlow signInFlow,
            IConfirmSignInFlow confirmSignInFlow,
            IContextBuilder contextBuilder)
            : base(authenticationFlow, signInFlow, confirmSignInFlow, contextBuilder)
        {
        }

        public ClaimsPrincipal GetCurrentUserReturns { get; set; }

        protected override ClaimsPrincipal GetCurrentUser() => GetCurrentUserReturns;
    }
}
