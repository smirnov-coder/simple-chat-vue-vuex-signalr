using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;

namespace SimpleChat.Tests.Unit.Validators
{
    internal class ValidatorBaseUnderTest : ValidatorBase
    {
        public ValidatorBaseUnderTest(string key, IGuard guard) : base(key, guard)
        {
        }

        protected override void InternalValidate(IContext context, ICollection<string> errors)
        {
            InternalValidateCallback?.Invoke(context, errors);
        }

        public Action<IContext, ICollection<string>> InternalValidateCallback { get; set; }
    }
}
