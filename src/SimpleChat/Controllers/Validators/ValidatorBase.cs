using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Validators
{
    public abstract class ValidatorBase : IValidator
    {
        protected IGuard _guard;
        protected string _key;

        public ValidatorBase(string key) : this(key, null)
        {
        }

        public ValidatorBase(string key, IGuard guard)
        {
            _guard = guard ?? new Guard();
            _key = _guard.EnsureStringParamIsNotNullOrEmpty(key, nameof(key));
        }

        public virtual bool Validate(IContext context, ICollection<string> errors)
        {
            _guard.EnsureObjectParamIsNotNull(context, nameof(context));
            _guard.EnsureObjectParamIsNotNull(errors, nameof(errors));

            if (errors.Any())
                throw new InvalidOperationException($"Коллекция '{nameof(errors)}' не пуста.");

            if (!context.ContainsKey(_key))
                errors.Add($"В контексте отсутствует значение по ключу '{_key}'.");
            else if (context.Get(_key) == null)
                errors.Add($"Значение по ключу '{_key}' равно null.");
            else
                InternalValidate(context, errors);

            return !errors.Any();
        }

        protected abstract void InternalValidate(IContext context, ICollection<string> errors);
    }
}
