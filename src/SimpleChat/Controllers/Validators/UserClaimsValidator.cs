using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace SimpleChat.Controllers.Validators
{
    public class UserClaimsValidator : ObjectValidator<IList<Claim>>
    {
        public const string ContextKey = "userClaims";

        public UserClaimsValidator() : this(null)
        {
        }

        public UserClaimsValidator(IGuard guard) : base(ContextKey, guard)
        {
        }

        protected override void InternalValidate(IContext context, ICollection<string> errors)
        {
            var value = context.Get(ContextKey) as IList<Claim>;
            if (!value.Any())
                errors.Add("Коллекция клаймов пользователя пуста.");
        }
    }
}
