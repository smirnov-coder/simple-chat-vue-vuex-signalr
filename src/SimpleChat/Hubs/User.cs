using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Hubs
{
    /// <summary>
    /// Пользователь чата <see cref="ChatHub"/>.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Идентификатор пользователя чата.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Полное имя пользователя чата.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Аватар пользователя чата (веб-путь к файлу изображения).
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// Имя внешнего OAuth2-провайдера, через который пользователь чата вошёл на наш сайт.
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Коллекция подключений пользователя к чату.
        /// </summary>
        public HashSet<string> ConnectionIds { get; set; } = new HashSet<string>();
    }
}
