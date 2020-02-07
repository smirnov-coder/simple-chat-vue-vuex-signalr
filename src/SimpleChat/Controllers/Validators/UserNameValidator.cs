using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Validators
{
    public class UserNameValidator : StringValidator
    {
        public const string ContextKey = "userName";

        public UserNameValidator() : this(null)
        {
        }

        public UserNameValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
