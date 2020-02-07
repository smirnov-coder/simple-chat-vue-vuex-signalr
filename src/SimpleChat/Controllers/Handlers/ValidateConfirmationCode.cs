using Microsoft.AspNetCore.Hosting;
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
    public class ValidateConfirmationCode : Handler
    {
        private ConfirmationCodeValidator _confirmationCodeValidator;
        private UserInfoValidator _userInfoValidator;
        private IHostingEnvironment _environment;

        public ValidateConfirmationCode(
            ConfirmationCodeValidator confirmationCodeValidator,
            UserInfoValidator userInfoValidator,
            IHostingEnvironment environment)
            : this(confirmationCodeValidator, userInfoValidator, environment, null)
        {
        }

        public ValidateConfirmationCode(
            ConfirmationCodeValidator confirmationCodeValidator,
            UserInfoValidator userInfoValidator,
            IHostingEnvironment environment,
            IGuard guard)
        {
            _confirmationCodeValidator = _guard.EnsureObjectParamIsNotNull(confirmationCodeValidator,
                nameof(confirmationCodeValidator));
            _userInfoValidator = _guard.EnsureObjectParamIsNotNull(userInfoValidator, nameof(userInfoValidator));
            _environment = _guard.EnsureObjectParamIsNotNull(environment, nameof(environment));
        }

        protected override bool CanHandle(IContext context)
        {
            bool isConfirmationCodeValid = _confirmationCodeValidator.Validate(context, _errors);
            bool isUserInfoValid = _userInfoValidator.Validate(context, _errors);
            return isConfirmationCodeValid && isUserInfoValid;
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            string confirmationCode = context.Get(ConfirmationCodeValidator.ContexKey) as string;
            ExternalUserInfo userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;

            IAuthResult result = null;
            if (!IsValidCode(confirmationCode, GenerateConfirmationCode(userInfo.Id)))
                result = new ErrorResult("Неверный код подтверждения.");
            return Task.FromResult(result);
        }

        protected virtual bool IsValidCode(string actual, string expected)
        {
            return _environment.IsDevelopment() ? actual == "test" : actual == expected;
        }

        protected virtual string GenerateConfirmationCode(string userId) => userId.GetHashCode().ToString("x8");
    }
}
