using SimpleChat.Controllers.Core;
using System.Collections.Generic;

namespace SimpleChat.Controllers.Validators
{
    /// <summary>
    /// Инкапсулирует логику проверки пригодности для использования значения данных, хранящегося в контексте
    /// <see cref="IContext"/>.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Проверяет, пригодно ли для использования значение данных, хранящееся в контексте.
        /// </summary>
        /// <param name="context">Контекст выполняемой операции.</param>
        /// <param name="errors">Коллекция ошибок, возникших в ходе проверки.</param>
        /// <returns>true, если данные пригодны для использования; иначе false</returns>
        bool Validate(IContext context, ICollection<string> errors);
    }
}
