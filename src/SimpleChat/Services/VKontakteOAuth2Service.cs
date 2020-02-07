using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;

namespace SimpleChat.Services
{
    public class VKontakteOAuth2Service : OAuth2ServiceBase, IVKontakteOAuth2Service
    {
        private const string AccessTokenEndpoint = "https://oauth.vk.com/access_token";
        private const string UserInfoEndpoint = "https://api.vk.com/method/users.get";
        private string _userEmail;

        public override string AccessToken
        {
            get => base.AccessToken;
            set
            {
                _userEmail = string.Empty;
                base.AccessToken = value;
            }
        }

        public VKontakteOAuth2Service(IConfiguration configuration, IUriHelper uriHelper)
            : this(configuration, uriHelper, null, null)
        {
        }

        public VKontakteOAuth2Service(
            IConfiguration configuration,
            IUriHelper uriHelper,
            IJsonHelper jsonHelper,
            IGuard guard)
            : base(ConfigurationKeys.VKontakteClientId, ConfigurationKeys.VKontakteClientSecret,
                  ExternalProvider.VKontakte, configuration, uriHelper, jsonHelper, guard)
        {
        }

        protected override HttpRequestMessage CreateAccessTokenRequest(string code)
        {
            string requestUri = _uriHelper.AddQueryString(AccessTokenEndpoint, new Dictionary<string, string>
            {
                ["client_id"] = ClientId,
                ["client_secret"] = ClientSecret,
                ["redirect_uri"] = RedirectUri,
                ["code"] = code,
            });
            return new HttpRequestMessage(HttpMethod.Get, requestUri);
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
                RequestParams = "request_params"
            };
            if (dataSource.ContainsKey(keys.Error))
            {
                if (dataSource[keys.Error].Type == JTokenType.String)
                {
                    data.Add(keys.Error, (string)dataSource[keys.Error]);
                }
                else // Type == JTokenType.Object
                {
                    var errorObject = (JObject)dataSource[keys.Error];
                    if (errorObject.ContainsKey(keys.ErrorCode))
                        data.Add(keys.ErrorCode, (int)errorObject[keys.ErrorCode]);
                    if (errorObject.ContainsKey(keys.ErrorMessage))
                        data.Add(keys.ErrorMessage, (string)errorObject[keys.ErrorMessage]);
                    if (errorObject.ContainsKey(keys.RequestParams))
                    {
                        data.Add(keys.RequestParams, errorObject[keys.RequestParams]
                            .ToObject<KeyValuePair<string, string>[]>());
                    }
                }
            }
            if (dataSource.ContainsKey(keys.ErrorDescription))
                data.Add(keys.ErrorDescription, (string)dataSource[keys.ErrorDescription]);
        }

        protected override Task HandleAccessTokenResponseAsync(JObject accessTokenResponse)
        {
            _userEmail = (string)accessTokenResponse["email"];
            base.AccessToken = (string)accessTokenResponse["access_token"];
            return Task.CompletedTask;
        }

        protected override HttpRequestMessage CreateUserInfoRequest()
        {
            string requestUri = _uriHelper.AddQueryString(UserInfoEndpoint, new Dictionary<string, string>
            {
                ["access_token"] = AccessToken,
                ["fields"] = "photo_50",
                ["name_case"] = "nom",
                ["v"] = "5.101"
            });
            return new HttpRequestMessage(HttpMethod.Get, requestUri);
        }

        protected override bool IsErrorUserInfoResponse(JObject parsedResponse)
        {
            return IsErrorAccessTokenResponse(parsedResponse);
        }

        protected override Task HandleUserInfoResponseAsync(JObject userInfoResponse)
        {
            UserInfo = new ExternalUserInfo
            {
                Id = (string)userInfoResponse["response"].First["id"],
                Name = string.Format("{0} {1}",
                    (string)userInfoResponse["response"].First["first_name"],
                    (string)userInfoResponse["response"].First["last_name"]),
                Email = _userEmail,
                AccessToken = AccessToken,
                Picture = (string)userInfoResponse["response"].First["photo_50"],
                Provider = _providerName
            };
            return Task.CompletedTask;
        }
    }
}
