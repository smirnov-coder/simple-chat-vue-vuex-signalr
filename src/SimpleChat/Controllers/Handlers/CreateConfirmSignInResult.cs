using Microsoft.AspNetCore.Identity;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using SimpleChat.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    public class CreateConfirmSignInResult : Handler
    {
        private SignInResultValidator _signInResultValidator;
        private UserInfoValidator _userInfoValidator;
        private IEmailService _emailService;
        private ISessionHelper _sessionHelper;
        private IUriHelper _uriHelper;

        public CreateConfirmSignInResult(
            SignInResultValidator signInResultValidator,
            UserInfoValidator userInfoValidator,
            IEmailService emailService,
            ISessionHelper sessionHelper,
            IUriHelper uriHelper)
            : this(signInResultValidator, userInfoValidator, emailService, sessionHelper, uriHelper, null)
        {
        }

        public CreateConfirmSignInResult(
            SignInResultValidator signInResultValidator,
            UserInfoValidator userInfoValidator,
            IEmailService emailService,
            ISessionHelper sessionHelper,
            IUriHelper uriHelper,
            IGuard guard)
            : base(guard)
        {
            _signInResultValidator = _guard.EnsureObjectParamIsNotNull(signInResultValidator,
                nameof(signInResultValidator));
            _userInfoValidator = _guard.EnsureObjectParamIsNotNull(userInfoValidator, nameof(userInfoValidator));
            _emailService = _guard.EnsureObjectParamIsNotNull(emailService, nameof(emailService));
            _sessionHelper = _guard.EnsureObjectParamIsNotNull(sessionHelper, nameof(sessionHelper));
            _uriHelper = _guard.EnsureObjectParamIsNotNull(uriHelper, nameof(uriHelper));
        }

        protected override bool CanHandle(IContext context)
        {
            bool isSignInResultValid = _signInResultValidator.Validate(context, _errors);
            bool isUserInfoValid = _userInfoValidator.Validate(context, _errors);
            return isSignInResultValid && isUserInfoValid;
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            var signInResult = context.Get(SignInResultValidator.ContextKey) as SignInResult;
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;

            IAuthResult result = null;
            if (!signInResult.Succeeded)
            {
                await SendSignInConfirmationEmail(userInfo);
                string sessionId = await _sessionHelper.SaveUserInfoAsync(userInfo);
                result = new ConfirmSignInResult(sessionId, userInfo.Email, userInfo.Provider);
            }
            return result;
        }

        private async Task SendSignInConfirmationEmail(ExternalUserInfo userInfo)
        {
            string appUrl = _uriHelper.GetControllerActionUri("Home", "Index");
            string text = _emailService.CreateSignInConfirmationEmail(userInfo.Name, userInfo.Provider, appUrl,
                GenerateConfirmationCode(userInfo.Id));
            await _emailService.SendEmailAsync(userInfo.Name, userInfo.Email, $"Добавление входа через " +
                $"'{userInfo.Provider}'.", text);
        }

        private string GenerateConfirmationCode(string userId) => userId.GetHashCode().ToString("x8");

    }
}
