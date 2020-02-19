using System;

namespace SimpleChat.Models
{
    /// <summary>
    /// Информация о пользователе внешнего OAuth2-провайдера.
    /// </summary>
    public class ExternalUserInfo
    {
        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Полное имя пользователя.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// E-mail адрес пользователя.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Фотография профайла пользователя (веб-путь к файлу изображения).
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        /// Имя внешнего OAuth2-провайдера.
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Маркер доступа для выполнения запросов к API внешнего провайдера.
        /// </summary>
        public string AccessToken { get; set; }
    }
}
