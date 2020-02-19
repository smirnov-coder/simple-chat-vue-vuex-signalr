using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Constants
{
    /// <summary>
    /// Содержит значения ключей в виде строковых констант, по которым в файле настроек приложения (appsettings.json)
    /// доступны различные параметры доступа к API внешних OAuth2-провайдеров, используемых в приложении.
    /// </summary>
    public static class ConfigurationKeys
    {
        public const string FacebookAppId = "Authentication:Facebook:AppId";
        public const string FacebookAppSecret = "Authentication:Facebook:AppSecret";
        public const string OdnoklassnikiApplicationId = "Authentication:Odnoklassniki:ApplicationId";
        public const string OdnoklassnikiApplicationSecretKey = "Authentication:Odnoklassniki:ApplicationSecretKey";
        public const string OdnoklassnikiApplicationKey = "Authentication:Odnoklassniki:ApplicationKey";
        public const string VKontakteClientId = "Authentication:VKontakte:ClientId";
        public const string VKontakteClientSecret = "Authentication:VKontakte:ClientSecret";
    }
}
