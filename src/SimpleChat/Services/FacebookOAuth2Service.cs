using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Models;

namespace SimpleChat.Services
{
    public class FacebookOAuth2Service : IOAuth2Service
    {
        private IConfiguration _configuration;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private const string BASE_URL = "https://graph.facebook.com";
        private readonly string ACCESS_TOKEN_ENDPOINT = $"{BASE_URL}/v3.3/oauth/access_token";

        public string RedirectUri { get; set; }

        private HttpClient _httpClient = new HttpClient();
        public virtual HttpClient HttpClient
        {
            get => _httpClient;
            set => _httpClient = value ?? throw new ArgumentNullException(nameof(value), "Значение не может быть равно 'null'.");
        }

        public FacebookOAuth2Service(IConfiguration configuration)
        {
            _configuration = configuration;
            _clientId = _configuration["Authentication:Facebook:AppId"];
            _clientSecret = _configuration["Authentication:Facebook:AppSecret"];
        }

        public async Task<string> GetAccessTokenAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Значение не может быть пустой строкой или равно 'null'.", nameof(code));
            
            string requestUri = QueryHelpers.AddQueryString(ACCESS_TOKEN_ENDPOINT, new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["redirect_uri"] = RedirectUri,
                ["code"] = code,
                ["auth_type"] = "rerequest" /// TODO: let's see what happened
            });
            var response = await HttpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
                throw new OAuth2ServiceException("Не удалось подключиться к 'Facebook' для обмена кода авторизации на маркер доступа.");
            string json = await response.Content.ReadAsStringAsync();
            var accessTokenResponse = JObject.Parse(json);
            if (accessTokenResponse.ContainsKey("error"))
                ThrowException("Не удалось обменять код авторизации на маркер доступа.", accessTokenResponse);
            return await ExchangeShortLivedAccessTokenAsync((string)accessTokenResponse["access_token"]);
        }

        private void ThrowException(string message, JObject dataSource)
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
            var exception = new OAuth2ServiceException(message);
            if (dataSource.ContainsKey(keys.Message))
                exception.Data.Add(keys.Message, (string)dataSource[keys.Message]);
            if (dataSource.ContainsKey(keys.Type))
                exception.Data.Add(keys.Type, (string)dataSource[keys.Type]);
            if (dataSource.ContainsKey(keys.Code))
                exception.Data.Add(keys.Code, (int)dataSource[keys.Code]);
            if (dataSource.ContainsKey(keys.ErrorSubcode))
                exception.Data.Add(keys.ErrorSubcode, (int)dataSource[keys.ErrorSubcode]);
            if (dataSource.ContainsKey(keys.ErrorUserTitle))
                exception.Data.Add(keys.ErrorUserTitle, (string)dataSource[keys.ErrorUserTitle]);
            if (dataSource.ContainsKey(keys.ErrorUserMessage))
                exception.Data.Add(keys.ErrorUserMessage, (string)dataSource[keys.ErrorUserMessage]);
            if (dataSource.ContainsKey(keys.TraceId))
                exception.Data.Add(keys.TraceId, (string)dataSource[keys.TraceId]);
            throw exception;
        }

        private async Task<string> ExchangeShortLivedAccessTokenAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Значение не может быть пустой строкой или равно 'null'.", nameof(accessToken));

            string requestUri = QueryHelpers.AddQueryString(ACCESS_TOKEN_ENDPOINT, new Dictionary<string, string>
            {
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["grant_type"] = "fb_exchange_token",
                ["fb_exchange_token"] = accessToken,
                ["access_token"] = accessToken
            });
            var response = await HttpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Не удалось подключиться к 'Facebook' для обмена краткосрочного маркера доступа на долгосрочный.");
            string json = await response.Content.ReadAsStringAsync();
            var exchangeTokenResponse = JObject.Parse(json);
            if (exchangeTokenResponse.ContainsKey("error"))
                ThrowException("Не удалось обменять краткосрочный маркер доступа на долгострочный.", exchangeTokenResponse);
            return (string)exchangeTokenResponse["access_token"];
        }

        public async Task<ExternalUserInfo> GetUserInfoAsync(string accessToken)
        {
            string requestUri = QueryHelpers.AddQueryString($"{BASE_URL}/me", new Dictionary<string, string>
            {
                ["fields"] = "id,name,email,picture",
                ["access_token"] = accessToken
            });
            var response = await HttpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Не удалось подключиться к 'Facebook' для получения информации о пользователе.");
            string json = await response.Content.ReadAsStringAsync();
            var userInfoResponse = JObject.Parse(json);
            if (userInfoResponse.ContainsKey("error"))
                ThrowException("Не удалось получить информацию о пользователе.", userInfoResponse);
            return new ExternalUserInfo
            {
                Id = (string)userInfoResponse["id"],
                Name = (string)userInfoResponse["name"],
                Email = (string)userInfoResponse["email"],
                AccessToken = accessToken,
                Picture =  (string)userInfoResponse["picture"]["data"]["url"],
                Provider = ExternalProvider.Facebook
            };
        }
    }

    //
    // https://developers.facebook.com/docs/graph-api/using-graph-api/error-handling
    // https://developers.facebook.com/docs/facebook-login/access-tokens/debugging-and-error-handling
    //
}
