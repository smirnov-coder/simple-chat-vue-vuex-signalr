using System;
using System.Collections;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;

namespace SimpleChat.Services
{
    /// <summary>
    /// Абстрактный базовый класс, инкапсулирующий доступ к внешнему OAuth2-провайдера.
    /// </summary>
    public abstract class OAuth2ServiceBase : IOAuth2Service
    {
        /// <summary>
        /// Компонент доступа к файлу настроек приложения appsettings.json.
        /// </summary>
        protected IConfiguration _configuration;

        /// <summary>
        /// Компонент для работы согласно технике защитного программирования.
        /// </summary>
        protected IGuard _guard;

        /// <summary>
        /// Компонент для работы с JSON.
        /// </summary>
        protected IJsonHelper _jsonHelper;

        /// <summary>
        /// Компонент для работы с Uri.
        /// </summary>
        protected IUriHelper _uriHelper;

        /// <summary>
        /// Имя внешнего OAuth2-провайдера.
        /// </summary>
        protected string _provider;

        private string _clientId;
        /// <summary>
        /// Внутренний идентификатор приложения внешнего OAuth2-провайдера. Необходим для выполнения некоторых запросов 
        /// к API внешнего провайдера.
        /// </summary>
        protected string ClientId
        {
            get => _clientId;
            set => _clientId = _guard.EnsureStringParamIsNotNullOrEmpty(value, nameof(ClientId));
        }

        private string _clientSecret;
        /// <summary>
        /// Секрет приложения внешнего OAuth2-приложения. Необходим для выполнения некоторых запросов к API внешнего
        /// провайдера.
        /// </summary>
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
        /// <summary>
        /// Компонент, используемый для выполнения HTTP-запросов к API внешнего OAuth2-провайдера.
        /// </summary>
        public virtual HttpClient HttpClient
        {
            get => _httpClient;
            set => _httpClient = _guard.EnsureObjectParamIsNotNull(value, nameof(HttpClient));
        }

        /// <summary>
        /// Создаёт новый объект класса.
        /// </summary>
        /// <param name="clientIdKey">
        /// Значение ключа, по которому доступен внутренний идентификатор приложения внешнего OAuth2-провайдера в файле
        /// настроек приложения appsettings.json.
        /// </param>
        /// <param name="clientSecretKey">
        /// Значение ключа, по которому доступен секрет приложения внешнего OAuth2-провайдера в файле настроек
        /// приложения appsettings.json.
        /// </param>
        /// <param name="provider">Имя внешнего OAuth2-провайдера.</param>
        /// <param name="configuration">Компонент доступа к файлу настроек приложения appsettings.json.</param>
        /// <param name="uriHelper">Компонент для работы с Uri.</param>
        /// <param name="jsonHelper">Компонент для работы с JSON.</param>
        /// <param name="guard">Компонент для работы согласно технике защитного программирования.</param>
        protected OAuth2ServiceBase(string clientIdKey, string clientSecretKey, string provider,
            IConfiguration configuration, IUriHelper uriHelper, IJsonHelper jsonHelper = null, IGuard guard = null)
        {
            _guard = guard ?? new Guard();
            _jsonHelper = jsonHelper ?? new JsonHelper();
            _guard.EnsureStringParamIsNotNullOrEmpty(clientIdKey, nameof(clientIdKey));
            _guard.EnsureStringParamIsNotNullOrEmpty(clientSecretKey, nameof(clientSecretKey));
            _provider = _guard.EnsureStringParamIsNotNullOrEmpty(provider, nameof(provider));
            _configuration = _guard.EnsureObjectParamIsNotNull(configuration, nameof(configuration));
            _uriHelper = _guard.EnsureObjectParamIsNotNull(uriHelper, nameof(uriHelper));
            ClientId = _configuration[clientIdKey];
            ClientSecret = _configuration[clientSecretKey];
        }

        public virtual async Task RequestAccessTokenAsync(string code)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(code, nameof(code));
            _guard.EnsureStringPropertyIsNotNullOrEmpty(RedirectUri, $"Не задан адрес обратного вызова " +
                $"({nameof(RedirectUri)}) для OAuth-службы '{_provider}'.");

            var request = CreateAccessTokenRequest(code);
            var accessTokenResponse = await GetParsedResponseAsync(request, "Не удалось подключиться к " +
                $"'{_provider}' для обмена кода авторизации на маркер доступа.");
            if (IsErrorAccessTokenResponse(accessTokenResponse))
                ThrowException("Не удалось обменять код авторизации на маркер доступа.", accessTokenResponse);

            await HandleAccessTokenResponseAsync(accessTokenResponse);
        }

