using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using SimpleChat.Services;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    /// <summary>
    /// Представляет собой процесс получения от внешнего OAuth2-провайдера данных о пользователе.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class RequestUserInfo : HandlerBase
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
            // Извлечь из контекста OAuth2-сервис.
            var oauth2Service = context.Get(OAuth2ServiceValidator.ContextKey) as IOAuth2Service;

            IAuthResult result = null;
            try
            {
                // Запросить у внешнего провайдера информацию о пользователе.
                await oauth2Service.RequestUserInfoAsync();

                // Сохранить в контексте информацию о пользователе. Идентификационное имя дублируется в контексте.
                context.Set(UserInfoValidator.ContextKey, oauth2Service.UserInfo);
                context.Set(UserNameValidator.ContextKey, oauth2Service.UserInfo.Email);
            }
            catch (OAuth2ServiceException ex)
            {
                // В случае неудачи обмена, будет выброшено исключение OAuth2ServiceException. На основе данных этого
                // исключения сформировать сообщение об ошибке и вернуть его, прервав цепочку обработчиков.
                CreateUserFriendlyErrorMessages(ex.Data, out IEnumerable<string> errors);
                result = new ExternalLoginErrorResult(ex.Message, errors);
            }

            return result;
        }

        private void CreateUserFriendlyErrorMessages(IDictionary exceptionData, out IEnumerable<string> errors)
        {
            // Обработать наиболее ожидаемые ошибки для каждого конкретного внешнего провайдера
            // и сформировать "user friendly" сообщениe об ошибке, которое не раскрывает никаких подробностей
            // работы программы и тем более взаимодействие с внешним провайдером, но поможет понять причины
            // неработоспособности и исправить их.

            // 1. Недействительный код авторизации (authorization code).
            // 2. Недействительный access_token.
            // 3. Нехватка permissions.
            // 4. Пользователь сменил пароль.

            // Пока оставлю общий сценарий для всех.
            errors = new string[] { (string)exceptionData["message"] };
        }
    }
}
