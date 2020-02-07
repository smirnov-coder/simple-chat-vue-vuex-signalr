using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    public class PrepareForConfirmSignInReturn : Handler
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
            _sessionHelper.ClearSession();
            return Task.FromResult(default(IAuthResult));
        }
    }
}
