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
    /// <summary>
    /// Представляет собой процесс создания результата, информирующего пользователя о необходимости подтверждения входа
    /// на сайт через внешнего OAuth2-провайдера по e-mail.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class CreateConfirmSignInResult : HandlerBase
    {
        private SignInResultValidator _signInResultValidator;
        private UserInfoValidator _userInfoValidator;
        private IEmailService _emailService;
        private ISessionHelper _sessionHelper;
        private IUriHelper _uriHelper;

        #region Constuctors
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
        #endregion

        protected override bool CanHandle(IContext context)
        {
            bool canUseSignInResult = _signInResultValidator.Validate(context, _errors);
            bool canUseUserInfo = _userInfoValidator.Validate(context, _errors);
            return canUseSignInResult && canUseUserInfo;
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста результат попытки входа на сайт через внешний OAuth2-провайдер и информацию о
            // пользователе внешнего провайдера.
            var signInResult = context.Get(SignInResultValidator.ContextKey) as SignInResult;
            var userInfo = context.Get(UserInfoValidator.ContextKey) as ExternalUserInfo;

            // Если попытка входа на сайт через внешний OAuth2-провайдер неудачна, значит пользователь ещё ни разу не
            // входил на наш сайт через этого провайдера, и надо подтвердить e-mail, полученный от внешнего провайдера,
            // отправив письмо с кодом подтверждения на e-mail пользователя. Прервать цепочку обработчиков.
            IAuthResult result = null;
            if (!signInResult.Succeeded)
            {
                await SendSignInConfirmationEmail(userInfo);
                string sessionId = await _sessionHelper.SaveUserInfoAsync(userInfo);
                result = new ConfirmSignInResult(sessionId, userInfo.Email, userInfo.Provider);
            }

            // Иначе передать управление следующему обработчику в цепочке, вернув null.
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

        // Сгенерировать под подтверждения. Код должен быть воспроизводимым на серверной стороне.
        /// TODO: надо бы заменить на какой-нить генератор в виде зависимости
        private string GenerateConfirmationCode(string userId) => userId.GetHashCode().ToString("x8");
    }
}
