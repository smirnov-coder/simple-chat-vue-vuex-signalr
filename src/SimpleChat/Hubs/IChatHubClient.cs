using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleChat.Models;

namespace SimpleChat.Hubs
{
    /// <summary>
    /// Определяет набор событий чат-хаба, на которые пользователь хаба может подписываться с помощью объекта
    /// "connection" из JavaScript-пакета "aspnet/signalr".
    /// </summary>
    public interface IChatHubClient
    {
        /// <summary>
        /// Событие получения нового сообщения чата.
        /// </summary>
        /// <param name="message">Новое сообщение чата.</param>
        Task ReceiveMessage(ChatMessage message);

        /// <summary>
        /// Событие подключения к чату нового пользователя.
        /// </summary>
        /// <param name="user">Новый пользователь чата.</param>
        Task NewUser(User user);

        /// <summary>
        /// Событие получения списка всех подключённых к чату пользователей.
        /// </summary>
        /// <param name="ownConnectionIds">
        /// Коллекция идентификаторов собственных подключений к чату текущего пользователя. Каждый пользователь может
        /// иметь больше одного подключения - фича SignalR для возможности одновременного подключения одного аккаунта
        /// с разных устройств, например, desktop и mobile.
        /// </param>
        /// <param name="users">Коллекция пользователей, подключённых к чату.</param>
        Task ConnectedUsers(IEnumerable<string> ownConnectionIds, IEnumerable<User> users);

        /// <summary>
        /// Событие нового подключения пользователя к чату (например, с мобильного устройства).
        /// </summary>
        /// <param name="userId">Идентификатор пользователя чата.</param>
        /// <param name="connectionId">Идентификатор подключения к чату.</param>
        Task NewUserConnection(string userId, string connectionId);

        /// <summary>
        /// Событие отключения пользователя от чата.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <param name="connectionId">Идентификатор подключения.</param>
        Task DisconnectedUser(string userId, string connectionId);

        /// <summary>
        /// Событие, информирующее клиента о необходимости принудительного разрыва подключения к чату. Используемая
        /// версия SignalR не позволяет управлять подключениями вручную на стороне сервера :(
        /// </summary>
        Task ForceSignOut();
    }
}
