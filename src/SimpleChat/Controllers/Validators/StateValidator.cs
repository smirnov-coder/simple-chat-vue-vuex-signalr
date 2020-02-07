using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Validators
{
    public class StateValidator : StringValidator
    {
        public const string ContextKey = "state";

        public StateValidator() : this(null)
        {
        }

        public StateValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
