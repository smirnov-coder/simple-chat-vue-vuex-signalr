using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using SimpleChat.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    public class RequestAccessToken : Handler
    {
        private AuthorizationCodeValidator _authorizationCodeValidator;
        private OAuth2ServiceValidator _oauth2ServiceValidator;

        public RequestAccessToken(
            AuthorizationCodeValidator authorizationCodeValidator,
            OAuth2ServiceValidator oauth2ServiceValidator)
            : this(authorizationCodeValidator, oauth2ServiceValidator, null)
        {
        }

        public RequestAccessToken(
            AuthorizationCodeValidator authorizationCodeValidator,
            OAuth2ServiceValidator oauth2ServiceValidator,
            IGuard guard)
            : base(guard)
        {
            _authorizationCodeValidator = _guard.EnsureObjectParamIsNotNull(authorizationCodeValidator,
                nameof(authorizationCodeValidator));
            _oauth2ServiceValidator = _guard.EnsureObjectParamIsNotNull(oauth2ServiceValidator,
                nameof(oauth2ServiceValidator));
        }

        protected override bool CanHandle(IContext context)
        {
            bool isAuthorizationCodeValid = _authorizationCodeValidator.Validate(context, _errors);
            bool isOAuth2ServiceValid = _oauth2ServiceValidator.Validate(context, _errors);
            return isAuthorizationCodeValid && isOAuth2ServiceValid;
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            string authorizationCode = context.Get(AuthorizationCodeValidator.ContextKey) as string;
            var service = context.Get(OAuth2ServiceValidator.ContextKey) as IOAuth2Service;

            IAuthResult result = null;
            try
            {
                await service.RequestAccessTokenAsync(authorizationCode);
            }
            catch (OAuth2ServiceException ex)
            {
                CreateUserFriendlyErrorMessages(ex.Data, out IEnumerable<string> errors);
                result = new ExternalLoginErrorResult(ex.Message, errors);
            }
            return result;
        }

        private void CreateUserFriendlyErrorMessages(IDictionary exceptionData, out IEnumerable<string> errors)
        {
            /// TODO: Обработать наиболее ожидаемые ошибки для каждого конкретного внешнего провайдера
            /// и сформировать "user friendly" сообщениe об ошибке, которое не раскрывает никаких подробностей
            /// работы программы и тем более взаимодействие с внешним провайдером, но поможет понять причины
            /// неработоспособности и исправить их.

            // 1. Недействительный код авторизации (authorization code).
            // 2. Недействительный access_token.
            // 3. Нехватка permissions.
            // 4. Пользователь сменил пароль.

            // Пока оставлю общий сценарий для всех.
            errors = new string[] { (string)exceptionData["message"] };
        }
    }
}
