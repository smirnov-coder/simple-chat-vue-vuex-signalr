using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Helpers;
using System.Collections.Generic;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования строкового значения данных, хранящегося в контексте
    /// <see cref="IContext"/>.
    /// </summary>
    public class StringValidator : ValidatorBase
    {
        /// <inheritdoc cref="ValidatorBase(string)"/>
        public StringValidator(string key) : this(key, null)
        {
        }

        /// <inheritdoc cref="ValidatorBase(string, IGuard)"/>
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
