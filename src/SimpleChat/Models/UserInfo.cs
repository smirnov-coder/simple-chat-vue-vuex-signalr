using System;

namespace SimpleChat.Models
{
    /// <summary>
    /// Информация о текущем пользователе чата.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// Идентификатор пользователя (identity id).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Полное имя пользователя.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Аватар пользователя (веб-путь к файлу изображения).
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// Имя внешнего OAuth2-провайдера, через который пользователь вошёл на сайт.
        /// </summary>
        public string Provider { get; set; }
    }
}
