using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Абстрактный базовый класс валидатора, реализующий интерфейс <see cref="IValidator"/>.
    /// </summary>
    /// <inheritdoc cref="IValidator"/>
    public abstract class ValidatorBase : IValidator
    {
        protected IGuard _guard;
        /// <summary>
        /// Ключ, по которому в контексте <see cref="IContext"/> доступны данные для проверки.
        /// </summary>
        /// <remarks>
        /// Хочется иметь в каждом производном классе валидатора константное поле ключа для удобной работы
        /// IntelliSense. Данное закрытое поле служит как источник xml-комментария к константному полю.
        /// </remarks>
        protected string _key;

        /// <summary>
        /// Основной конструктор с одним параметром.
        /// </summary>
        /// <param name="key">Ключ, по которому в контексте <see cref="IContext"/> доступны данные для проверки.</param>
        public ValidatorBase(string key) : this(key, null)
        {
        }

        /// <summary>
        /// Вспомогательный конструктор с двумя параметрами. Используется для тестирования.
        /// </summary>
        /// <param name="guard">Компонент, инкапсулирующий логику валидации входных параметров.</param>
        /// <inheritdoc cref="ValidatorBase(string)"/>
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

        /// <summary>
        /// Выполняет проверку пригодности для использования значения данных, хранящегося в контексте
        /// <see cref="IContext"/>, специфичную для каждого производного класса.
        /// </summary>
        /// <inheritdoc cref="IValidator.Validate"/>
        protected abstract void InternalValidate(IContext context, ICollection<string> errors);
    }
}