        /// <summary>
        /// Создаёт объект HTTP-запроса к API внешнего OAuth2-провайдера для обмена кода авторизации на маркер доступа.
        /// </summary>
        /// <param name="code">Код авторизации.</param>
        protected abstract HttpRequestMessage CreateAccessTokenRequest(string code);

        /// <summary>
        /// Проверяет, получен ли ответ с ошибкой внешнего OAuth2-провайдера в ходе обмена кода авторизации на маркер
        /// доступа.
        /// </summary>
        /// <param name="parsedResponse">
        /// Ответ внешнего OAuth2-провайдера, пригодный для выполнения запросов LINQ to JSON.
        /// </param>
        /// <returns>true, если получен ответ с ошибкой; иначе false</returns>
        protected abstract bool IsErrorAccessTokenResponse(JObject parsedResponse);

        /// <summary>
        /// Выбрасывает исключение <see cref="OAuth2ServiceException"/> с заданным сообщением об ошибке.
        /// </summary>
        /// <param name="message">Сообщение о возникшей ошибке.</param>
        /// <param name="dataSource">
        /// Объект, содержащий информацию о возникших внутренних ошибках и пригодный для выполнения запросов LINQ to
        /// JSON. Служит источником для наполнения данными свойства <see cref="Exception.Data"/>.
        /// </param>
        protected void ThrowException(string message, JObject dataSource)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(message, nameof(message));
            _guard.EnsureObjectParamIsNotNull(dataSource, nameof(dataSource));

            var exception = new OAuth2ServiceException(message);
            CollectErrorData(exception.Data, dataSource);
            throw exception;
        }

        /// <summary>
        /// Собирает в коллекцию <paramref name="data"/> информацию об ошибке внешнего OAuth2-провайдера.
        /// </summary>
        /// <param name="data">Коллекция, в которую собирается информация об ошибке внешнего OAuth2-провайдера.</param>
        /// <param name="dataSource">
        /// Источник информации об ошибках, пригодный для выполнения запросов LINQ to JSON.
        /// </param>
        protected abstract void CollectErrorData(IDictionary data, JObject dataSource);

        /// <summary>
        /// Асинхронно выполняет обработку успешно выполненного запроса обмена кода авторизации на маркер доступа.
        /// Базовая реализация извлекает из ответа сервера маркер доступа по ключу "access_token".
        /// </summary>
        /// <param name="accessTokenResponse">
        /// Ответ запроса обмена кода авторизации на маркер доступа в виде объекта, пригодного для выполнения запросов
        /// LINQ to JSON.
        /// </param>
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

        /// <summary>
        /// Асинхронно проверяет, произошла ли ошибка подключения при выполнении запроса к API внешнего
        /// OAuth2-провайдера. Базовая реализация проверяет "плохой" статус код и пустое тело ответа.
        /// </summary>
        /// <remarks>
        /// Каждый провайдер по разному сообщает об ошибках. Чтоб отличить ошибку подключения от ошибки внешнего
        /// провайдера, нельзя полагаться только на StatusCode, т.к. некоторые внешние API возвращают сообщения о своих
        /// внутренних ошибках в 400 BadRequest.
        /// </remarks>
        /// <param name="response">Объект HTTP-ответа.</param>
        /// <returns>true, если произошла ошибка подключения; иначе false</returns>
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
                $"'{_provider}' для получения информации о пользователе.");
            if (IsErrorUserInfoResponse(userInfoResponse))
                ThrowException("Не удалось получить информацию о пользователе.", userInfoResponse);

            await HandleUserInfoResponseAsync(userInfoResponse);
        }

        /// <summary>
        /// Создаёт объект HTTP-запроса к API внешнего OAuth2-провайдера для получения информации о пользователе.
        /// </summary>
        /// <param name="code">Код авторизации.</param>
        protected abstract HttpRequestMessage CreateUserInfoRequest();

        /// <summary>
        /// Проверяет, получен ли ответ с ошибкой внешнего OAuth2-провайдера в ходе получения информации о пользователе.
        /// </summary>
        /// <param name="parsedResponse">
        /// Ответ внешнего OAuth2-провайдера, пригодный для выполнения запросов LINQ to JSON.
        /// </param>
        /// <returns>true, если получен ответ с ошибкой; иначе false</returns>
        protected virtual bool IsErrorUserInfoResponse(JObject parsedResponse)
        { 
            return IsErrorAccessTokenResponse(parsedResponse);
        }

        /// <summary>
        /// Асинхронно выполняет обработку успешно выполненного запроса получения информации о пользователе.
        /// </summary>
        /// <param name="userInfoResponse">
        /// Ответ запроса получения информации о пользователе в виде объекта, пригодного для выполнения запросов LINQ
        /// to JSON.
        /// </param>
        protected abstract Task HandleUserInfoResponseAsync(JObject userInfoResponse);
    }
}
