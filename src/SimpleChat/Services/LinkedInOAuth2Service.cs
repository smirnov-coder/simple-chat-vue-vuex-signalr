using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Models;

namespace SimpleChat.Services
{
    public class LinkedInOAuth2Service : IOAuth2Service
    {
        private IConfiguration _configuration;

        private const string ACCESS_TOKEN_ENDPOINT = "https://www.linkedin.com/oauth/v2/accessToken";
        private const string API_BASE_URL = "https://api.linkedin.com/v2";

        public string RedirectUri { get; set; }

        private HttpClient _httpClient = new HttpClient();
        public virtual HttpClient HttpClient
        {
            get => _httpClient;
            set => _httpClient = value ?? throw new ArgumentNullException(nameof(value), "Значение не может быть равно 'null'.");
        }

        public LinkedInOAuth2Service(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetAccessTokenAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Значение не может быть пустой строкой или равно 'null'.", nameof(code));

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _configuration["Authentication:LinkedIn:ClientId"],
                ["client_secret"] = _configuration["Authentication:LinkedIn:ClientSecret"],
                ["redirect_uri"] = RedirectUri,
                ["code"] = code,
                ["grant_type"] = "authorization_code"
            });
            var response = await HttpClient.PostAsync(ACCESS_TOKEN_ENDPOINT, content);
            if (!response.IsSuccessStatusCode)
                throw new OAuth2ServiceException("Не удалось подключиться к 'LinkedIn' для обмена кода авторизации на маркер доступа.");
            string json = await response.Content.ReadAsStringAsync();
            var accessTokenResponse = JObject.Parse(json);
            if (accessTokenResponse.ContainsKey("error"))
                ThrowException("Не удалось обменять код авторизации на маркер доступа.", accessTokenResponse);
            return (string)accessTokenResponse["access_token"];
        }

        private void ThrowException(string message, JObject dataSource)
        {
            var exception = new OAuth2ServiceException(message);
            /// TODO: Найти обработку ошибок.
            throw exception;
        }

        public async Task<ExternalUserInfo> GetUserInfoAsync(string accessToken)
        {
            string requestUri = QueryHelpers.AddQueryString($"{API_BASE_URL}/me", new Dictionary<string, string>
            {
                ["projection"] = "(id,localizedFirstName,localizedLastName,profilePicture(displayImage~:playableStreams))",
            });
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
            var response = await HttpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Не удалось подключиться к 'LinkedIn' для получения информации о пользователе.");
            string json = await response.Content.ReadAsStringAsync();
            var userInfoResponse = JObject.Parse(json);
            if (userInfoResponse.ContainsKey("error"))
                ThrowException("Не удалось получить информацию о пользователе.", userInfoResponse);
            return new ExternalUserInfo
            {
                Id = (string)userInfoResponse["id"],
                Name = $"{(string)userInfoResponse["localizedFirstName"]} {(string)userInfoResponse["localizedLastName"]}",
                Email = await GetEmailAsync(accessToken),
                AccessToken = accessToken,
                Picture = (string)userInfoResponse["profilePicture"]["displayImage~"]["elements"].First["identifiers"].First["identifier"],
                Provider = ExternalProvider.LinkedIn
            };
        }

        private async Task<string> GetEmailAsync(string accessToken)
        {
            string requestUri = QueryHelpers.AddQueryString($"{API_BASE_URL}/clientAwareMemberHandles", new Dictionary<string, string>
            {
                ["q"] = "members",
                ["projection"] = "(elements*(primary,type,handle~))",
            });
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);
            var response = await HttpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Не удалось подключиться к 'LinkedIn' для получения адреса электронной почты пользователя.");
            string json = await response.Content.ReadAsStringAsync();
            var emailResponse = JObject.Parse(json);
            string email = (string)emailResponse["elements"]
                .FirstOrDefault(node => (string)node["type"] == "EMAIL" && (bool)node["primary"])?["handle~"]["emailAddress"];
            return email;
        }
    }
}
