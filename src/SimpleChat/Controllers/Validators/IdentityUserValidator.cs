using Microsoft.AspNetCore.Identity;
using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleChat.Controllers.Validators
{
    public class IdentityUserValidator : ObjectValidator<IdentityUser>
    {
        public const string ContextKey = "identityUser";

        public IdentityUserValidator() : this(null)
        {
        }

        public IdentityUserValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
