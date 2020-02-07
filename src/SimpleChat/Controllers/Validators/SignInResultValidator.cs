using Microsoft.AspNetCore.Identity;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Validators
{
    public class SignInResultValidator : ObjectValidator<SignInResult>
    {
        public const string ContextKey = "signInResult";

        public SignInResultValidator() : this(null)
        {
        }

        public SignInResultValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
