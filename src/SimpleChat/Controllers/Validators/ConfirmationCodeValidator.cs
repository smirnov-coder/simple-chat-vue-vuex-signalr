using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Validators
{
    public class ConfirmationCodeValidator : StringValidator
    {
        public const string ContexKey = "confirmationCode";

        public ConfirmationCodeValidator() : this(null)
        {
        }

        public ConfirmationCodeValidator(IGuard guard) : base(ContexKey, guard)
        {
        }
    }
}
