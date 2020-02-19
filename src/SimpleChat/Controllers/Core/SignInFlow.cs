using Microsoft.Extensions.DependencyInjection;
using SimpleChat.Controllers.Handlers;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Core
{
    /// <inheritdoc cref="ISignInFlow"/>
    public class SignInFlow : ChainOfResponsibility, ISignInFlow
    {
        public SignInFlow(IServiceProvider serviceProvider) : this(serviceProvider, null)
        {
        }

        public SignInFlow(IServiceProvider serviceProvider, IGuard guard) : base(guard)
        {
            _guard.EnsureObjectParamIsNotNull(serviceProvider, nameof(serviceProvider));

            AddHandler(serviceProvider.GetRequiredService<ValidateAuthorizationCode>());
            AddHandler(serviceProvider.GetRequiredService<ValidateState>());
            AddHandler(serviceProvider.GetRequiredService<ConfigureOAuth2ServiceForSignIn>());
            AddHandler(serviceProvider.GetRequiredService<RequestAccessToken>());
            AddHandler(serviceProvider.GetRequiredService<RequestUserInfo>());
            AddHandler(serviceProvider.GetRequiredService<ValidateEmailAddress>());
            AddHandler(serviceProvider.GetRequiredService<ExternalLoginSignIn>());
            AddHandler(serviceProvider.GetRequiredService<CreateConfirmSignInResult>());
            AddHandler(serviceProvider.GetRequiredService<FetchIdentityUser>());
            AddHandler(serviceProvider.GetRequiredService<FetchUserClaims>());
            AddHandler(serviceProvider.GetRequiredService<RefreshUserClaims>());
            AddHandler(serviceProvider.GetRequiredService<PrepareForSignInReturn>());
            AddHandler(serviceProvider.GetRequiredService<CreateSuccessResult>());
        }

    }
}
