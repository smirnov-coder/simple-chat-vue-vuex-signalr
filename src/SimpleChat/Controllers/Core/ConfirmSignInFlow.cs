using Microsoft.Extensions.DependencyInjection;
using SimpleChat.Controllers.Handlers;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Core
{
    public class ConfirmSignInFlow : ChainOfResponsibility, IConfirmSignInFlow
    {
        public ConfirmSignInFlow(IServiceProvider serviceProvider) : this(serviceProvider, null)
        {
        }

        public ConfirmSignInFlow(IServiceProvider serviceProvider, IGuard guard) : base(guard)
        {
            _guard.EnsureObjectParamIsNotNull(serviceProvider, nameof(serviceProvider));

            AddHandler(serviceProvider.GetRequiredService<ValidateSession>());
            AddHandler(serviceProvider.GetRequiredService<ValidateConfirmationCode>());

            var fetchIdentityUser = serviceProvider.GetRequiredService<FetchIdentityUser>();
            fetchIdentityUser.IsNullValid = true;
            AddHandler(fetchIdentityUser);

            AddHandler(serviceProvider.GetRequiredService<CreateIdentityUserIfNotExist>());
            AddHandler(serviceProvider.GetRequiredService<PickUpProviderData>());
            AddHandler(serviceProvider.GetRequiredService<AddUserClaims>());
            AddHandler(serviceProvider.GetRequiredService<AddExternalLogin>());
            AddHandler(serviceProvider.GetRequiredService<PrepareForConfirmSignInReturn>());
            AddHandler(serviceProvider.GetRequiredService<CreateSuccessResult>());
        }
    }
}
