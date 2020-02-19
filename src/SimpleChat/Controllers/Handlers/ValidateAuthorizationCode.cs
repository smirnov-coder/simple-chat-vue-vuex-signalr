using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    /// <summary>
    /// Представляет собой процесс проверки пригодности для использования кода авторизации, полученного от внешнего
    /// OAuth2-провайдера.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class ValidateAuthorizationCode : HandlerBase
    {
        private AuthorizationCodeValidator _authorizationCodeValidator;
        private ProviderValidator _providerValidator;

        #region Constructors
        public ValidateAuthorizationCode(
            AuthorizationCodeValidator authorizationCodeValidator,
            ProviderValidator providerValidator)
            : this(authorizationCodeValidator, providerValidator, null)
        {
        }

        public ValidateAuthorizationCode(
            AuthorizationCodeValidator authorizationCodeValidator,
            ProviderValidator providerValidator,
            IGuard guard)
            : base(guard)
        {
            _authorizationCodeValidator = _guard.EnsureObjectParamIsNotNull(authorizationCodeValidator,
                nameof(authorizationCodeValidator));
            _providerValidator = _guard.EnsureObjectParamIsNotNull(providerValidator, nameof(providerValidator));
        }
        #endregion

        protected override bool CanHandle(IContext context)
        {
            bool canUseAuthorizationCode = _authorizationCodeValidator.Validate(context, _errors);
            bool canUseProvider = _providerValidator.Validate(context, _errors);
            return canUseAuthorizationCode && canUseProvider;
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            //string
            //    authorizationCode = context.Get(AuthorizationCodeValidator.ContextKey) as string,
            //    provider = context.Get(ProviderValidator.ContextKey) as string;

            //IAuthResult result = null;
            //if (string.IsNullOrWhiteSpace(authorizationCode))
            //    result = new ErrorResult($"Не удалось получить код авторизации от внешнего провайдера '{provider}'.");

            // Ничего не делаем, т.к. наличие и пригодность для использования кода авторизации уже проверил
            // соответствующий валидатор. Просто передаём управление следующему обработчику, вернув null.
            return Task.FromResult(default(IAuthResult));
        }
    }
    /// TODO: переделать
}
