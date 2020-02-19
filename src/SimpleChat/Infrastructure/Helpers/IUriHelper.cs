using System.Collections.Generic;

namespace SimpleChat.Infrastructure.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с Uri/Url.
    /// </summary>
    public interface IUriHelper
    {
        /// <summary>
        /// Добавляет строку запроса к Uri.
        /// </summary>
        /// <param name="uri">
        /// Uri для добавления строки запроса в виде строки <see cref="string"/>. Может быть пустой строкой.
        /// </param>
        /// <param name="queryParams">Коллекция параметров строки запроса.</param>
        /// <returns>Uri с кодированными параметрами запроса в виде строки <see cref="string"/>.</returns>
        string AddQueryString(string uri, IDictionary<string, string> queryParams);

        /// <summary>
        /// Возращает полный (включая протокол и домен), абсолютный Uri к заданному методу действия контроллера.
        /// </summary>
        /// <param name="controller">Имя контроллера.</param>
        /// <param name="action">Имя метода действия контроллера.</param>
        string GetControllerActionUri(string controller, string action);
    }
}
