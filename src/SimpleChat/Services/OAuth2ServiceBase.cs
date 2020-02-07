using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;

namespace SimpleChat.Services
{
    public abstract class OAuth2ServiceBase : IOAuth2Service
    {
        protected IConfiguration _configuration;
        protected IGuard _guard;
        protected IJsonHelper _jsonHelper;
        protected IUriHelper _uriHelper;
        protected string _providerName;

        private string _clientId;
        protected string ClientId
        {
            get => _clientId;
            set => _clientId = _guard.EnsureStringParamIsNotNullOrEmpty(value, nameof(ClientId));
        }

        private string _clientSecret;
        protected string ClientSecret
        {
            get => _clientSecret;
            set => _clientSecret = _guard.EnsureStringParamIsNotNullOrEmpty(value, nameof(ClientSecret));
        }

        protected string _redirectUri;
        public virtual string RedirectUri
        {
            get => _redirectUri;
            set => _redirectUri = _guard.EnsureStringParamIsNotNullOrEmpty(value, nameof(RedirectUri));
        }

        protected string _accessToken;
        public virtual string AccessToken
        {
            get => _accessToken;
            set => _accessToken = _guard.EnsureStringParamIsNotNullOrEmpty(value, nameof(AccessToken));
        }

        public virtual ExternalUserInfo UserInfo { get; protected set; }

        private HttpClient _httpClient = new HttpClient();
        public virtual HttpClient HttpClient
        {
            get => _httpClient;
            set => _httpClient = _guard.EnsureObjectParamIsNotNull(value, nameof(HttpClient));
        }

        protected OAuth2ServiceBase(string clientIdKey, string clientSecretKey, string providerName,
            IConfiguration configuration, IUriHelper uriHelper, IJsonHelper jsonHelper = null, IGuard guard = null)
        {
            _guard = guard ?? new Guard();
            _jsonHelper = jsonHelper ?? new JsonHelper();
            _guard.EnsureStringParamIsNotNullOrEmpty(clientIdKey, nameof(clientIdKey));
            _guard.EnsureStringParamIsNotNullOrEmpty(clientSecretKey, nameof(clientSecretKey));
            _providerName = _guard.EnsureStringParamIsNotNullOrEmpty(providerName, nameof(providerName));
            _configuration = _guard.EnsureObjectParamIsNotNull(configuration, nameof(configuration));
            _uriHelper = _guard.EnsureObjectParamIsNotNull(uriHelper, nameof(uriHelper));
            ClientId = _configuration[clientIdKey];
            ClientSecret = _configuration[clientSecretKey];
        }

        public virtual async Task RequestAccessTokenAsync(string code)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(code, nameof(code));
            _guard.EnsureStringPropertyIsNotNullOrEmpty(RedirectUri, $"Не задан адрес обратного вызова " +
                $"({nameof(RedirectUri)}) для OAuth-службы '{_providerName}'.");

            var request = CreateAccessTokenRequest(code);
            var accessTokenResponse = await GetParsedResponseAsync(request, "Не удалось подключиться к " +
                $"'{_providerName}' для обмена кода авторизации на маркер доступа.");
            if (IsErrorAccessTokenResponse(accessTokenResponse))
                ThrowException("Не удалось обменять код авторизации на маркер доступа.", accessTokenResponse);

            await HandleAccessTokenResponseAsync(accessTokenResponse);
        }

        protected abstract HttpRequestMessage CreateAccessTokenRequest(string code);

        protected abstract bool IsErrorAccessTokenResponse(JObject parsedResponse);

        protected void ThrowException(string message, JObject dataSource)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(message, nameof(message));
            _guard.EnsureObjectParamIsNotNull(dataSource, nameof(dataSource));

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

        private async Task<JObject> GetParsedResponseAsync(HttpRequestMessage request, string failMessage)
        {
            // Используется перегруженная версия метода SendAsync(HttpRequestMessage, CancellationToken)
            // для возможности замокать HttpClient при тестировании.
            using (var response = await HttpClient.SendAsync(request, default(CancellationToken)))
            {
                if (await IsConnectionErrorResponseAsync(response))
                    throw new OAuth2ServiceException(failMessage);

                string json = await response.Content.ReadAsStringAsync();
                return _jsonHelper.Parse(json);
            }
        }

        protected virtual async Task<bool> IsConnectionErrorResponseAsync(HttpResponseMessage response)
        {
            string responseText = string.Empty;
            if (response.Content != null)
            {
                responseText = await response.Content.ReadAsStringAsync();
            }
            return !response.IsSuccessStatusCode && string.IsNullOrWhiteSpace(responseText);
        }

        public virtual async Task RequestUserInfoAsync()
        {
            _guard.EnsureStringPropertyIsNotNullOrEmpty(AccessToken, $"Для выполнения операции необходим маркер " +
                $"доступа ({nameof(AccessToken)}).");

            var request = CreateUserInfoRequest();
            var userInfoResponse = await GetParsedResponseAsync(request, $"Не удалось подключиться к " +
                $"'{_providerName}' для получения информации о пользователе.");
            if (IsErrorUserInfoResponse(userInfoResponse))
                ThrowException("Не удалось получить информацию о пользователе.", userInfoResponse);

            await HandleUserInfoResponseAsync(userInfoResponse);
        }

        protected abstract HttpRequestMessage CreateUserInfoRequest();

        protected virtual bool IsErrorUserInfoResponse(JObject parsedResponse)
        { 
            return IsErrorAccessTokenResponse(parsedResponse);
        }

        protected abstract Task HandleUserInfoResponseAsync(JObject userInfoResponse);
    }
}
