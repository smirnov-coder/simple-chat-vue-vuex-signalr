using SimpleChat.Infrastructure.Helpers;

namespace SimpleChat.Controllers.Validators
{
    public class NameClaimTypeValidator : StringValidator
    {
        public const string ContextKey = "nameClaimType";

        public NameClaimTypeValidator() : this(null)
        {
        }

        public NameClaimTypeValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
