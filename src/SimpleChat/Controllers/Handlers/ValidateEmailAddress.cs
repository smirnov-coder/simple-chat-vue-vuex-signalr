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
    public class ValidateEmailAddress : Handler
    {
        private UserInfoValidator _validator;

        public ValidateEmailAddress(UserInfoValidator userInfoValidator) : this(userInfoValidator, null)
        {
        }

        public ValidateEmailAddress(UserInfoValidator userInfoValidator, IGuard guard) : base(guard)
        {
            _validator = _guard.EnsureObjectParamIsNotNull(userInfoValidator, nameof(userInfoValidator));
        }

        protected override bool CanHandle(IContext context)
        {
            return _validator.Validate(context, _errors);
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;

            IAuthResult result = null;
            if (string.IsNullOrWhiteSpace(userInfo.Email))
                result = new EmailRequiredResult(userInfo.Provider);
            return Task.FromResult(result);
        }
    }
}
