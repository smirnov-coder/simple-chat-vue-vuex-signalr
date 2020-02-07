using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Services;

namespace SimpleChat.Controllers.Validators
{
    public class OAuth2ServiceValidator : ObjectValidator<IOAuth2Service>
    {
        public const string ContextKey = "oauth2Service";

        public OAuth2ServiceValidator() : this(null)
        {
        }

        public OAuth2ServiceValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
