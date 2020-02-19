using System.Collections.Generic;

namespace SimpleChat.Hubs
{
    /// <summary>
    /// Представляет собой коллекцию пользователей чата <see cref="ChatHub"/>.
    /// </summary>
    public interface IUserCollection
    {
        /// <summary>
        /// Добавляет идентификатор нового подключения пользователя в коллекцию подключений.
        /// </summary>
        /// <param name="userId">Значение идентификатора пользователя чата.</param>
        /// <param name="connectionId">Значение идентификатора нового подключения к чату.</param>
        /// <returns>true, если удалось добавить новое подключение; иначе false</returns>
        bool AddConnection(string userId, string connectionId);

        /// <summary>
        /// Добавляет нового пользователя чата в коллекцию пользователей.
        /// </summary>
        /// <param name="user">Новый пользователь чата.</param>
        /// <returns>true, если удалось добавить нового пользователя; иначе false</returns>
        bool AddUser(User user);

        /// <summary>
        /// Извлекает из коллекции пользователей чата пользователя с заданным значением идентификатора.
        /// </summary>
        /// <param name="userId">Значение идентификатора пользователя.</param>
        /// <returns>Пользователь с заданным значением идентификатора или null, если пользователь не найден.</returns>
        User GetUser(string userId);

        /// <summary>
        /// Извлекает коллекцию всех пользователей чата.
        /// </summary>
        IEnumerable<User> GetUsers();

        /// <summary>
        /// Удаляет подключение с заданным идентификатором из коллекции подключений пользователя чата.
        /// </summary>
        /// <param name="userId">Значение идентификатора пользователя чата.</param>
        /// <param name="connectionId">Значение идентификатора удаляемого подключения к чату.</param>
        void RemoveConnection(string userId, string connectionId);

        /// <summary>
        /// Удаляет пользователя из коллекции пользователей чата.
        /// </summary>
        /// <param name="userId">Значение идентификатора удаляемого пользователя чата.</param>
        void RemoveUser(string userId);
    }
}
