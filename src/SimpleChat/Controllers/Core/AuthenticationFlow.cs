using Microsoft.Extensions.DependencyInjection;
using SimpleChat.Controllers.Handlers;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Core
{
    public class AuthenticationFlow : ChainOfResponsibility, IAuthenticationFlow
    {
        public AuthenticationFlow(IServiceProvider serviceProvider) : this(serviceProvider, null)
        {
        }

        public AuthenticationFlow(IServiceProvider serviceProvider, IGuard guard) : base(guard) 
        {
            _guard.EnsureObjectParamIsNotNull(serviceProvider, nameof(serviceProvider));

            AddHandler(serviceProvider.GetRequiredService<ValidateRequestUser>());
            AddHandler(serviceProvider.GetRequiredService<FetchIdentityUser>());
            AddHandler(serviceProvider.GetRequiredService<FetchUserClaims>());
            AddHandler(serviceProvider.GetRequiredService<PickUpProviderData>());
            AddHandler(serviceProvider.GetRequiredService<ConfigureOAuth2ServiceForAuthentication>());
            AddHandler(serviceProvider.GetRequiredService<RequestUserInfo>());
            AddHandler(serviceProvider.GetRequiredService<RefreshUserClaims>());
            AddHandler(serviceProvider.GetRequiredService<CreateAuthenticatedResult>());
        }
    }
}
