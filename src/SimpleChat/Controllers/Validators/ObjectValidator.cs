using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace SimpleChat.Controllers.Validators
{
    public class ObjectValidator<T> : ValidatorBase
        where T : class
    {
        public ObjectValidator(string key) : this(key, null)
        {
        }

        public ObjectValidator(string key, IGuard guard) : base(key, guard)
        {
        }

        protected override void InternalValidate(IContext context, ICollection<string> errors)
        {
            object value = context.Get(_key);
            if (value as T == null)
            {
                errors.Add($"Неверный тип значения по ключу '{_key}' в контексте. Ожидаемый тип: " +
                    $"{typeof(T).FullName}. Фактический тип: {value.GetType().FullName}.");
            }
        }
    }
}
