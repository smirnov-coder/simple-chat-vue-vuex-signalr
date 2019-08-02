using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SimpleChat.Models;

namespace SimpleChat.Services
{
    public abstract class OAuth2ServiceBase : IOAuth2Service
    {
        protected IConfiguration _configuration;
        protected string _providerName;

        private string _clientId;
        protected string ClientId
        {
            get => _clientId;
            set => _clientId = EnsureStringParamIsNotNullOrEmpty(value, nameof(ClientId));
        }

        private string _clientSecret;
        protected string ClientSecret
        {
            get => _clientSecret;
            set => _clientSecret = EnsureStringParamIsNotNullOrEmpty(value, nameof(ClientSecret));
        }

        protected string _redirectUri;
        public virtual string RedirectUri
        {
            get => _redirectUri;
            set => _redirectUri = EnsureStringParamIsNotNullOrEmpty(value, nameof(RedirectUri));
        }

        protected string _accessToken;
        public virtual string AccessToken
        {
            get => _accessToken;
            set => _accessToken = EnsureStringParamIsNotNullOrEmpty(value, nameof(AccessToken));
        }

        public virtual ExternalUserInfo UserInfo { get; protected set; }

        private HttpClient _httpClient = new HttpClient();
        public virtual HttpClient HttpClient
        {
            get => _httpClient;
            set => _httpClient = EnsureObjectParamIsNotNull(value, nameof(HttpClient));
        }

        protected OAuth2ServiceBase(IConfiguration configuration, string clientIdKey, string clientSecretKey, string providerName)
        {
            EnsureStringParamIsNotNullOrEmpty(clientIdKey, nameof(clientIdKey));
            EnsureStringParamIsNotNullOrEmpty(clientSecretKey, nameof(clientSecretKey));
            EnsureStringParamIsNotNullOrEmpty(providerName, nameof(providerName));
            _configuration = EnsureObjectParamIsNotNull(configuration, nameof(configuration));
            ClientId = _configuration[clientIdKey];
            ClientSecret = _configuration[clientSecretKey];
            _providerName = providerName;
        }

        protected string EnsureStringParamIsNotNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Значение не может быть пустой строкой или равно 'null'.", paramName);
            return value;
        }

        protected T EnsureObjectParamIsNotNull<T>(T value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName, "Значение не может быть равно 'null'.");
            return value;
        }

        public virtual async Task RequestAccessTokenAsync(string code)
        {
            EnsureStringParamIsNotNullOrEmpty(code, nameof(code));
            EnsureStringPropertyIsNotNullOrEmpty(RedirectUri, $"Не задан адрес обратного вызова ({nameof(RedirectUri)}) для OAuth-службы '{_providerName}'.");

            var request = CreateAccessTokenRequest(code);
            var accessTokenResponse = await GetParsedResponseAsync(request, $"Не удалось подключиться к '{_providerName}' для обмена кода авторизации на маркер доступа.");
            if (IsErrorAccessTokenResponse(accessTokenResponse))
                ThrowException("Не удалось обменять код авторизации на маркер доступа.", accessTokenResponse);

            await HandleAccessTokenResponseAsync(accessTokenResponse);
        }

        protected abstract HttpRequestMessage CreateAccessTokenRequest(string code);

        protected abstract bool IsErrorAccessTokenResponse(JObject parsedResponse);

        protected void ThrowException(string message, JObject dataSource)
        {
            EnsureStringParamIsNotNullOrEmpty(message, nameof(message));
            EnsureObjectParamIsNotNull(dataSource, nameof(dataSource));

            var exception = new OAuth2ServiceException(message);
            CollectErrorData(exception.Data, dataSource);
            throw exception;
        }

        protected abstract void CollectErrorData(IDictionary data, JObject dataSource);

        protected virtual Task HandleAccessTokenResponseAsync(JObject accessTokenResponse)
        {
            AccessToken = (string)accessTokenResponse["access_token"];
            return Task.CompletedTask;
        }

        protected void EnsureStringPropertyIsNotNullOrEmpty(string value, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(errorMessage);
        }

        private async Task<JObject> GetParsedResponseAsync(HttpRequestMessage request, string failMessage)
        {
            // Используется перегруженная версия метода SendAsync(HttpRequestMessage, CancellationToken)
            // для возможности замокать HttpClient при тестировании.
            var response = await HttpClient.SendAsync(request, default(CancellationToken));
            if (!response.IsSuccessStatusCode)
                throw new OAuth2ServiceException(failMessage);

            string json = await response.Content.ReadAsStringAsync();
            return JObject.Parse(json);
        }

        public virtual async Task RequestUserInfoAsync()
        {
            EnsureStringPropertyIsNotNullOrEmpty(AccessToken, $"Для выполнения операции необходим маркер доступа ({nameof(AccessToken)}).");

            var request = CreateUserInfoRequest();
            var userInfoResponse = await GetParsedResponseAsync(request, $"Не удалось подключиться к '{_providerName}' для получения информации о пользователе.");
            if (IsErrorUserInfoResponse(userInfoResponse))
                ThrowException("Не удалось получить информацию о пользователе.", userInfoResponse);

            await HandleUserInfoResponseAsync(userInfoResponse);
        }

        protected abstract HttpRequestMessage CreateUserInfoRequest();

        protected virtual bool IsErrorUserInfoResponse(JObject parsedResponse) => IsErrorAccessTokenResponse(parsedResponse);

        protected abstract Task HandleUserInfoResponseAsync(JObject userInfoResponse);
    }
}
