using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Models;

namespace SimpleChat.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private IAuthenticationFlow _authenticationFlow;
        private ISignInFlow _signInFlow;
        private IConfirmSignInFlow _confirmSignInFlow;
        private IContextBuilder _contextBuilder;
        private const string PartialViewName = "_CallbackPartial";

        public AuthController(IAuthenticationFlow authenticationFlow, ISignInFlow signInFlow,
            IConfirmSignInFlow confirmSignInFlow, IContextBuilder contextBuilder)
        {
            _authenticationFlow = authenticationFlow;
            _signInFlow = signInFlow;
            _confirmSignInFlow = confirmSignInFlow;
            _contextBuilder = contextBuilder;
        }

        //
        // GET /auth/check
        //
        [HttpGet("check")]
        public async Task<IAuthResult> AuthenticateAsync()
        {
            IContext context = _contextBuilder
                .WithRequestUser(GetCurrentUser())
                .Build();
            return await _authenticationFlow.RunAsync(context);
        }

        protected virtual ClaimsPrincipal GetCurrentUser() => User;

        //
        // GET /auth/sign-in/facebook
        //
        [HttpGet("sign-in/facebook")]
        public async Task<PartialViewResult> SignInWithFacebookAsync(string code, string state, string error,
            string errorReason, string errorDescription)
        {
            string provider = ExternalProvider.Facebook;

            // Проверить наличие ошибки входа Facebook.
            if (!string.IsNullOrWhiteSpace(error))
                return GetExternalLoginErrorResult(provider, new string[] { error, errorReason, errorDescription });

            return await InternalSignInAsync(provider, state, code, CustomClaimTypes.FacebookName,
                CustomClaimTypes.FacebookAvatar);
        }

        private PartialViewResult GetExternalLoginErrorResult(string provider, params string[] errors)
        {
            return PartialView(PartialViewName, new ExternalLoginErrorResult($"Ошибка окна входа через '{provider}'.",
                errors));
        }

        private async Task<PartialViewResult> InternalSignInAsync(string provider, string state, string code,
            string nameClaimType, string avatarClaimType)
        {
            IContext context = _contextBuilder
                .WithProvider(provider)
                .WithState(state)
                .WithAuthorizationCode(code)
                .WithNameClaimType(nameClaimType)
                .WithAvatarClaimType(avatarClaimType)
                .Build();
            IAuthResult result = await _signInFlow.RunAsync(context);
            return PartialView(PartialViewName, result);
        }

        //
        // POST /auth/sign-in/confirm
        //
        [HttpPost("sign-in/confirm")]
        public async Task<IAuthResult> ConfirmSigInAsync([FromForm]string sessionId, [FromForm]string code)
        {
            IContext context = _contextBuilder
                .WithSessionId(sessionId)
                .WithConfirmationCode(code)
                .Build();
            return await _confirmSignInFlow.RunAsync(context);
        }

        //
        // GET /auth/sign-in/vkontakte
        //
        [HttpGet("sign-in/vkontakte")]
        public async Task<PartialViewResult> SignInWithVKontakteAsync(string code, string state, string error,
            string errorDescription)
        {
            string provider = ExternalProvider.VKontakte;

            // Проверить наличие ошибки входа ВКонтакте.
            if (!string.IsNullOrWhiteSpace(error))
                return GetExternalLoginErrorResult(provider, new string[] { error, errorDescription });

            return await InternalSignInAsync(provider, state, code, CustomClaimTypes.VKontakteName,
                CustomClaimTypes.VKontakteAvatar);
        }

        //
        // GET /auth/sign-in/odnoklassniki
        //
        [HttpGet("sign-in/odnoklassniki")]
        public async Task<PartialViewResult> SignInWithOdnoklassnikiAsync(string code, string state, string error)
        {
            string provider = ExternalProvider.Odnoklassniki;

            // Проверить наличие ошибки входа Одноклассники.
            if (!string.IsNullOrWhiteSpace(error))
                return GetExternalLoginErrorResult(provider, new string[] { error });

            return await InternalSignInAsync(provider, state, code, CustomClaimTypes.OdnoklassnikiName,
                CustomClaimTypes.OdnoklassnikiAvatar);
        }
    }
}
