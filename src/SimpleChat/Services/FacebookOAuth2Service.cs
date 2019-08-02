using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Models;

namespace SimpleChat.Services
{
    public class FacebookOAuth2Service : OAuth2ServiceBase
    {
        private const string BASE_URL = "https://graph.facebook.com";
        private readonly string ACCESS_TOKEN_ENDPOINT = $"{BASE_URL}/v3.3/oauth/access_token";

        public FacebookOAuth2Service(IConfiguration configuration)
            : base(configuration, "Authentication:Facebook:AppId", "Authentication:Facebook:AppSecret", ExternalProvider.Facebook)
        { }

        protected override HttpRequestMessage CreateAccessTokenRequest(string code)
        {
            string requestUri = QueryHelpers.AddQueryString(ACCESS_TOKEN_ENDPOINT, new Dictionary<string, string>
            {
                ["client_id"] = ClientId,
                ["client_secret"] = ClientSecret,
                ["redirect_uri"] = RedirectUri,
                ["code"] = code,
                ["auth_type"] = "rerequest"
            });
            return new HttpRequestMessage(HttpMethod.Get, requestUri);
        }

        protected override bool IsErrorAccessTokenResponse(JObject parsedResponse) => parsedResponse.ContainsKey("error");

        protected override void CollectErrorData(IDictionary data, JObject dataSource)
        {
            var keys = new
            {
                Message = "message",
                Type = "type",
                Code = "code",
                ErrorSubcode = "error_subcode",
                ErrorUserTitle = "error_user_title",
                ErrorUserMessage = "error_user_msg",
                TraceId = "fbtrace_id"
            };
            var errorObject = (JObject)dataSource["error"];
            if (errorObject.ContainsKey(keys.Message))
                data.Add(keys.Message, (string)errorObject[keys.Message]);
            if (errorObject.ContainsKey(keys.Type))
                data.Add(keys.Type, (string)errorObject[keys.Type]);
            if (errorObject.ContainsKey(keys.Code))
                data.Add(keys.Code, (int)errorObject[keys.Code]);
            if (errorObject.ContainsKey(keys.ErrorSubcode))
                data.Add(keys.ErrorSubcode, (int)errorObject[keys.ErrorSubcode]);
            if (errorObject.ContainsKey(keys.ErrorUserTitle))
                data.Add(keys.ErrorUserTitle, (string)errorObject[keys.ErrorUserTitle]);
            if (errorObject.ContainsKey(keys.ErrorUserMessage))
                data.Add(keys.ErrorUserMessage, (string)errorObject[keys.ErrorUserMessage]);
            if (errorObject.ContainsKey(keys.TraceId))
                data.Add(keys.TraceId, (string)errorObject[keys.TraceId]);
        }

        protected override async Task HandleAccessTokenResponseAsync(JObject accessTokenResponse)
        {
            await ExchangeShortLivedAccessTokenAsync((string)accessTokenResponse["access_token"]);
        }

        private async Task ExchangeShortLivedAccessTokenAsync(string accessToken)
        {
            EnsureStringParamIsNotNullOrEmpty(accessToken, nameof(accessToken));

            string requestUri = QueryHelpers.AddQueryString(ACCESS_TOKEN_ENDPOINT, new Dictionary<string, string>
            {
                ["client_id"] = ClientId,
                ["client_secret"] = ClientSecret,
                ["grant_type"] = "fb_exchange_token",
                ["fb_exchange_token"] = accessToken,
                ["access_token"] = accessToken
            });
            var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), default(CancellationToken));
            if (!response.IsSuccessStatusCode)
                throw new OAuth2ServiceException($"Не удалось подключиться к '{_providerName}' для обмена краткосрочного маркера доступа на долгосрочный.");

            string json = await response.Content.ReadAsStringAsync();
            var exchangeTokenResponse = JObject.Parse(json);
            if (IsErrorAccessTokenResponse(exchangeTokenResponse))
                ThrowException("Не удалось обменять краткосрочный маркер доступа на долгострочный.", exchangeTokenResponse);

            AccessToken = (string)exchangeTokenResponse["access_token"];
        }

        protected override HttpRequestMessage CreateUserInfoRequest()
        {
            string requestUri = QueryHelpers.AddQueryString($"{BASE_URL}/me", new Dictionary<string, string>
            {
                ["fields"] = "id,name,email,picture",
                ["access_token"] = AccessToken
            });
            return new HttpRequestMessage(HttpMethod.Get, requestUri);
        }

        protected override Task HandleUserInfoResponseAsync(JObject userInfoResponse)
        {
            UserInfo = new ExternalUserInfo
            {
                Id = (string)userInfoResponse["id"],
                Name = (string)userInfoResponse["name"],
                Email = (string)userInfoResponse["email"],
                AccessToken = AccessToken,
                Picture = (string)userInfoResponse["picture"]["data"]["url"],
                Provider = _providerName
            };
            return Task.CompletedTask;
        }
    }

    //
    // https://developers.facebook.com/docs/graph-api/using-graph-api/error-handling
    // https://developers.facebook.com/docs/facebook-login/access-tokens/debugging-and-error-handling
    //
}
