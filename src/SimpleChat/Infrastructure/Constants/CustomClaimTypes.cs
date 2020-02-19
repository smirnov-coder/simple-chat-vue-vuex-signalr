using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Constants
{
    /// <summary>
    /// Пользовательские типы клаймов для хранения небольших порций информации в хранилище.
    /// </summary>
    public class CustomClaimTypes
    {
        /// <summary>
        /// Полное имя пользователя (имя + фамилия).
        /// </summary>
        public const string FullName = "full_name";

        /// <summary>
        /// Аватар пользователя (веб-путь к файлу изображения).
        /// </summary>
        public const string Avatar = "avatar";

        /// <summary>
        /// Имя внешнего OAuth2-провайдера.
        /// </summary>
        public const string Provider = "provider";

        /// <summary>
        /// Полное имя пользователя соц. сети "ВКонтакте".
        /// </summary>
        public const string VKontakteName = "vkontakte:name";

        /// <summary>
        /// Аватар пользователя соц. сети "ВКонтакте".
        /// </summary>
        public const string VKontakteAvatar = "vkontakte:avatar";

        /// <summary>
        /// Маркер доступа к API соц. сети "ВКонтакте".
        /// </summary>
        public const string VKontakteAccessToken = "vkontakte:access_token";

        /// <summary>
        /// Полное имя пользователя соц. сети "Facebook".
        /// </summary>
        public const string FacebookName = "facebook:name";

        /// <summary>
        /// Аватар пользователя соц. сети "Facebook".
        /// </summary>
        public const string FacebookAvatar = "facebook:avatar";

        /// <summary>
        /// Маркер доступа к API соц. сети "Facebook".
        /// </summary>
        public const string FacebookAccessToken = "facebook:access_token";

        /// <summary>
        /// Полное имя пользователя соц. сети "Одноклассники".
        /// </summary>
        public const string OdnoklassnikiName = "odnoklassniki:name";

        /// <summary>
        /// Аватар пользователя соц. сети "Одноклассники".
        /// </summary>
        public const string OdnoklassnikiAvatar = "odnoklassniki:avatar";

        /// <summary>
        /// Маркер доступа к API соц. сети "Одноклассники".
        /// </summary>
        public const string OdnoklassnikiAccessToken = "odnoklassniki:access_token";
    }
}
