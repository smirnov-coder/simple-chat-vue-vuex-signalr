using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Extensions;
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
    public class RequestUserInfo : Handler
    {
        private OAuth2ServiceValidator _validator;

        public RequestUserInfo(OAuth2ServiceValidator oauth2ServiceValidator, IGuard guard) : base(guard)
        {
            _validator = _guard.EnsureObjectParamIsNotNull(oauth2ServiceValidator, nameof(oauth2ServiceValidator));
        }

        public RequestUserInfo(OAuth2ServiceValidator oauth2ServiceValidator) : this(oauth2ServiceValidator, null)
        {
        }

        protected override bool CanHandle(IContext context)
        {
            return _validator.Validate(context, _errors);
        }

        protected override async Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            IAuthResult result = null;
            var oauth2Service = context.Get(OAuth2ServiceValidator.ContextKey) as IOAuth2Service;
            try
            {
                await oauth2Service.RequestUserInfoAsync();
                context.Set(UserInfoValidator.ContextKey, oauth2Service.UserInfo);
                context.Set(UserNameValidator.ContextKey, oauth2Service.UserInfo.Email);
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
