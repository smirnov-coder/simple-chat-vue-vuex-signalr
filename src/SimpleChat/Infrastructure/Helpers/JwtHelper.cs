using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace SimpleChat.Infrastructure.Helpers
{
    /// <inheritdoc cref="IJwtHelper"/>
    public class JwtHelper : IJwtHelper
    {
        private JwtOptions _options;
        private JwtSecurityTokenHandler _tokenHandler;
        private IGuard _guard;

        public JwtHelper() : this(null, null, null)
        {
        }

        public JwtHelper(JwtOptions options, JwtSecurityTokenHandler tokenHandler) : this(options, tokenHandler, null)
        {
        }

        public JwtHelper(JwtOptions options, JwtSecurityTokenHandler tokenHandler, IGuard guard)
        {
            _guard = guard ?? new Guard();
            _options = options ?? JwtOptions.Default;
            _tokenHandler = tokenHandler ?? new JwtSecurityTokenHandler();
        }

        public string CreateEncodedJwt(IEnumerable<Claim> userClaims)
        {
            _guard.EnsureObjectParamIsNotNull(userClaims, nameof(userClaims));
            if (!userClaims.Any())
                throw new ArgumentException("Коллекция клаймов пуста.", nameof(userClaims));

            var today = GetCurrentDateTime();
            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                notBefore: today,
                claims: userClaims,
                expires: today.Add(_options.Lifetime),
                signingCredentials: new SigningCredentials(_options.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));
            var encodedJwt = _tokenHandler.WriteToken(jwt);

            return encodedJwt;
        }

        protected virtual DateTime GetCurrentDateTime() => DateTime.UtcNow;

        public bool ValidateToken(string encodedJwt)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(encodedJwt, nameof(encodedJwt));
            _tokenHandler.ValidateToken(encodedJwt, GetValidationParameters(), out SecurityToken token);
            return token != null;
        }

        public TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters
            {
                // Укзывает, будет ли валидироваться издатель при валидации токена.
                ValidateIssuer = true,

                // Строка, представляющая издателя.
                ValidIssuer = _options.Issuer,

                // Будет ли валидироваться потребитель токена.
                ValidateAudience = true,

                // Установка потребителя токена.
                ValidAudience = _options.Audience,

                // Будет ли валидироваться время существования.
                ValidateLifetime = true,

                // Установка ключа безопасности.
                IssuerSigningKey = _options.GetSymmetricSecurityKey(),

                // Валидация ключа безопасности.
                ValidateIssuerSigningKey = true
            };
        }
    }
}
