using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    /// <summary>
    /// Представляет собой процесс проверки наличия данных аутентицикации в запросе к <see cref="AuthController"/>.
    /// </summary>
    /// <inheritdoc cref="HandlerBase"/>
    public class ValidateRequestUser : HandlerBase
    {
        private RequestUserValidator _validator;

        public ValidateRequestUser(RequestUserValidator requestUserValidator, IGuard guard) : base(guard)
        {
            _validator = _guard.EnsureObjectParamIsNotNull(requestUserValidator, nameof(requestUserValidator));
        }

        public ValidateRequestUser(RequestUserValidator requestUserValidator) : this(requestUserValidator, null)
        {
        }

        protected override bool CanHandle(IContext context)
        {
            return _validator.Validate(context, _errors);
        }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            // Извлечь из контекста данные аунетификации пользователя.
            var requestUser = context.Get(RequestUserValidator.ContextKey) as ClaimsPrincipal;

            // Если идентификационное имя пользователя отсутствует, то пользователь не авторизован, прервать цепочку
            // обработчиков, вернув соответствующий результат. 
            if (string.IsNullOrWhiteSpace(requestUser.Identity?.Name))
                return GetResult(new NotAuthenticatedResult());

            // Если в JWT отсутствует клайм с именем провайдера, то прервать цепочку обработчиков и вернуть сообщение
            // об ошибе.
            string providerClaimType = CustomClaimTypes.Provider;
            if (!requestUser.HasClaim(claim => claim.Type == providerClaimType))
                return GetResult(new ErrorResult($"Отсутствует клайм '{providerClaimType}' в JWT."));

            // Иначе сохранить в контексте имя внешнего провайдера и идентификационное имя пользователя.
            string provider = requestUser.FindFirst(claim => claim.Type == providerClaimType).Value;
            context.Set(ProviderValidator.ContextKey, provider);
            context.Set(UserNameValidator.ContextKey, requestUser.Identity.Name);

            // Передать управление следующему обработчику, вернув null.
            return GetResult(default(IAuthResult));
        }

        private Task<IAuthResult> GetResult(IAuthResult authResult)
        {
            return Task.FromResult(authResult);
        }
    }
}
