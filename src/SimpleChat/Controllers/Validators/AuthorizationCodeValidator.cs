using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Validators
{
    public class AuthorizationCodeValidator : StringValidator
    {
        public const string ContextKey = "authorizationCode";

        public AuthorizationCodeValidator() : this(null)
        {
        }

        public AuthorizationCodeValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
