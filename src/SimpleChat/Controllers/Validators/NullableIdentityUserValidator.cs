using SimpleChat.Controllers.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования информации о Identity-пользователе, хранящейся в
    /// контексте <see cref="IContext"/>. Валидатор допускает значение null.
    /// </summary>
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
