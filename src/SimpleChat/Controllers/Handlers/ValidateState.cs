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
    public class ValidateState : Handler
    {
        private StateValidator _stateValidator;
        private ProviderValidator _providerValidator;

        public ValidateState(StateValidator stateValidator, ProviderValidator providerValidator)
            : this(stateValidator, providerValidator, null)
        {
        }

        public ValidateState(StateValidator stateValidator, ProviderValidator providerValidator, IGuard guard)
            : base(guard)
        {
            _stateValidator = _guard.EnsureObjectParamIsNotNull(stateValidator, nameof(stateValidator));
            _providerValidator = _guard.EnsureObjectParamIsNotNull(providerValidator, nameof(providerValidator));
        }

        protected override bool CanHandle(IContext context)
        {
            bool isStateValid = _stateValidator.Validate(context, _errors);
            bool isProviderValid = _providerValidator.Validate(context, _errors);
            return isStateValid && isProviderValid;
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            string
                state = context.Get(StateValidator.ContextKey) as string,
                provider = context.Get(ProviderValidator.ContextKey) as string;

            IAuthResult result = null;
            if (state != provider)
                result = new ErrorResult($"Внешний провайдер '{provider}' вернул неправильный 'state'.");
            return Task.FromResult(result);
        }
    }
}
