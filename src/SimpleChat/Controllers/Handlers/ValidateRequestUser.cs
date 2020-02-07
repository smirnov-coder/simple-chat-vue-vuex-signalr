using Microsoft.AspNetCore.Http;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Extensions;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    public class ValidateRequestUser : Handler
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
            var requestUser = context.Get(RequestUserValidator.ContextKey) as ClaimsPrincipal;

            if (string.IsNullOrWhiteSpace(requestUser.Identity?.Name))
                return GetResult(new NotAuthenticatedResult());

            string providerClaimType = CustomClaimTypes.Provider;
            if (!requestUser.HasClaim(claim => claim.Type == providerClaimType))
                return GetResult(new ErrorResult($"Отсутствует клайм '{providerClaimType}' в JWT."));

            string provider = requestUser.FindFirst(claim => claim.Type == providerClaimType).Value;
            context.Set(ProviderValidator.ContextKey, provider);
            context.Set(UserNameValidator.ContextKey, requestUser.Identity.Name);
            return Task.FromResult(default(IAuthResult));
        }

        private Task<IAuthResult> GetResult(IAuthResult authResult)
        {
            return Task.FromResult(authResult);
        }
    }
}
