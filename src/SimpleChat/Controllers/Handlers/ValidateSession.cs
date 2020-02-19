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
    /// Представляет собой процесс проверки наличия сессии пользователя.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class ValidateSession : HandlerBase
    {
        private SessionIdValidator _sessionIdValidator;
        private ISessionHelper _sessionHelper;

        #region Constructors
        public ValidateSession(SessionIdValidator sessionIdValidator, ISessionHelper sessionHelper)
            : this(sessionIdValidator, sessionHelper, null)
        {
        }

        public ValidateSession(SessionIdValidator sessionIdValidator, ISessionHelper sessionHelper, IGuard guard)
            : base(guard)
        {
            _sessionIdValidator = _guard.EnsureObjectParamIsNotNull(sessionIdValidator, nameof(sessionIdValidator));
            _sessionHelper = _guard.EnsureObjectParamIsNotNull(sessionHelper, nameof(sessionHelper));
        }
        #endregion

        protected override bool CanHandle(IContext context)
        {
            return _sessionIdValidator.Validate(context, _errors);
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста идентификатор сессии.
            string sessionId = context.Get(SessionIdValidator.ContextKey) as string;

            // Если сессия с заданным идентификатором не существует, то прервать цепочку обработчиков и вернуть
            // сообщение об ошибке.
            IAuthResult result = null;
            if (!_sessionHelper.SessionExists(sessionId))
            {
                result = new ErrorResult("Сессия не существует.");
            }
            // Иначе извлечь из сессии данные пользователя и сохранить их в контексте. Идентификационное имя
            // пользователя и имя внешнего OAuth2-провайдера сохраняются отдельно.
            else
            {
                ExternalUserInfo userInfo = _sessionHelper.FetchUserInfo(sessionId);
                context.Set(UserInfoValidator.ContextKey, userInfo);
                context.Set(UserNameValidator.ContextKey, userInfo.Email);
                context.Set(ProviderValidator.ContextKey, userInfo.Provider);
            }

            return Task.FromResult(result);
        }
    }
}
