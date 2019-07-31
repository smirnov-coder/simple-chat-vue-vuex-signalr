using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace SimpleChat.Infrastructure.Helpers
{
    /// <summary>
    /// Содержит различные параметры для создания и валидации JSON Web Token (JWT).
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// Издатель JWT.
        /// </summary>
        public const string ISSUER = "SimpleChat";

        /// <summary>
        /// Потребитель JWT.
        /// </summary>
        public const string AUDIENCE = "SimpleChat";

        /// <summary>
        /// Ключ, используемый для подписи JWT.
        /// </summary>
        const string KEY = "mysupersecret_secretkey!123";

        /// <summary>
        /// Время жизни JWT в условных единицах (минуты, часы, дни и т.д.).
        /// </summary>
        public const int LIFETIME = 10;

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
