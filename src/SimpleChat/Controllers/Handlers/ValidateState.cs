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
    /// Представляет собой процесс проверки правильности строки состояния, которую вернул внешний OAuth2-провайдер при
    /// обратном вызове.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class ValidateState : HandlerBase
    {
        private StateValidator _stateValidator;
        private ProviderValidator _providerValidator;

        #region Constructors
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
        #endregion

        protected override bool CanHandle(IContext context)
        {
            bool canUseState = _stateValidator.Validate(context, _errors);
            bool canUseProvider = _providerValidator.Validate(context, _errors);
            return canUseState && canUseProvider;
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста имя внешнего провайдера и строку состояния.
            string
                state = context.Get(StateValidator.ContextKey) as string,
                provider = context.Get(ProviderValidator.ContextKey) as string;

            // В качестве строки состояния использовалось имя соответствующего внешнего провайдера. Значит, если строка
            // состояния не равна имени провайдера, то прервать цепочку обработчиков и вернуть сообщение об ошибке
            IAuthResult result = null;
            if (state != provider)
                result = new ErrorResult($"Внешний провайдер '{provider}' вернул неправильный 'state'.");

            return Task.FromResult(result);
        }
    }
}
