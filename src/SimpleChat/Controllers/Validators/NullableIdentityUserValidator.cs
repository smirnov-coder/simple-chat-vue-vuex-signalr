using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Validators
{
    public class NullableIdentityUserValidator : IdentityUserValidator
    {
        public override bool Validate(IContext context, ICollection<string> errors)
        {
            _guard.EnsureObjectParamIsNotNull(context, nameof(context));
            _guard.EnsureObjectParamIsNotNull(errors, nameof(errors));

            if (errors.Any())
                throw new InvalidOperationException($"Коллекция '{nameof(errors)}' не пуста.");

            if (!context.ContainsKey(_key))
                errors.Add($"В контексте отсутствует значение по ключу '{_key}'.");
            else if (context.Get(_key) != null)
                InternalValidate(context, errors);

            return !errors.Any();
        }
    }
}
