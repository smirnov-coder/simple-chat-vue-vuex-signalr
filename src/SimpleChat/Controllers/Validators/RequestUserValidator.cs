using SimpleChat.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SimpleChat.Controllers.Validators
{
    public class RequestUserValidator : ObjectValidator<ClaimsPrincipal>
    {
        public const string ContextKey = "requestUser";

        public RequestUserValidator() : this(null)
        {
        }

        public RequestUserValidator(IGuard guard) : base(ContextKey, guard)
        {
        }
    }
}
