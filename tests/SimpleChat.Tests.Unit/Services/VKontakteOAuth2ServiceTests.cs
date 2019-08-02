using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using SimpleChat.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Services
{
    public class VKontakteOAuth2ServiceTests
    {
        private Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private Mock<HttpClient> _mockHttpClient = new Mock<HttpClient>();
        private VKontakteOAuth2Service _target;

        public VKontakteOAuth2ServiceTests()
        {
            _mockConfiguration.Setup(x => x["Authentication:VKontakte:ClientId"]).Returns("test_client_id");
            _mockConfiguration.Setup(x => x["Authentication:VKontakte:ClientSecret"]).Returns("test_client_secret");
            _target = new VKontakteOAuth2Service(_mockConfiguration.Object)
            {
                HttpClient = _mockHttpClient.Object,
                RedirectUri = "test_redirect_uri"
            };
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Good()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(GetSuccessAccessTokenResponse());

            await _target.RequestAccessTokenAsync("test_code");

            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request => VerifyAccessTokenRequest(request)), default(CancellationToken)), Times.Once());
            Assert.Equal("test_access_token", _target.AccessToken);
        }

        private HttpResponseMessage GetSuccessAccessTokenResponse()
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"access_token\":\"test_access_token\"}")
            };
        }

        private bool VerifyAccessTokenRequest(HttpRequestMessage request)
        {
            var queryParams = QueryHelpers.ParseNullableQuery(request.RequestUri.Query);
            string requestUri = $"{request.RequestUri.Scheme}://{request.RequestUri.Host}{request.RequestUri.AbsolutePath}";
            return request.Method == HttpMethod.Get
                && requestUri == "https://oauth.vk.com/access_token"
                && queryParams.ContainsKey("client_id") && queryParams["client_id"] == "test_client_id"
                && queryParams.ContainsKey("client_secret") && queryParams["client_secret"] == "test_client_secret"
                && queryParams.ContainsKey("redirect_uri") && queryParams["redirect_uri"] == "test_redirect_uri"
                && queryParams.ContainsKey("code") && queryParams["code"] == "test_code";
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Bad_OAuth2ServiceException_ErrorAccessTokenResponse()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(GetErrorAccessTokenResponse());

            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestAccessTokenAsync("test_code"));

            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request => VerifyAccessTokenRequest(request)), default(CancellationToken)), Times.Once());
            Assert.Equal("Не удалось обменять код авторизации на маркер доступа.", ex.Message);
            Assert.Equal(2, ex.Data.Count);
            Assert.True(ex.Data.Contains("error"));
            Assert.True(ex.Data.Contains("error_description"));
            Assert.True(string.IsNullOrWhiteSpace(_target.AccessToken));
        }

        private HttpResponseMessage GetErrorAccessTokenResponse()
        {
            // Пример ошибки OAuth ВКонтакте
            // https://vk.com/dev.php?method=authcode_flow_user
            var response = new
            {
                error = "invalid_grant",
                error_description = "Code is expired."
            };
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response))
            };
        }

        [Fact]
        public async Task RequestUserInfoAsync_Good()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(GetSuccessUserInfoResponse());
            _target.AccessToken = "test_access_token";

            await _target.RequestUserInfoAsync();

            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request => VerifyUserInfoRequest(request)), default(CancellationToken)), Times.Once());
            Assert.Equal("test_id", _target.UserInfo.Id);
            Assert.Equal("test_first_name test_last_name", _target.UserInfo.Name);
            Assert.Equal("test_url", _target.UserInfo.Picture);
            Assert.Equal("test_access_token", _target.UserInfo.AccessToken);
            Assert.Equal("ВКонтакте", _target.UserInfo.Provider);
            Assert.True(string.IsNullOrWhiteSpace(_target.UserInfo.Email));
        }

        private HttpResponseMessage GetSuccessUserInfoResponse()
        {
            var response = new
            {
                response = new[]
                {
                    new
                    {
                        id = "test_id",
                        first_name = "test_first_name",
                        last_name = "test_last_name",
                        photo_50 = "test_url"
                    }
                }
            };
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response))
            };
        }

        private bool VerifyUserInfoRequest(HttpRequestMessage request)
        {
            var queryParams = QueryHelpers.ParseNullableQuery(request.RequestUri.Query);
            string requestUri = $"{request.RequestUri.Scheme}://{request.RequestUri.Host}{request.RequestUri.AbsolutePath}";
            return request.Method == HttpMethod.Get
                && requestUri == "https://api.vk.com/method/users.get"
                && queryParams.ContainsKey("fields") && queryParams["fields"] == "photo_50"
                && queryParams.ContainsKey("name_case") && queryParams["name_case"] == "nom"
                && queryParams.ContainsKey("v") && queryParams["v"] == "5.101"
                && queryParams.ContainsKey("access_token") && queryParams["access_token"] == "test_access_token";
        }

        [Fact]
        public async Task RequestUserInfoAsync_Bad_OAuth2ServiceException_ErrorUserInfoResponse()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(GetErrorUserInfoResponse());
            _target.AccessToken = "test_access_token";

            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestUserInfoAsync());

            Assert.Equal("Не удалось получить информацию о пользователе.", ex.Message);
            Assert.Equal(3, ex.Data.Count);
            Assert.True(ex.Data.Contains("error_code"));
            Assert.True(ex.Data.Contains("error_msg"));
            Assert.True(ex.Data.Contains("request_params"));
            Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, string>>>(ex.Data["request_params"]);
            Assert.Equal(2, (ex.Data["request_params"] as IEnumerable<KeyValuePair<string, string>>).Count());
            Assert.Null(_target.UserInfo);
        }

        private HttpResponseMessage GetErrorUserInfoResponse()
        {
            var response = new
            {
                error = new
                {
                    error_code = 113,
                    error_msg = "Invalid user id",
                    request_params = new[]
                    {
                        new
                        {
                            key = "key1",
                            value = "value1"
                        },
                        new
                        {
                            key = "key2",
                            value = "value2"
                        }
                    }
                }
            };
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response))
            };
        }
    }
}
