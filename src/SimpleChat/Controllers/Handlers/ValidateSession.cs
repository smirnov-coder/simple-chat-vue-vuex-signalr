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
    public class ValidateSession : Handler
    {
        private SessionIdValidator _sessionIdValidator;
        private ISessionHelper _sessionHelper;

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

        protected override bool CanHandle(IContext context)
        {
            return _sessionIdValidator.Validate(context, _errors);
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            string sessionId = context.Get(SessionIdValidator.ContextKey) as string;

            IAuthResult result = null;
            if (!_sessionHelper.SessionExists(sessionId))
            {
                result = new ErrorResult("Сессия не существует.");
            }
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
