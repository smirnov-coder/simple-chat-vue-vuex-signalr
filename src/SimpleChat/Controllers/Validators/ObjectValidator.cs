using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Helpers;
using System.Collections.Generic;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования значения данных ссылочного типа
    /// <typeparamref name="T"/>, хранящегося в контексте <see cref="IContext"/>.
    /// </summary>
    /// <typeparam name="T">Тип значения данных, хранящегося в контексте <see cref="IContext"/>.</typeparam>
    public class ObjectValidator<T> : ValidatorBase
        where T : class
    {
        /// <inheritdoc cref="ValidatorBase(string)"/>
        public ObjectValidator(string key) : this(key, null)
        {
        }

        /// <inheritdoc cref="ValidatorBase(string, IGuard)"/>
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
