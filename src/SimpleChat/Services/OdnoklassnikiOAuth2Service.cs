using System;
using System.Collections;
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
using SimpleChat.Infrastructure.Extensions;

namespace SimpleChat.Services
{
    public class OdnoklassnikiOAuth2Service : OAuth2ServiceBase
    {
        private readonly string _publicKey;

        public OdnoklassnikiOAuth2Service(IConfiguration configuration)
            : base(configuration, "Authentication:Odnoklassniki:ApplicationId", "Authentication:Odnoklassniki:ApplicationSecretKey", ExternalProvider.Odnoklassniki)
        {
            _publicKey = _configuration["Authentication:Odnoklassniki:ApplicationKey"];
        }

        protected override HttpRequestMessage CreateAccessTokenRequest(string code)
        {
            const string ACCESS_TOKEN_ENDPOINT = "https://api.ok.ru/oauth/token.do";

            string requestUri = QueryHelpers.AddQueryString(ACCESS_TOKEN_ENDPOINT, new Dictionary<string, string>
            {
                ["client_id"] = ClientId,
                ["client_secret"] = ClientSecret,
                ["redirect_uri"] = RedirectUri,
                ["code"] = code,
                ["grant_type"] = "authorization_code"
            });
            return new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(requestUri),
                Content = new StringContent("")
            };
        }

        protected override bool IsErrorAccessTokenResponse(JObject parsedResponse) => parsedResponse.ContainsKey("error");

        protected override void CollectErrorData(IDictionary data, JObject dataSource)
        {
            var keys = new
            {
                Error = "error",
                ErrorDescription = "error_description",
                ErrorCode = "error_code",
                ErrorMessage = "error_msg",
                ErrorData = "error_data",
                ErrorField = "error_field"
            };
            if (dataSource.ContainsKey(keys.Error))
                data.Add(keys.Error, (string)dataSource[keys.Error]);
            if (dataSource.ContainsKey(keys.ErrorDescription))
                data.Add(keys.ErrorDescription, (string)dataSource[keys.ErrorDescription]);
            if (dataSource.ContainsKey(keys.ErrorCode))
                data.Add(keys.ErrorCode, (int)dataSource[keys.ErrorCode]);
            if (dataSource.ContainsKey(keys.ErrorMessage))
                data.Add(keys.ErrorMessage, (string)dataSource[keys.ErrorMessage]);
            if (dataSource.ContainsKey(keys.ErrorData))
                data.Add(keys.ErrorData, (string)dataSource[keys.ErrorData]);
            if (dataSource.ContainsKey(keys.ErrorField))
                data.Add(keys.ErrorField, (string)dataSource[keys.ErrorField]);
        }

        protected override HttpRequestMessage CreateUserInfoRequest()
        {
            const string REQUEST_URL = "https://api.ok.ru/api/users/getCurrentUser";
            using (var md5 = MD5.Create())
            {
                string sessionSecretKey = md5.ComputeHash(AccessToken + ClientSecret);
                string signatureSource = ""
                    + $"application_key={_publicKey}"
                    + $"fields=uid,name,email,pic50x50"
                    + $"format=json"
                    + $"{sessionSecretKey}";
                string signature = md5.ComputeHash(signatureSource);
                var queryParams = new Dictionary<string, string>
                {
                    ["application_key"] = _publicKey,
                    ["fields"] = "uid,name,email,pic50x50",
                    ["format"] = "json",
                    ["access_token"] = AccessToken,
                    ["sig"] = signature
                };
                string requestUri = QueryHelpers.AddQueryString(REQUEST_URL, queryParams);
                return new HttpRequestMessage(HttpMethod.Get, requestUri);
            }
        }

        protected override bool IsErrorUserInfoResponse(JObject parsedResponse) => parsedResponse.ContainsKey("error_code");

        protected override Task HandleUserInfoResponseAsync(JObject userInfoResponse)
        {
            UserInfo = new ExternalUserInfo
            {
                Id = (string)userInfoResponse["uid"],
                Name = (string)userInfoResponse["name"],
                Email = (string)userInfoResponse["email"],
                AccessToken = AccessToken,
                Picture = (string)userInfoResponse["pic50x50"],
                Provider = _providerName
            };
            return Task.CompletedTask;
        }
    }
}
