using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Models;

namespace SimpleChat.Services
{
    public class LinkedInOAuth2Service : OAuth2ServiceBase//IOAuth2Service
    {
        private const string API_BASE_URL = "https://api.linkedin.com/v2";
        private const string ACCESS_TOKEN_ENDPOINT = "https://www.linkedin.com/oauth/v2/accessToken";

        public LinkedInOAuth2Service(IConfiguration configuration)
            : base(configuration, "Authentication:LinkedIn:ClientId", "Authentication:LinkedIn:ClientSecret", ExternalProvider.LinkedIn)
        { }

        protected override HttpRequestMessage CreateAccessTokenRequest(string code)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = ClientId,
                ["client_secret"] = ClientSecret,
                ["redirect_uri"] = RedirectUri,
                ["code"] = code,
                ["grant_type"] = "authorization_code"
            });
            return new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ACCESS_TOKEN_ENDPOINT),
                Content = content
            };
        }

        protected override bool IsErrorAccessTokenResponse(JObject parsedResponse) => parsedResponse.ContainsKey("serviceErrorCode");

        protected override void CollectErrorData(IDictionary data, JObject dataSource)
        {
            var keys = new
            {
                Message = "message",
                ServiceErrorCode = "serviceErrorCode",
                Status = "status"
            };
            if (dataSource.ContainsKey(keys.Message))
                data.Add(keys.Message, (string)dataSource[keys.Message]);
            if (dataSource.ContainsKey(keys.ServiceErrorCode))
                data.Add(keys.ServiceErrorCode, (int)dataSource[keys.ServiceErrorCode]);
            if (dataSource.ContainsKey(keys.Status))
                data.Add(keys.Status, (int)dataSource[keys.Status]);
        }

        protected override HttpRequestMessage CreateUserInfoRequest()
        {
            string requestUri = QueryHelpers.AddQueryString($"{API_BASE_URL}/me", new Dictionary<string, string>
            {
                ["projection"] = "(id,localizedFirstName,localizedLastName,profilePicture(displayImage~:playableStreams))",
            });
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, AccessToken);
            return request;
        }

        protected override bool IsErrorUserInfoResponse(JObject parsedResponse) => IsErrorAccessTokenResponse(parsedResponse);

        protected override async Task HandleUserInfoResponseAsync(JObject userInfoResponse)
        {
            UserInfo = new ExternalUserInfo
            {
                Id = (string)userInfoResponse["id"],
                Name = $"{(string)userInfoResponse["localizedFirstName"]} {(string)userInfoResponse["localizedLastName"]}",
                Email = await GetEmailAsync(AccessToken),
                AccessToken = AccessToken,
                Picture = (string)userInfoResponse["profilePicture"]["displayImage~"]["elements"].First["identifiers"].First["identifier"],
                Provider = _providerName
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
            var response = await HttpClient.SendAsync(request, default(CancellationToken));
            if (!response.IsSuccessStatusCode)
                throw new OAuth2ServiceException($"Не удалось подключиться к '{ExternalProvider.LinkedIn}' для получения адреса электронной почты пользователя.");

            string json = await response.Content.ReadAsStringAsync();
            var emailResponse = JObject.Parse(json);
            if (IsErrorUserInfoResponse(emailResponse))
                ThrowException("Не удалось получить адрес электронной почты пользователя.", emailResponse);

            string email = (string)emailResponse["elements"]
                .FirstOrDefault(node => (string)node["type"] == "EMAIL" && (bool)node["primary"])?["handle~"]["emailAddress"];
            return email;
        }
    }
}
