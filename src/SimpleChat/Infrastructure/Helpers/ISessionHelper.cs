using SimpleChat.Models;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с сессией пользователя.
    /// </summary>
    public interface ISessionHelper
    {
        /// <summary>
        /// Асинхронно сохраняет в текущей сессии информацию о пользователе внешнего OAuth2-провайдера.
        /// </summary>
        /// <param name="userInfo">Информация о пользователе внешнего OAuth2-провайдера.</param>
        /// <returns>
        /// Идентификатор сессии (ключ, по которому будут доступны данные) в виде строки <see cref="string"/>.
        /// </returns>
        Task<string> SaveUserInfoAsync(ExternalUserInfo userInfo);

        /// <summary>
        /// Извлекает по идентификатору сессии (ключу) из текущей сессии информацию о пользователе внешнего
        /// OAuth2-провайдера.
        /// </summary>
        /// <param name="sessionId">Значение идентификатора сессии.</param>
        ExternalUserInfo FetchUserInfo(string sessionId);

        /// <summary>
        /// Удаляет все данные из текущей сессии.
        /// </summary>
        void ClearSession();

        /// <summary>
        /// Проверяет, существует ли в текущей сессии запись по заданному значению ключа (идентификатора сессии).
        /// </summary>
        /// <param name="sessionId">Значение идентификатора сессии.</param>
        /// <returns>true, если запись существует; иначе false</returns>
        bool SessionExists(string sessionId);
    }
}
