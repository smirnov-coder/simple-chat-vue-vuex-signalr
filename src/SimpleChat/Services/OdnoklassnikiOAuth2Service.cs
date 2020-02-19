using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Models;
using SimpleChat.Infrastructure.Helpers;

namespace SimpleChat.Services
{
    /// <inheritdoc cref="IOdnoklassnikiOAuth2Service"/>
    public class OdnoklassnikiOAuth2Service : OAuth2ServiceBase, IOdnoklassnikiOAuth2Service
    {
        private readonly string _publicKey;
        private const string AccessTokenEndpoint = "https://api.ok.ru/oauth/token.do";
        private const string UserInfoEndpoint = "https://api.ok.ru/api/users/getCurrentUser";

        private IMD5Hasher _md5Hasher = new MD5Hasher();
        /// <summary>
        /// Компонент для работы с MD5.
        /// </summary>
        public IMD5Hasher MD5Hasher
        {
            get => _md5Hasher;
            set => _md5Hasher = _guard.EnsureObjectParamIsNotNull(value, nameof(MD5Hasher));
        }

        /// <inheritdoc cref="OAuth2ServiceBase(string, string, string, IConfiguration, IUriHelper, IJsonHelper, IGuard)"/>
        public OdnoklassnikiOAuth2Service(IConfiguration configuration, IUriHelper uriHelper)
            : this(configuration, uriHelper, null, null)
        {
        }

        /// <inheritdoc cref="OAuth2ServiceBase(string, string, string, IConfiguration, IUriHelper, IJsonHelper, IGuard)"/>
        public OdnoklassnikiOAuth2Service(
            IConfiguration configuration,
            IUriHelper uriHelper,
            IJsonHelper jsonHelper,
            IGuard guard = null)
            : base(ConfigurationKeys.OdnoklassnikiApplicationId, ConfigurationKeys.OdnoklassnikiApplicationSecretKey,
                  ExternalProvider.Odnoklassniki, configuration, uriHelper, jsonHelper, guard)
        {
            _publicKey = _configuration[ConfigurationKeys.OdnoklassnikiApplicationKey];
        }


        protected override HttpRequestMessage CreateAccessTokenRequest(string code)
        {
            string requestUri = _uriHelper.AddQueryString(AccessTokenEndpoint, new Dictionary<string, string>
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
            };
        }

        protected override bool IsErrorAccessTokenResponse(JObject parsedResponse)
        { 
            return parsedResponse.ContainsKey("error");
        }

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
            string sessionSecretKey = MD5Hasher.ComputeHash(AccessToken + ClientSecret);
            string signatureSource = ""
                + $"application_key={_publicKey}"
                + $"fields=uid,name,email,pic50x50"
                + $"format=json"
                + $"{sessionSecretKey}";
            string signature = MD5Hasher.ComputeHash(signatureSource);
            var queryParams = new Dictionary<string, string>
            {
                ["application_key"] = _publicKey,
                ["fields"] = "uid,name,email,pic50x50",
                ["format"] = "json",
                ["access_token"] = AccessToken,
                ["sig"] = signature
            };
            string requestUri = _uriHelper.AddQueryString(UserInfoEndpoint, queryParams);
            return new HttpRequestMessage(HttpMethod.Get, requestUri);
        }

        protected override bool IsErrorUserInfoResponse(JObject parsedResponse)
        {
            return parsedResponse.ContainsKey("error_code");
        }

        protected override Task HandleUserInfoResponseAsync(JObject userInfoResponse)
        {
            UserInfo = new ExternalUserInfo
            {
                Id = (string)userInfoResponse["uid"],
                Name = (string)userInfoResponse["name"],
                Email = (string)userInfoResponse["email"],
                AccessToken = AccessToken,
                Picture = (string)userInfoResponse["pic50x50"],
                Provider = _provider
            };
            return Task.CompletedTask;
        }
    }
}
