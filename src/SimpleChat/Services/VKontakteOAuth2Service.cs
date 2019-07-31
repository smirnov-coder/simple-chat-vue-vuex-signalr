using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Models;

namespace SimpleChat.Services
{
    public class VKontakteOAuth2Service : IOAuth2Service
    {
        private IConfiguration _configuration;
        private const string ACCESS_TOKEN_ENDPOINT = "https://oauth.vk.com/access_token";
        private const string USERS_URL = "https://api.vk.com/method/users.get";
        private string _userEmail;

        public string RedirectUri { get; set; }

        private HttpClient _httpClient = new HttpClient();
        public virtual HttpClient HttpClient
        {
            get => _httpClient;
            set => _httpClient = value ?? throw new ArgumentNullException(nameof(value), "Значение не может быть равно 'null'.");
        }

        public VKontakteOAuth2Service(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetAccessTokenAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Значение не может быть пустой строкой или равно 'null'.", nameof(code));

            string requestUri = QueryHelpers.AddQueryString(ACCESS_TOKEN_ENDPOINT, new Dictionary<string, string>
            {
                ["client_id"] = _configuration["Authentication:VKontakte:ClientId"],
                ["client_secret"] = _configuration["Authentication:VKontakte:ClientSecret"],
                ["redirect_uri"] = RedirectUri,
                ["code"] = code,
            });
            var response = await HttpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
                throw new OAuth2ServiceException("Не удалось подключиться к 'ВКонтакте' для обмена кода авторизации на маркер доступа.");
            string json = await response.Content.ReadAsStringAsync();
            var accessTokenResponse = JObject.Parse(json);
            if (accessTokenResponse.ContainsKey("error"))
                ThrowException("Не удалось обменять код авторизации на маркер доступа.", accessTokenResponse);
            _userEmail = (string)accessTokenResponse["email"]; /// HACK: Это явно неправильно.
            return (string)accessTokenResponse["access_token"];
        }

        private void ThrowException(string message, JObject dataSource)
        {
            var keys = new
            {
                Error = "error",
                ErrorDescription = "error_description",
                ErrorCode = "error_code",
                ErrorMessage = "error_msg",
                RequestParams = "request_params"
            };
            var exception = new OAuth2ServiceException(message);
            if (dataSource.ContainsKey(keys.Error))
                exception.Data.Add(keys.Error, (string)dataSource[keys.Error]);
            if (dataSource.ContainsKey(keys.ErrorDescription))
                exception.Data.Add(keys.ErrorDescription, (string)dataSource[keys.ErrorDescription]);
            if (dataSource.ContainsKey(keys.ErrorCode))
                exception.Data.Add(keys.ErrorCode, (int)dataSource[keys.ErrorCode]);
            if (dataSource.ContainsKey(keys.ErrorMessage))
                exception.Data.Add(keys.ErrorMessage, (string)dataSource[keys.ErrorMessage]);
            if (dataSource.ContainsKey(keys.RequestParams))
                exception.Data.Add(keys.RequestParams, dataSource.Value<Dictionary<string, string>>(keys.RequestParams));
            throw exception;
        }

        public async Task<ExternalUserInfo> GetUserInfoAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new InvalidOperationException($"Значение '${nameof(accessToken)}' не может быть пустой строкой или равно 'null'.");

            string requestUri = QueryHelpers.AddQueryString(USERS_URL, new Dictionary<string, string>
            {
                ["access_token"] = accessToken,
                ["fields"] = "photo_50",
                ["name_case"] = "nom",
                ["v"] = "5.101"
            });

            var response = await HttpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
                throw new OAuth2ServiceException("Не удалось подключиться к 'ВКонтакте' для получения информации о пользователе.");
            string json = await response.Content.ReadAsStringAsync();
            var userInfoResponse = JObject.Parse(json);
            if (userInfoResponse.ContainsKey("error"))
                ThrowException("Не удалось получить информацию о пользователе.", userInfoResponse);
            return new ExternalUserInfo
            {
                Id = (string)userInfoResponse["response"].First["id"],
                Name = $"{(string)userInfoResponse["response"].First["first_name"]} {(string)userInfoResponse["response"].First["last_name"]}",
                Email = _userEmail,
                AccessToken = accessToken,
                Picture = (string)userInfoResponse["response"].First["photo_50"],
                Provider = ExternalProvider.VKontakte
            };
        }
    }
}
