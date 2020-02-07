using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Validators
{
    public class SessionIdValidator : StringValidator
    {
        public const string ContextKey = "sessionId";

        public SessionIdValidator() : this(null)
        {
        }

        public SessionIdValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
