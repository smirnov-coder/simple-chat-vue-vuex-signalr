using SimpleChat.Infrastructure.Helpers;

namespace SimpleChat.Controllers.Validators
{
    public class AvatarClaimTypeValidator : StringValidator
    {
        public const string ContextKey = "avatarClaimType";

        public AvatarClaimTypeValidator() : this(null)
        {
        }

        public AvatarClaimTypeValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
