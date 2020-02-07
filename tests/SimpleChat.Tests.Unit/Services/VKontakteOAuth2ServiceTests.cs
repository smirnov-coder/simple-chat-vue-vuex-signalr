using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleChat.Infrastructure.Helpers;
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
        private Mock<IGuard> _mockGuard = new Mock<IGuard>();
        private Mock<IJsonHelper> _mockJsonHelper = new Mock<IJsonHelper>();
        private Mock<IUriHelper> _mockUriHelper = new Mock<IUriHelper>();
        private Mock<HttpClient> _mockHttpClient = new Mock<HttpClient>();
        private const string AccessTokenEndpoint = "https://oauth.vk.com/access_token";
        private const string UserInfoEndpoint = "https://api.vk.com/method/users.get";
        private VKontakteOAuth2Service _target;

        public VKontakteOAuth2ServiceTests()
        {
            _mockConfiguration.Setup(x => x["Authentication:VKontakte:ClientId"]).Returns("test_client_id");
            _mockConfiguration.Setup(x => x["Authentication:VKontakte:ClientSecret"]).Returns("test_client_secret");
            _mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockConfiguration.Object, "configuration"))
                .Returns(_mockConfiguration.Object);
            _mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockHttpClient.Object, "HttpClient"))
                .Returns(_mockHttpClient.Object);
            _mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockJsonHelper.Object, "jsonHelper"))
                .Returns(_mockJsonHelper.Object);
            _mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockUriHelper.Object, "uriHelper"))
                .Returns(_mockUriHelper.Object);
            _mockGuard.Setup(x => x.EnsureStringParamIsNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((value, paramName) => value);
            _target = new VKontakteOAuth2Service(_mockConfiguration.Object, _mockUriHelper.Object,
                _mockJsonHelper.Object, _mockGuard.Object)
            {
                HttpClient = _mockHttpClient.Object,
                RedirectUri = "test_redirect_uri"
            };
        }

        [Fact]
        public void Constructor_Good()
        {
            // arrange
            string
                clientIdKey = "Authentication:VKontakte:ClientId",
                clientSecretKey = "Authentication:VKontakte:ClientSecret",
                clientId = "test_client_id",
                clientSecret = "test_client_secret";
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupSequence(x => x[It.IsAny<string>()])
                .Returns(clientId)
                .Returns(clientSecret);
            var mockGuard = new Mock<IGuard>();
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(It.IsAny<IConfiguration>(), It.IsAny<string>()))
                .Returns(mockConfiguration.Object);
            mockGuard.Setup(x => x.EnsureStringParamIsNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((value, paramName) => value);

            // act
            var target = new VKontakteOAuth2Service(mockConfiguration.Object, _mockUriHelper.Object,
                _mockJsonHelper.Object, mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientIdKey, "clientIdKey"), Times.Once());
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientSecretKey, "clientSecretKey"), Times.Once);
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty("ВКонтакте", "providerName"), Times.Once());
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientId, "ClientId"), Times.Once());
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientSecret, "ClientSecret"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(mockConfiguration.Object, "configuration"), Times.Once);
            mockConfiguration.Verify(x => x[clientIdKey], Times.Once());
            mockConfiguration.Verify(x => x[clientSecretKey], Times.Once());
            Assert.NotNull(target.HttpClient);
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Good()
        {
            // arrange
            _mockUriHelper.Setup(x => x.AddQueryString(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(GetAccessTokenRequestUri());
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateResponse(GetAccessTokenJson()));
            _mockJsonHelper.Setup(x => x.Parse(It.IsAny<string>())).Returns(JObject.Parse(GetAccessTokenJson()));

            // act
            await _target.RequestAccessTokenAsync("test_code");

            // assert
            _mockUriHelper.Verify(x => x.AddQueryString(AccessTokenEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyAccessTokenRequestParams(queryParams))), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(
                request => VerifyAccessTokenRequest(request)), default(CancellationToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetAccessTokenJson()), Times.Once());
            Assert.Equal("test_access_token", _target.AccessToken);
        }

        private string GetAccessTokenRequestUri()
        {
            return $"{AccessTokenEndpoint}?" +
                $"client_id=test_client_id&" +
                $"client_secret=test_client_secret&" +
                $"redirect_uri=test_redirect_uri&" +
                $"code=test_code";
        }

        private HttpResponseMessage CreateResponse(string responseJson)
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            };
        }

        private string GetAccessTokenJson()
        {
            return "{\"access_token\":\"test_access_token\"}";
        }

        private bool VerifyAccessTokenRequestParams(IDictionary<string, string> queryParams)
        {
            return queryParams.ContainsKey("client_id") && queryParams["client_id"] == "test_client_id"
                && queryParams.ContainsKey("client_secret") && queryParams["client_secret"] == "test_client_secret"
                && queryParams.ContainsKey("redirect_uri") && queryParams["redirect_uri"] == "test_redirect_uri"
                && queryParams.ContainsKey("code") && queryParams["code"] == "test_code";
        }

        private bool VerifyAccessTokenRequest(HttpRequestMessage request)
        {
            var queryParams = QueryHelpers.ParseNullableQuery(request.RequestUri.Query);
            string
                scheme = request.RequestUri.Scheme,
                host = request.RequestUri.Host,
                path = request.RequestUri.AbsolutePath,
                requestUri = $"{scheme}://{host}{path}";
            return request.Method == HttpMethod.Get
                && requestUri == AccessTokenEndpoint
                && VerifyAccessTokenRequestParams(queryParams.ToDictionary(entry => entry.Key,
                    entry => (string)entry.Value));
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Bad_OAuth2ServiceException_ConnectionError()
        {
            // arrange
            _mockUriHelper.Setup(x => x.AddQueryString(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(GetAccessTokenRequestUri());
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(
                () => _target.RequestAccessTokenAsync("test_code"));

            // assert
            _mockUriHelper.Verify(x => x.AddQueryString(AccessTokenEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyAccessTokenRequestParams(queryParams))), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request =>
                VerifyAccessTokenRequest(request)), default(CancellationToken)), Times.Once());
            Assert.Equal($"Не удалось подключиться к 'ВКонтакте' для обмена кода авторизации на маркер доступа.",
                ex.Message);
            Assert.True(string.IsNullOrWhiteSpace(_target.AccessToken));
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Bad_OAuth2ServiceException_ErrorAccessTokenResponse()
        {
            // arrange
            _mockUriHelper.Setup(x => x.AddQueryString(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(GetAccessTokenRequestUri());
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateResponse(GetErrorAccessTokenJson()));
            _mockJsonHelper.Setup(x => x.Parse(It.IsAny<string>())).Returns(JObject.Parse(GetErrorAccessTokenJson()));

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(
                () => _target.RequestAccessTokenAsync("test_code"));

            // assert
            _mockUriHelper.Verify(x => x.AddQueryString(AccessTokenEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyAccessTokenRequestParams(queryParams))), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request =>
                VerifyAccessTokenRequest(request)), default(CancellationToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetErrorAccessTokenJson()), Times.Once());
            Assert.Equal("Не удалось обменять код авторизации на маркер доступа.", ex.Message);
            Assert.Equal(2, ex.Data.Count);
            Assert.True(ex.Data.Contains("error"));
            Assert.True(ex.Data.Contains("error_description"));
            Assert.True(string.IsNullOrWhiteSpace(_target.AccessToken));
        }

        private string GetErrorAccessTokenJson()
        {
            // Пример ошибки OAuth ВКонтакте
            // https://vk.com/dev.php?method=authcode_flow_user
            var response = new
            {
                error = "invalid_grant",
                error_description = "Code is expired."
            };
            return JsonConvert.SerializeObject(response);
        }

        [Fact]
        public async Task RequestUserInfoAsync_Good()
        {
            // arrange
            _mockUriHelper.Setup(x => x.AddQueryString(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(GetUserInfoRequestUri());
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(CreateResponse(GetUserInfoJson()));
            _mockJsonHelper.Setup(x => x.Parse(It.IsAny<string>())).Returns(JObject.Parse(GetUserInfoJson()));
            _target.AccessToken = "test_access_token";

            // act
            await _target.RequestUserInfoAsync();

            // assert
            _mockUriHelper.Verify(x => x.AddQueryString(UserInfoEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyUserInfoRequestParams(queryParams))), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(
                request => VerifyUserInfoRequest(request)), default(CancellationToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetUserInfoJson()), Times.Once());
            Assert.Equal("test_id", _target.UserInfo.Id);
            Assert.Equal("test_first_name test_last_name", _target.UserInfo.Name);
            Assert.Equal("test_url", _target.UserInfo.Picture);
            Assert.Equal("test_access_token", _target.UserInfo.AccessToken);
            Assert.Equal("ВКонтакте", _target.UserInfo.Provider);
            Assert.True(string.IsNullOrWhiteSpace(_target.UserInfo.Email));
        }

        private string GetUserInfoRequestUri()
        {
            return $"{UserInfoEndpoint}?fields=photo_50&name_case=nom&v=5.101&access_token=test_access_token";
        }

        private string GetUserInfoJson()
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
            return JsonConvert.SerializeObject(response);
        }

        private bool VerifyUserInfoRequestParams(IDictionary<string, string> queryParams)
        {
            return queryParams.ContainsKey("fields") && queryParams["fields"] == "photo_50"
                && queryParams.ContainsKey("name_case") && queryParams["name_case"] == "nom"
                && queryParams.ContainsKey("v") && queryParams["v"] == "5.101"
                && queryParams.ContainsKey("access_token") && queryParams["access_token"] == "test_access_token";
        }

        private bool VerifyUserInfoRequest(HttpRequestMessage request)
        {
            var queryParams = QueryHelpers.ParseNullableQuery(request.RequestUri.Query);
            string
                scheme = request.RequestUri.Scheme,
                host = request.RequestUri.Host,
                path = request.RequestUri.AbsolutePath,
                requestUri = $"{scheme}://{host}{path}";
            return request.Method == HttpMethod.Get
                && requestUri == UserInfoEndpoint
                && VerifyUserInfoRequestParams(queryParams.ToDictionary(entry => entry.Key,
                    entry => (string)entry.Value));
        }

        [Fact]
        public async Task RequestUserInfoAsync_Bad_OAuth2ServiceException_ConnectionError()
        {
            // arrange
            _mockUriHelper.Setup(x => x.AddQueryString(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(GetUserInfoRequestUri());
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));
            _target.AccessToken = "test_access_token";

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestUserInfoAsync());

            // assert
            _mockUriHelper.Verify(x => x.AddQueryString(UserInfoEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyUserInfoRequestParams(queryParams))), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request =>
                VerifyUserInfoRequest(request)), default(CancellationToken)), Times.Once());
            Assert.Equal($"Не удалось подключиться к 'ВКонтакте' для получения информации о пользователе.", ex.Message);
            Assert.Null(_target.UserInfo);
        }

        [Fact]
        public async Task RequestUserInfoAsync_Bad_OAuth2ServiceException_ErrorUserInfoResponse()
        {
            // arrange
            _mockUriHelper.Setup(x => x.AddQueryString(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(GetUserInfoRequestUri());
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(CreateResponse(GetErrorUserInfoJson()));
            _mockJsonHelper.Setup(x => x.Parse(It.IsAny<string>())).Returns(JObject.Parse(GetErrorUserInfoJson()));
            _target.AccessToken = "test_access_token";

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestUserInfoAsync());

            // assert
            _mockUriHelper.Verify(x => x.AddQueryString(UserInfoEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyUserInfoRequestParams(queryParams))), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request =>
                VerifyUserInfoRequest(request)), default(CancellationToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetErrorUserInfoJson()), Times.Once());
            Assert.Equal("Не удалось получить информацию о пользователе.", ex.Message);
            Assert.Equal(3, ex.Data.Count);
            Assert.True(ex.Data.Contains("error_code"));
            Assert.True(ex.Data.Contains("error_msg"));
            Assert.True(ex.Data.Contains("request_params"));
            Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, string>>>(ex.Data["request_params"]);
            Assert.Equal(2, (ex.Data["request_params"] as IEnumerable<KeyValuePair<string, string>>).Count());
            Assert.Null(_target.UserInfo);
        }
        private string GetErrorUserInfoJson()
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
            return JsonConvert.SerializeObject(response);
        }
    }
}
