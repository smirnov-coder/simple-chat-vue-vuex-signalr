using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Core
{
    /// <summary>
    /// Контекст запроса к <see cref="AuthController"/> является контейнером для хранения информации, необходимой для
    /// выполнения составной операции. Обработчики из цепочки обработчиков <see cref="ChainOfResponsibility"/> читают,
    /// изменяют или добавляют информацию в контекст.
    /// </summary>
    public interface IContext
    {
        /// <summary>
        /// Проверяет, содержится ли в контексте информация по заданному ключу.
        /// </summary>
        /// <param name="key">Ключ, по которому доступны данные в контексте.</param>
        /// <returns>true, если в контексте есть данные по заданному ключу; инан false</returns>
        bool ContainsKey(string key);

        /// <summary>
        /// Помещает данные в контекст по заданному ключу.
        /// </summary>
        /// <param name="key">Ключ, по которому доступны данные в контексте.</param>
        /// <param name="value">Данные, помещаемые к контекст.</param>
        void Set(string key, object value);

        /// <summary>
        /// Извлекает данные из контекста по заданному ключу.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Данные, хранящиеся в контексте по заданному ключу, либо null, если контекст не содержит данных
        /// по заданному ключу.</returns>
        object Get(string key);
    }
}
