using SimpleChat.Infrastructure.Helpers;

namespace SimpleChat.Controllers.Validators
{
    public class AccessTokenClaimTypeValidator : StringValidator
    {
        public const string ContextKey = "accessTokenClaimType";

        public AccessTokenClaimTypeValidator() : this(null)
        {
        }

        public AccessTokenClaimTypeValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
