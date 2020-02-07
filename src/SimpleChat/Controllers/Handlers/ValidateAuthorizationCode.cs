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
    public class ValidateAuthorizationCode : Handler
    {
        private AuthorizationCodeValidator _authorizationCodeValidator;
        private ProviderValidator _providerValidator;

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

        protected override bool CanHandle(IContext context)
        {
            bool isAuthorizationCodeValid = _authorizationCodeValidator.Validate(context, _errors);
            bool isProviderValid = _providerValidator.Validate(context, _errors);
            return isAuthorizationCodeValid && isProviderValid;
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            string
                authorizationCode = context.Get(AuthorizationCodeValidator.ContextKey) as string,
                provider = context.Get(ProviderValidator.ContextKey) as string;

            IAuthResult result = null;
            if (string.IsNullOrWhiteSpace(authorizationCode))
                result = new ErrorResult($"Не удалось получить код авторизации от внешнего провайдера '{provider}'.");
            return Task.FromResult(result);
        }
    }
}
