using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Constants;
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
    /// Представляет собой процесс предварительной настройки OAuth2-сервиса для выполнения запросов к API внешнего
    /// OAuth2-провайдера в ходе выполнения входа на сайт.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class ConfigureOAuth2ServiceForSignIn : HandlerBase
    {
        private ProviderValidator _providerValidator;
        private IOAuth2Service _facebook;
        private IOAuth2Service _vkontakte;
        private IOAuth2Service _odnoklassniki;
        private IUriHelper _uriHelper;

        public ConfigureOAuth2ServiceForSignIn(
            ProviderValidator providerValidator,
            IFacebookOAuth2Service facebook,
            IVKontakteOAuth2Service vkontakte,
            IOdnoklassnikiOAuth2Service odnoklassniki,
            IUriHelper uriHelper)
            : this(providerValidator, facebook, vkontakte, odnoklassniki, uriHelper, null)
        {
        }
        
        public ConfigureOAuth2ServiceForSignIn(
            ProviderValidator providerValidator,
            IFacebookOAuth2Service facebook,
            IVKontakteOAuth2Service vkontakte,
            IOdnoklassnikiOAuth2Service odnoklassniki,
            IUriHelper uriHelper,
            IGuard guard) : base(guard)
        {
            _providerValidator = _guard.EnsureObjectParamIsNotNull(providerValidator, nameof(providerValidator));
            _facebook = _guard.EnsureObjectParamIsNotNull(facebook, nameof(facebook));
            _vkontakte = _guard.EnsureObjectParamIsNotNull(vkontakte, nameof(vkontakte));
            _odnoklassniki = _guard.EnsureObjectParamIsNotNull(odnoklassniki, nameof(odnoklassniki));
            _uriHelper = _guard.EnsureObjectParamIsNotNull(uriHelper, nameof(uriHelper));
        }

        protected override bool CanHandle(IContext context)
        {
            return _providerValidator.Validate(context, _errors);
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста имя провайдера, через который пользователь осуществляет вход на сайт.
            string provider = context.Get(ProviderValidator.ContextKey) as string;

            // В зависимости от текущего провайдера, определить имя метода AuthController, задаваемого в качестве
            // обратного вызова для внешнего OAuth2-провайдера, а также OAuth2-сервис, для выполнения запросов к
            // внешнему провайдеру.
            string actionName = string.Empty;
            IOAuth2Service service = null;

            switch (provider)
            {
                case ExternalProvider.Facebook:
                    actionName = nameof(AuthController.SignInWithFacebookAsync);
                    service = _facebook;
                    break;

                case ExternalProvider.VKontakte:
                    actionName = nameof(AuthController.SignInWithVKontakteAsync);
                    service = _vkontakte;
                    break;

                case ExternalProvider.Odnoklassniki:
                    actionName = nameof(AuthController.SignInWithOdnoklassnikiAsync);
                    service = _odnoklassniki;
                    break;

                default:
                    return Task.FromResult<IAuthResult>(new ErrorResult($"Неизвестный внешний провайдер. " +
                        $"Значение: {provider}."));
            }

            // Адрес, по которому внешний провайдер будет осуществлять callback-вызов, должен быть полным, включая
            // протокол и домен.
            service.RedirectUri = _uriHelper.GetControllerActionUri("Auth", actionName);

            // Сохранить настроенный OAuth2-сервис в контексте.
            context.Set(OAuth2ServiceValidator.ContextKey, service);

            // Передать управление следующему обработчику в цепочке, вернув null.
            return Task.FromResult(default(IAuthResult));
        }
    }
}
