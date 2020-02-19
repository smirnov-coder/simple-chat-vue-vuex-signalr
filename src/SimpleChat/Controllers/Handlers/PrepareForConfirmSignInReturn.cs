using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    /// <summary>
    /// Представляет собой процесс подготовки к успешному завершению подтверждения входа на сайт через внешний
    /// OAuth2-провайдер.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class PrepareForConfirmSignInReturn : HandlerBase
    {
        private ISessionHelper _sessionHelper;

        public PrepareForConfirmSignInReturn(ISessionHelper sessionHelper) : this(sessionHelper, null)
        {
        }

        public PrepareForConfirmSignInReturn(ISessionHelper sessionHelper, IGuard guard) : base(guard)
        {
            _sessionHelper = _guard.EnsureObjectParamIsNotNull(sessionHelper, nameof(sessionHelper));
        }

        protected override bool CanHandle(IContext context)
        {
            return true;
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Очистить данные сессии пользователя за ненадобностью.
            _sessionHelper.ClearSession();

            // Передать управление следующему обработчику, вернув null.
            return Task.FromResult(default(IAuthResult));
        }
    }
}
