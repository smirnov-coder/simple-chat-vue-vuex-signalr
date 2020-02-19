using Microsoft.AspNetCore.Identity;
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
    /// Представляет собой процесс подготовки к успешному входу на сайт через внешний OAuth2-провайдер.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class PrepareForSignInReturn : HandlerBase
    {
        private SignInManager<IdentityUser> _signInManager;

        public PrepareForSignInReturn(SignInManager<IdentityUser> signInManager) : this(signInManager, null)
        {
        }

        public PrepareForSignInReturn(SignInManager<IdentityUser> signInManager, IGuard guard) : base(guard)
        {
            _signInManager = _guard.EnsureObjectParamIsNotNull(signInManager, nameof(signInManager));
        }

        protected override bool CanHandle(IContext context)
        {
            return true;
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // На всякий случай выполним выход из приложения, чтобы убедиться, что никакие auth-cookie не будут
            // добавлены к ответу сервера.
            await _signInManager.SignOutAsync();

            // Передать управление следующему обработчику, вернув null.
            return default(IAuthResult);
        }
    }
}
