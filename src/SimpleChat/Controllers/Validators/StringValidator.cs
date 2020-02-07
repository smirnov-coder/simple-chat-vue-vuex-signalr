using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace SimpleChat.Controllers.Validators
{
    public class StringValidator : ValidatorBase
    {
        public StringValidator(string key) : this(key, null)
        {
        }

        public StringValidator(string key, IGuard guard) : base(key, guard)
        {
        }

        protected override void InternalValidate(IContext context, ICollection<string> errors)
        {
            object value = context.Get(_key);
            if (!(value is string))
            {
                errors.Add($"Неверный тип значения по ключу '{_key}' в контексте. Ожидаемый тип: " +
                    $"{typeof(string).FullName}. Фактический тип: {value.GetType().FullName}.");
            }
            else if (string.IsNullOrWhiteSpace(value as string))
            {
                errors.Add($"Значение по ключу '{_key}' равно пустой строке.");
            }
        }
    }
}
