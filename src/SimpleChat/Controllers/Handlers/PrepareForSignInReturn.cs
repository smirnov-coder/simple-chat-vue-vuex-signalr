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
    public class PrepareForSignInReturn : Handler
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
            await _signInManager.SignOutAsync();
            return default(IAuthResult);
        }
    }
}
