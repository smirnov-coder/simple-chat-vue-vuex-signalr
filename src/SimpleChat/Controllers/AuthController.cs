using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using SimpleChat.Services;

namespace SimpleChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private const string VIEW_NAME = "_CallbackPartial";

        private SignInManager<IdentityUser> _signInManager;
        private UserManager<IdentityUser> _userManager;

        private FacebookOAuth2Service _facebook;
        private VKontakteOAuth2Service _vkontakte;
        private LinkedInOAuth2Service _linkedIn;
        private OdnoklassnikiOAuth2Service _odnoklassniki;
        private IEmailService _emailService;
        private IHostingEnvironment _environment;
        private ILogger<AuthController> _logger;

        public AuthController(
            SignInManager<IdentityUser> signInManager,
            FacebookOAuth2Service facebook,
            VKontakteOAuth2Service vkontakte,
            LinkedInOAuth2Service linkedIn,
            OdnoklassnikiOAuth2Service odnoklassniki,
            IEmailService emailService,
            IHostingEnvironment environment,
            ILogger<AuthController> logger)
        {
            _signInManager = signInManager;
            _userManager = _signInManager.UserManager;
            _facebook = facebook;
            _vkontakte = vkontakte;
            _linkedIn = linkedIn;
            _odnoklassniki = odnoklassniki;
            _emailService = emailService;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("check")]
        public async Task<IAuthResult> AuthenticateAsync()
        {
            if (User?.Identity?.Name != null)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                    return new ErrorResult("Пользователь не найден.");
                var userClaims = await _userManager.GetClaimsAsync(user);
                string provider = User.FindFirstValue(CustomClaimTypes.Provider);
                string
                    nameClaimType = string.Empty,
                    avatarClaimType = string.Empty,
                    accessTokenClaimType = string.Empty;
                IOAuth2Service oauth2Service = null;

                switch (provider)
                {
                    case ExternalProvider.Facebook:
                        nameClaimType = CustomClaimTypes.FacebookName;
                        avatarClaimType = CustomClaimTypes.FacebookAvatar;
                        accessTokenClaimType = CustomClaimTypes.FacebookAccessToken;
                        oauth2Service = _facebook;
                        break;

                    case ExternalProvider.VKontakte:
                        nameClaimType = CustomClaimTypes.VKontakteName;
                        avatarClaimType = CustomClaimTypes.VKontakteAvatar;
                        accessTokenClaimType = CustomClaimTypes.VKontakteAccessToken;
                        oauth2Service = _vkontakte;
                        break;

                    case ExternalProvider.LinkedIn:
                        nameClaimType = CustomClaimTypes.LinkedInName;
                        avatarClaimType = CustomClaimTypes.LinkedInAvatar;
                        accessTokenClaimType = CustomClaimTypes.LinkedInAccessToken;
                        oauth2Service = _linkedIn;
                        break;

                    case ExternalProvider.Odnoklassniki:
                        nameClaimType = CustomClaimTypes.OdnoklassnikiName;
                        avatarClaimType = CustomClaimTypes.OdnoklassnikiAvatar;
                        accessTokenClaimType = CustomClaimTypes.OdnoklassnikiAccessToken;
                        oauth2Service = _odnoklassniki;
                        break;

                    default:
                        return new NotAuthenticatedResult();
                }

                try
                {
                    string accessToken = userClaims.FirstOrDefault(claim => claim.Type == accessTokenClaimType).Value;
                    var userInfo = await oauth2Service.GetUserInfoAsync(accessToken);
                    // Обновить данные.
                    await RefreshUserClaims(user, userInfo, userClaims, nameClaimType, avatarClaimType);

                    return new AuthenticatedResult
                    {
                        User = new UserInfo
                        {
                            Id = user.Id,
                            Name = userClaims.FirstOrDefault(claim => claim.Type == nameClaimType).Value,
                            Avatar = userClaims.FirstOrDefault(claim => claim.Type == avatarClaimType).Value,
                            Provider = provider
                        }
                    };
                }
                catch
                {
                    //return new NotAuthenticatedResult();
                }
            }
            return new NotAuthenticatedResult();
        }

        private async Task<bool> RefreshUserClaims(IdentityUser user, ExternalUserInfo userInfo, IEnumerable<Claim> userClaims,
            string nameClaimType, string avatarClaimType)
        {
            var nameReplaceResult = await _userManager.ReplaceClaimAsync(user, userClaims.First(claim => claim.Type == nameClaimType),
                new Claim(nameClaimType, userInfo.Name));
            var avatarReplaceResult = await _userManager.ReplaceClaimAsync(user, userClaims.First(claim => claim.Type == avatarClaimType),
                new Claim(avatarClaimType, userInfo.Picture));
            return nameReplaceResult.Succeeded && avatarReplaceResult.Succeeded;
        }

        private SignInSuccessResult GetSuccessResult(string userId, ExternalUserInfo userInfo)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId), // Будет использовано в ChatHub.
                new Claim(ClaimTypes.Name, userInfo.Email),
                new Claim(CustomClaimTypes.FullName, userInfo.Name),
                new Claim(CustomClaimTypes.Avatar, userInfo.Picture),
                new Claim(CustomClaimTypes.Provider, userInfo.Provider)
            };
            return new SignInSuccessResult(JwtHelper.GetEncodedJwt(userClaims));
        }

        [HttpGet("sign-in/facebook")]
        public async Task<PartialViewResult> SignInWithFacebookAsync(string code, string state, string error, string errorReason,
            string errorDescription)
        {
            // Проверить наличие ошибки входа Facebook.
            if (!string.IsNullOrWhiteSpace(error))
            {
                return PartialView(VIEW_NAME, new ExternalLoginErrorResult($"Ошибка окна входа через {ExternalProvider.Facebook}.",
                    new string[] { error, errorReason, errorDescription }));
            }

            // Валидировать полученный state.
            if (state != ExternalProvider.Facebook)
                return PartialView(VIEW_NAME, new ErrorResult("Неправильный 'state'."));

            _facebook.RedirectUri = Url.Action(nameof(SignInWithFacebookAsync), "Auth", null, Request.Scheme, Request.Host.Value).ToLower();
            return await DoSignInAsync(_facebook, code, CustomClaimTypes.FacebookName, CustomClaimTypes.FacebookAvatar);
        }

        private async Task<PartialViewResult> DoSignInAsync(IOAuth2Service oauth2Service, string code, string nameClaimType,
            string avatarClaimType)
        {
            // Получить маркер доступа и информацию профайла пользователя.
            string accessToken = await oauth2Service.GetAccessTokenAsync(code);
            var userInfo = await oauth2Service.GetUserInfoAsync(accessToken);

            // Не получили от внешнего провайдера e-mail.
            if (string.IsNullOrWhiteSpace(userInfo.Email))
                return PartialView(VIEW_NAME, new EmailRequiredResult(userInfo.Provider));

            // Зарегистрированный пользователь осуществляет вход через внешнего провайдера.
            var signInResult = await _signInManager.ExternalLoginSignInAsync(userInfo.Provider, userInfo.Id, false, true);
            if (signInResult.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(userInfo.Provider, userInfo.Id);
                if (user == null)
                    return PartialView(VIEW_NAME, new ErrorResult("Пользователь не найден."));

                // Обновить данные профайла пользователя (полное имя пользователя и аватар). Если неудачно - оставить старые данные.
                var userClaims = await _userManager.GetClaimsAsync(user);
                await RefreshUserClaims(user, userInfo, userClaims, nameClaimType, avatarClaimType);

                await _signInManager.SignOutAsync();
                return PartialView(VIEW_NAME, GetSuccessResult(user.Id, userInfo));
            }

            // Зарегистрированный пользователь добавляет вход через внешнего провайдера или пользователь не зарегистрирован.
            string appUrl = Url.Action("Index", "Home", null, Request.Scheme, Request.Host.ToString());
            string text = _emailService.CreateSignInConfirmationEmail(userInfo.Name, userInfo.Provider, appUrl,
                GenerateConfirmationCode(userInfo.Id));
            await _emailService.SendEmailAsync(userInfo.Name, userInfo.Email, $"Добавление входа через {userInfo.Provider}", text);
            string sessionId = Guid.NewGuid().ToString();
            string json = JsonConvert.SerializeObject(userInfo);
            HttpContext.Session.SetString(sessionId, json);
            await HttpContext.Session.CommitAsync();
            return PartialView(VIEW_NAME, new ConfirmSignInResult(sessionId, userInfo.Email, userInfo.Provider));
        }

        [HttpPost("sign-in/confirm")]
        public async Task<IAuthResult> ConfirmSigInAsync([FromForm]string sessionId, [FromForm]string code)
        {
            //await HttpContext.Session.LoadAsync();
            if (!HttpContext.Session.Keys.Contains(sessionId))
                return new ErrorResult("Сессия не существует.");

            string json = HttpContext.Session.GetString(sessionId);
            var userInfo = JsonConvert.DeserializeObject<ExternalUserInfo>(json);
            if (!IsValidCode(code, GenerateConfirmationCode(userInfo.Id)))
                return new ErrorResult("Неверный код подтверждения.");
            HttpContext.Session.Clear();

            // Зарегистрированный пользователь добавляет вход через внешнего провайдера.
            var user = await _userManager.FindByEmailAsync(userInfo.Email);
            if (user != null)
                return await AddExternalLoginToUserAsync(user, userInfo);

            // Пользователь не зарегистрирован.
            user = new IdentityUser
            {
                UserName = userInfo.Email,
                Email = userInfo.Email,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
                return await AddExternalLoginToUserAsync(user, userInfo);

            return new ErrorResult($"Не удалось зарегистрировать пользователя '{userInfo.Name}'.",
                result.Errors.Select(error => error.Description));
        }

        private bool IsValidCode(string actual, string expected)
        {
            return _environment.IsDevelopment() ? actual == "test" : actual == expected;
        }

        private string GenerateConfirmationCode(string userId) => userId.GetHashCode().ToString("X8");

        private async Task<IAuthResult> AddExternalLoginToUserAsync(IdentityUser user, ExternalUserInfo userInfo)
        {
            string
                nameClaimType = string.Empty,
                avatarClaimType = string.Empty,
                accessTokenClaimType = string.Empty;
            switch (userInfo.Provider)
            {
                case ExternalProvider.Facebook:
                    nameClaimType = CustomClaimTypes.FacebookName;
                    avatarClaimType = CustomClaimTypes.FacebookAvatar;
                    accessTokenClaimType = CustomClaimTypes.FacebookAccessToken;
                    break;

                case ExternalProvider.VKontakte:
                    nameClaimType = CustomClaimTypes.VKontakteName;
                    avatarClaimType = CustomClaimTypes.VKontakteAvatar;
                    accessTokenClaimType = CustomClaimTypes.VKontakteAccessToken;
                    break;

                case ExternalProvider.LinkedIn:
                    nameClaimType = CustomClaimTypes.LinkedInName;
                    avatarClaimType = CustomClaimTypes.LinkedInAvatar;
                    accessTokenClaimType = CustomClaimTypes.LinkedInAccessToken;
                    break;

                case ExternalProvider.Odnoklassniki:
                    nameClaimType = CustomClaimTypes.OdnoklassnikiName;
                    avatarClaimType = CustomClaimTypes.OdnoklassnikiAvatar;
                    accessTokenClaimType = CustomClaimTypes.OdnoklassnikiAccessToken;
                    break;
            }
            var result = await _userManager.AddClaimsAsync(user, new List<Claim>
            {
                new Claim(nameClaimType, userInfo.Name),
                new Claim(avatarClaimType, userInfo.Picture),
                new Claim(accessTokenClaimType, userInfo.AccessToken)
            });
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, new UserLoginInfo(userInfo.Provider, userInfo.Id, null));
                if (result.Succeeded)
                    return GetSuccessResult(user.Id, userInfo);
                return new ErrorResult($"Не удалось добавить вход через '{userInfo.Provider}' для пользователя " +
                    $"'{userInfo.Name}'.", result.Errors.Select(error => error.Description));
            }
            return new ErrorResult($"Не удалось сохранить данные '{userInfo.Provider}' профайла пользователя '{userInfo.Name}'.",
                result.Errors.Select(error => error.Description));
        }

        [HttpGet("sign-in/vkontakte")]
        public async Task<PartialViewResult> SignInWithVKontakteAsync(string code, string state, string error, string errorDescription)
        {
            // Проверить наличие ошибки входа ВКонтакте.
            if (!string.IsNullOrWhiteSpace(error))
            {
                return PartialView(VIEW_NAME, new ExternalLoginErrorResult($"Ошибка окна входа через {ExternalProvider.VKontakte}.",
                    new string[] { error, errorDescription }));
            }

            // Валидировать полученный state.
            if (state != ExternalProvider.VKontakte)
                return PartialView(VIEW_NAME, new ErrorResult("Неправильный 'state'."));

            _vkontakte.RedirectUri = Url.Action(nameof(SignInWithVKontakteAsync), "Auth", null, Request.Scheme, Request.Host.Value).ToLower();
            return await DoSignInAsync(_vkontakte, code, CustomClaimTypes.VKontakteName, CustomClaimTypes.VKontakteAvatar);
        }

        [HttpGet("sign-in/linkedin")]
        public async Task<PartialViewResult> SignInWithLinkedInAsync(string code, string state, string error, string errorDescription)
        {
            // Проверить наличие ошибки входа LinkedIn.
            if (!string.IsNullOrWhiteSpace(error))
            {
                return PartialView(VIEW_NAME, new ExternalLoginErrorResult($"Ошибка окна входа через {ExternalProvider.LinkedIn}.",
                    new string[] { error, errorDescription }));
            }

            // Валидировать полученный state.
            if (state != ExternalProvider.LinkedIn)
                return PartialView(VIEW_NAME, new ErrorResult("Неправильный 'state'."));

            _linkedIn.RedirectUri = Url.Action(nameof(SignInWithLinkedInAsync), "Auth", null, Request.Scheme, Request.Host.Value).ToLower();
            return await DoSignInAsync(_linkedIn, code, CustomClaimTypes.LinkedInName, CustomClaimTypes.LinkedInAvatar);
        }

        [HttpGet("sign-in/odnoklassniki")]
        public async Task<PartialViewResult> SignInWithOdnoklassnikiAsync(string code, string state, string error)
        {
            // Проверить наличие ошибки входа Одноклассники.
            if (!string.IsNullOrWhiteSpace(error))
            {
                return PartialView(VIEW_NAME, new ExternalLoginErrorResult($"Ошибка окна входа через {ExternalProvider.Odnoklassniki}.",
                    new string[] { error }));
            }

            // Валидировать полученный state.
            if (state != ExternalProvider.Odnoklassniki)
                return PartialView(VIEW_NAME, new ErrorResult("Неправильный 'state'."));

            _odnoklassniki.RedirectUri = Url.Action(nameof(SignInWithOdnoklassnikiAsync), "Auth", null, Request.Scheme, Request.Host.Value).ToLower();
            return await DoSignInAsync(_odnoklassniki, code, CustomClaimTypes.OdnoklassnikiName, CustomClaimTypes.OdnoklassnikiAvatar);
        }
    }
}
