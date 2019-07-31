using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace SimpleChat.Infrastructure.Helpers
{
    public static class JwtHelper
    {
        public static string GetEncodedJwt(IEnumerable<Claim> userClaims)
        {
            var today = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: JwtOptions.ISSUER,
                audience: JwtOptions.AUDIENCE,
                notBefore: today,
                claims: userClaims,
                expires: today.Add(TimeSpan.FromDays(JwtOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(JwtOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        /// <summary>
        /// Проверяет валидность JSON Web Token (JWT).
        /// </summary>
        /// <param name="encodedJwt">JWT, закодированный в компактном сериализованном формате.</param>
        /// <returns>true, если JWT является валидным; иначе false.</returns>
        public static bool IsValid(string encodedJwt)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            jwtHandler.ValidateToken(encodedJwt, GetValidationParameters(), out SecurityToken token);
            return token != null;
        }

        /// <summary>
        /// Возвращает параметры проверки валидации JSON Web Token (JWT).
        /// </summary>
        public static TokenValidationParameters GetValidationParameters() =>
            new TokenValidationParameters
            {
                // Укзывает, будет ли валидироваться издатель при валидации токена.
                ValidateIssuer = true,
                // Строка, представляющая издателя.
                ValidIssuer = JwtOptions.ISSUER,
                // Будет ли валидироваться потребитель токена.
                ValidateAudience = true,
                // Установка потребителя токена.
                ValidAudience = JwtOptions.AUDIENCE,
                // Будет ли валидироваться время существования.
                ValidateLifetime = true,
                // Установка ключа безопасности.
                IssuerSigningKey = JwtOptions.GetSymmetricSecurityKey(),
                // Валидация ключа безопасности.
                ValidateIssuerSigningKey = true
            };
    }
}
