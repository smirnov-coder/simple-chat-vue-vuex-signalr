using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с JWT (JSON Web Token).
    /// </summary>
    public interface IJwtHelper
    {
        /// <summary>
        /// Создаёт новый JWT на основе коллекции клаймов пользователя.
        /// </summary>
        /// <param name="userClaims">Коллекция клаймов пользователя.</param>
        /// <returns>JWT компактного сериализуемого формата в виде строки <see cref="string"/>.</returns>
        string CreateEncodedJwt(IEnumerable<Claim> userClaims);

        /// <summary>
        /// Проверяет валидность JWT.
        /// </summary>
        /// <param name="encodedJwt">JWT компактного сериализуемого формата в виде строки <see cref="string"/>.</param>
        /// <returns>true, если JWT валиден; иначе false</returns>
        bool ValidateToken(string encodedJwt);

        /// <summary>
        /// Возвращает параметры, используемые для создания нового JWT и валидации JWT.
        /// </summary>
        TokenValidationParameters GetValidationParameters();
    }
}
