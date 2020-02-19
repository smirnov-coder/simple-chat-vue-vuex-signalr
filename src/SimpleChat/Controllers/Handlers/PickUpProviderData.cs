using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using SimpleChat.Services;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    /// <summary>
    /// Представляет собой процесс подбора данных, необходимых для последующих обработчиков, на основе имени внешнего
    /// OAuth2-провайдера.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class PickUpProviderData : HandlerBase
    {
        private IOAuth2Service _facebook;
        private IOAuth2Service _vkontakte;
        private IOAuth2Service _odnoklassniki;
        private ProviderValidator _providerValidator;

        #region Constructors
        public PickUpProviderData(
            IFacebookOAuth2Service facebook,
            IVKontakteOAuth2Service vkontakte,
            IOdnoklassnikiOAuth2Service odnoklassniki,
            ProviderValidator providerValidator,
            IGuard guard)
            : base(guard)
        {
            _facebook = _guard.EnsureObjectParamIsNotNull(facebook, nameof(facebook));
            _vkontakte = _guard.EnsureObjectParamIsNotNull(vkontakte, nameof(vkontakte));
            _odnoklassniki = _guard.EnsureObjectParamIsNotNull(odnoklassniki, nameof(odnoklassniki));
            _providerValidator = _guard.EnsureObjectParamIsNotNull(providerValidator, nameof(providerValidator));
        }

        public PickUpProviderData(
            IFacebookOAuth2Service facebook,
            IVKontakteOAuth2Service vkontakte,
            IOdnoklassnikiOAuth2Service odnoklassniki,
            ProviderValidator providerValidator)
            : this(facebook, vkontakte, odnoklassniki, providerValidator, null)
        {
        }
        #endregion

        protected override bool CanHandle(IContext context)
        {
            return _providerValidator.Validate(context, _errors);
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста имя внешнего провайдера.
            string provider = context.Get(ProviderValidator.ContextKey) as string;

            // Подобрать следующие данные:
            // - тип клайма полного имени пользователя;
            // - тип клайма аватара;
            // - тип клайма маркера доступа;
            // - OAuth2-сервис.
            string
                nameClaimType = string.Empty,
                avatarClaimType = string.Empty,
                accessTokenClaimType = string.Empty;
            IAuthResult result = null;
            IOAuth2Service service = null;

            switch (provider)
            {
                case ExternalProvider.Facebook:
                    nameClaimType = CustomClaimTypes.FacebookName;
                    avatarClaimType = CustomClaimTypes.FacebookAvatar;
                    accessTokenClaimType = CustomClaimTypes.FacebookAccessToken;
                    service = _facebook;
                    break;

                case ExternalProvider.VKontakte:
                    nameClaimType = CustomClaimTypes.VKontakteName;
                    avatarClaimType = CustomClaimTypes.VKontakteAvatar;
                    accessTokenClaimType = CustomClaimTypes.VKontakteAccessToken;
                    service = _vkontakte;
                    break;

                case ExternalProvider.Odnoklassniki:
                    nameClaimType = CustomClaimTypes.OdnoklassnikiName;
                    avatarClaimType = CustomClaimTypes.OdnoklassnikiAvatar;
                    accessTokenClaimType = CustomClaimTypes.OdnoklassnikiAccessToken;
                    service = _odnoklassniki;
                    break;

                default:
                    result = new ErrorResult($"Неизвестный провайдер. Значение: {provider}.");
                    break;
            }

            // Сохранить в контексте по соответствующим ключам все выбранные данные.
            context.Set(NameClaimTypeValidator.ContextKey, nameClaimType);
            context.Set(AvatarClaimTypeValidator.ContextKey, avatarClaimType);
            context.Set(AccessTokenClaimTypeValidator.ContextKey, accessTokenClaimType);
            context.Set(OAuth2ServiceValidator.ContextKey, service);

            return Task.FromResult(result);
        }
    }
}
