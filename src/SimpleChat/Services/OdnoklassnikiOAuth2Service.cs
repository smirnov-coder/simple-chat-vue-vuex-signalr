using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Models;

namespace SimpleChat.Services
{
    public class OdnoklassnikiOAuth2Service : IOAuth2Service
    {
        private IConfiguration _configuration;
        private readonly string _applicationId;
        private readonly string _applicationKey;
        private readonly string _applicationSecretKey;

        public string RedirectUri { get; set; }

        private HttpClient _httpClient = new HttpClient();
        public virtual HttpClient HttpClient
        {
            get => _httpClient;
            set => _httpClient = value ?? throw new ArgumentNullException(nameof(value), "Значение не может быть равно 'null'.");
        }

        public OdnoklassnikiOAuth2Service(IConfiguration configuration)
        {
            _configuration = configuration;
            _applicationId = _configuration["Authentication:Odnoklassniki:ApplicationId"];
            _applicationKey = _configuration["Authentication:Odnoklassniki:ApplicationKey"];
            _applicationSecretKey = _configuration["Authentication:Odnoklassniki:ApplicationSecretKey"];
        }

        public async Task<string> GetAccessTokenAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Значение не может быть пустой строкой или равно 'null'.", nameof(code));

            const string ACCESS_TOKEN_ENDPOINT = "https://api.ok.ru/oauth/token.do";

            string requestUri = QueryHelpers.AddQueryString(ACCESS_TOKEN_ENDPOINT, new Dictionary<string, string>
            {
                ["client_id"] = _applicationId,
                ["client_secret"] = _applicationSecretKey,
                ["redirect_uri"] = RedirectUri,
                ["code"] = code,
                ["grant_type"] = "authorization_code"
            });
            var response = await HttpClient.PostAsync(requestUri, new StringContent(""));
            if (!response.IsSuccessStatusCode)
                throw new OAuth2ServiceException("Не удалось подключиться к 'Одноклассники' для обмена кода авторизации на маркер доступа.");
            string json = await response.Content.ReadAsStringAsync();
            var accessTokenResponse = JObject.Parse(json);
            if (accessTokenResponse.ContainsKey("error"))
                ThrowException("Не удалось обменять код авторизации на маркер доступа.", accessTokenResponse);
            return (string)accessTokenResponse["access_token"];
        }

        private void ThrowException(string message, JObject dataSource)
        {
            var keys = new
            {
                Error = "error",
                ErrorDescription = "error_description",
                ErrorCode = "error_code",
                ErrorMessage = "error_msg"
            };
            /// TODO: Дополнить ошибки getCurrentUser.
            var exception = new OAuth2ServiceException(message);
            if (dataSource.ContainsKey(keys.Error))
                exception.Data.Add(keys.Error, (string)dataSource[keys.Error]);
            if (dataSource.ContainsKey(keys.ErrorDescription))
                exception.Data.Add(keys.ErrorDescription, (string)dataSource[keys.ErrorDescription]);
            if (dataSource.ContainsKey(keys.ErrorCode))
                exception.Data.Add(keys.ErrorCode, (int)dataSource[keys.ErrorCode]);
            if (dataSource.ContainsKey(keys.ErrorMessage))
                exception.Data.Add(keys.ErrorMessage, (string)dataSource[keys.ErrorMessage]);
            throw exception;
        }

        public async Task<ExternalUserInfo> GetUserInfoAsync(string accessToken)
        {
            const string REQUEST_URL = "https://api.ok.ru/api/users/getCurrentUser";
            string sessionSecretKey = GetMd5Hash(accessToken + _applicationSecretKey);
            string signatureSource =
                $"application_key={_applicationKey}" +
                $"fields=uid,name,email,pic50x50" +
                $"format=json" +
                $"{sessionSecretKey}";
            string signature = GetMd5Hash(signatureSource);
            var queryParams = new Dictionary<string, string>
            {
                ["application_key"] = _applicationKey,
                ["fields"] = "uid,name,email,pic50x50",
                ["format"] = "json",
                ["access_token"] = accessToken,
                ["sig"] = signature
            };
            string requestUri = QueryHelpers.AddQueryString(REQUEST_URL, queryParams);
            var response = await HttpClient.GetAsync(requestUri);
            if (!response.IsSuccessStatusCode)
                throw new OAuth2ServiceException("Не удалось подключиться к 'Одноклассники' для получения информации о пользователе.");
            string json = await response.Content.ReadAsStringAsync();
            var userInfoResponse = JObject.Parse(json);
            if (userInfoResponse.ContainsKey("error_code"))
                ThrowException("Не удалось получить информацию о пользователе.", userInfoResponse);
            return new ExternalUserInfo
            {
                Id = (string)userInfoResponse["uid"],
                Name = (string)userInfoResponse["name"],
                Email = (string)userInfoResponse["email"],
                AccessToken = accessToken,
                Picture = (string)userInfoResponse["pic50x50"],
                Provider = ExternalProvider.Odnoklassniki
            };
        }

        private string GetMd5Hash(string source)
        {
            using (var md5Hasher = MD5.Create())
            {
                byte[] bytes = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(source));
                StringBuilder builder = new StringBuilder();
                // Loop through each byte of the hashed data and format each one as a hexadecimal string.
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                string result = builder.ToString();
                return result;
            }
        }
    }
}
