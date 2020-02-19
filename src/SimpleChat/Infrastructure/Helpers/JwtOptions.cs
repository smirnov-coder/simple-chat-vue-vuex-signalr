using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace SimpleChat.Infrastructure.Helpers
{
    /// <summary>
    /// Содержит различные параметры для создания и валидации JWT (JSON Web Token).
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// Издатель JWT.
        /// </summary>
        public virtual string Issuer { get; set; }

        /// <summary>
        /// Потребитель JWT.
        /// </summary>
        public virtual string Audience { get; set; }

        /// <summary>
        /// Ключ, используемый для подписи JWT.
        /// </summary>
        public virtual string SigningKey { get; set; }

        /// <summary>
        /// Срок действия JWT.
        /// </summary>
        public virtual TimeSpan Lifetime { get; set; }

        /// <summary>
        /// Параметры JWT по умолчанию.
        /// </summary>
        public static JwtOptions Default { get; } = new JwtOptions
        {
            Issuer = "SimpleChat",
            Audience = "SimpleChat",
            SigningKey = "mysupersecret_secretkey!123",
            Lifetime = TimeSpan.FromDays(10)
        };

        public virtual SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SigningKey));
        }
    }
}
