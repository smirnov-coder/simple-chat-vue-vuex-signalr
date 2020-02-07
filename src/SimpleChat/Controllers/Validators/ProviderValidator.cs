using SimpleChat.Infrastructure.Helpers;

namespace SimpleChat.Controllers.Validators
{
    public class ProviderValidator : StringValidator
    {
        public const string ContextKey = "provider";

        public ProviderValidator() : this(null)
        {
        }

        public ProviderValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
