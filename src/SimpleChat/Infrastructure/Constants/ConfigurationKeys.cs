using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Constants
{
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
