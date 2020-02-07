using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;

namespace SimpleChat.Controllers.Validators
{
    public class UserInfoValidator : ObjectValidator<ExternalUserInfo>
    {
        public const string ContextKey = "userInfo";

        public UserInfoValidator() : this(null)
        {
        }

        public UserInfoValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
