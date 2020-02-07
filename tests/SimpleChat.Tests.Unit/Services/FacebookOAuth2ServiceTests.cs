using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
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
    public class FacebookOAuth2ServiceTests
    {
        private Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private Mock<HttpClient> _mockHttpClient = new Mock<HttpClient>();
        private Mock<IGuard> _mockGuard = new Mock<IGuard>();
        private Mock<IJsonHelper> _mockJsonHelper = new Mock<IJsonHelper>();
        private Mock<IUriHelper> _mockUriHelper = new Mock<IUriHelper>();
        private const string AccessTokenEndpoint = "https://graph.facebook.com/v3.3/oauth/access_token";
        private const string UserInfoEndpoint = "https://graph.facebook.com/me";
        private FacebookOAuth2Service _target;

        public FacebookOAuth2ServiceTests()
        {
            _mockConfiguration.Setup(x => x["Authentication:Facebook:AppId"]).Returns("test_client_id");
            _mockConfiguration.Setup(x => x["Authentication:Facebook:AppSecret"]).Returns("test_client_secret");
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
            _target = new FacebookOAuth2Service(_mockConfiguration.Object, _mockUriHelper.Object,
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
                clientIdKey = "Authentication:Facebook:AppId",
                clientSecretKey = "Authentication:Facebook:AppSecret",
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
            var target = new FacebookOAuth2Service(mockConfiguration.Object, _mockUriHelper.Object,
                _mockJsonHelper.Object, mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientIdKey, "clientIdKey"), Times.Once());
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientSecretKey, "clientSecretKey"),
                Times.Once());
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty("Facebook", "providerName"), Times.Once());
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientId, "ClientId"), Times.Once());
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientSecret, "ClientSecret"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(mockConfiguration.Object, "configuration"),
                Times.Once());
            mockConfiguration.Verify(x => x[clientIdKey], Times.Once());
            mockConfiguration.Verify(x => x[clientSecretKey], Times.Once());
            Assert.NotNull(target.HttpClient);
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Good()
        {
            // arrange
            string 
                shortAccessToken = "short_access_token",
                longAccessToken = "long_access_token",
                shortAccessTokenJson = GetAccessTokenJson(shortAccessToken),
                longAccessTokenJson = GetAccessTokenJson(longAccessToken);
            _mockUriHelper.SetupSequence(x => x.AddQueryString(It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>>()))
                .Returns(GetAccessTokenRequestUri())
                .Returns(GetExchangeRequestUri());
            _mockHttpClient.SetupSequence(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(GetAccessTokenResponse(shortAccessToken))
                .ReturnsAsync(GetAccessTokenResponse(longAccessToken));
            _mockJsonHelper.SetupSequence(x => x.Parse(It.IsAny<string>()))
                .Returns(JObject.Parse(GetAccessTokenJson(shortAccessToken)))
                .Returns(JObject.Parse(GetAccessTokenJson(longAccessToken)));

            // act
            await _target.RequestAccessTokenAsync("test_code");

            // assert
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(shortAccessToken, "accessToken"), Times.Once());
            _mockUriHelper.Verify(x => x.AddQueryString(AccessTokenEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyAccessTokenRequestParams(queryParams))), Times.Once());
            _mockUriHelper.Verify(x => x.AddQueryString(AccessTokenEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyExchangeRequestParams(queryParams))), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request => 
                VerifyAccessTokenRequest(request)), default(CancellationToken)), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request =>
                VerifyExchangeRequest(request)), default(CancellationToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetAccessTokenJson(shortAccessToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetAccessTokenJson(longAccessToken)), Times.Once());
            Assert.Equal(longAccessToken, _target.AccessToken);
        }

        private string GetAccessTokenRequestUri()
        {
            return $"{AccessTokenEndpoint}?" +
                $"client_id=test_client_id&" +
                $"client_secret=test_client_secret&" +
                $"redirect_uri=test_redirect_uri&" +
                $"code=test_code";
        }

        private string GetExchangeRequestUri()
        {
            return $"{AccessTokenEndpoint}?" +
                $"client_id=test_client_id&" +
                $"client_secret=test_client_secret&" +
                $"grant_type=fb_exchange_token&" +
                $"fb_exchange_token=short_access_token&" +
                $"access_token=short_access_token";
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

        private bool VerifyExchangeRequestParams(IDictionary<string, string> queryParams)
        {
            return queryParams.ContainsKey("client_id") && queryParams["client_id"] == "test_client_id"
                && queryParams.ContainsKey("client_secret") && queryParams["client_secret"] == "test_client_secret"
                && queryParams.ContainsKey("grant_type") && queryParams["grant_type"] == "fb_exchange_token"
                && queryParams.ContainsKey("fb_exchange_token")
                && queryParams["fb_exchange_token"] == "short_access_token"
                && queryParams.ContainsKey("access_token") && queryParams["access_token"] == "short_access_token";

        }

        private HttpResponseMessage GetAccessTokenResponse(string accessToken)
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(GetAccessTokenJson(accessToken))
            };
        }

        private string GetAccessTokenJson(string accessToken)
        {
            return $"{{\"access_token\":\"{accessToken}\"}}";
        }

        private bool VerifyExchangeRequest(HttpRequestMessage request)
        {
            var queryParams = QueryHelpers.ParseNullableQuery(request.RequestUri.Query);
            string
                scheme = request.RequestUri.Scheme,
                host = request.RequestUri.Host,
                path = request.RequestUri.AbsolutePath,
                requestUri = $"{scheme}://{host}{path}";
            return request.Method == HttpMethod.Get
                && requestUri == AccessTokenEndpoint
                && VerifyExchangeRequestParams(queryParams.ToDictionary(entry => entry.Key,
                    entry => (string)entry.Value));
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Bad_OAuth2ServiceException_ConnectionError()
        {
            // arrange
            _mockUriHelper.Setup(x => x.AddQueryString(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(GetAccessTokenRequestUri());
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(
                () => _target.RequestAccessTokenAsync("test_code"));

            // assert
            _mockUriHelper.Verify(x => x.AddQueryString(AccessTokenEndpoint,
                It.Is<IDictionary<string, string>>(queryParams => VerifyAccessTokenRequestParams(queryParams))),
                Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(
                request => VerifyAccessTokenRequest(request)), default(CancellationToken)), Times.Once());
            Assert.Equal("Не удалось подключиться к 'Facebook' для обмена кода авторизации на маркер доступа.",
                ex.Message);
            Assert.True(string.IsNullOrWhiteSpace(_target.AccessToken));
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Bad_OAuth2ServiceException_ErrorAccessTokenResponse()
        {
            // arrange
            _mockUriHelper.Setup(x => x.AddQueryString(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(GetAccessTokenRequestUri());
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(GetErrorResponse());
            _mockJsonHelper.Setup(x => x.Parse(It.IsAny<string>())).Returns(JObject.Parse(GetErrorJson()));

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(
                () => _target.RequestAccessTokenAsync("test_code"));

            // assert
            _mockUriHelper.Verify(x => x.AddQueryString(AccessTokenEndpoint,
                It.Is<IDictionary<string, string>>(queryParams => VerifyAccessTokenRequestParams(queryParams))),
                Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(
                request => VerifyAccessTokenRequest(request)), default(CancellationToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetErrorJson()), Times.Once());
            Assert.Equal("Не удалось обменять код авторизации на маркер доступа.", ex.Message);
            Assert.Equal(7, ex.Data.Count);
            Assert.True(ex.Data.Contains("message"));
            Assert.True(ex.Data.Contains("type"));
            Assert.True(ex.Data.Contains("code"));
            Assert.True(ex.Data.Contains("error_subcode"));
            Assert.True(ex.Data.Contains("error_user_title"));
            Assert.True(ex.Data.Contains("error_user_msg"));
            Assert.True(ex.Data.Contains("fbtrace_id"));
            Assert.True(string.IsNullOrWhiteSpace(_target.AccessToken));
        }

        private HttpResponseMessage GetErrorResponse()
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(GetErrorJson())
            };
        }

        private string GetErrorJson()
        {
            // Пример ошибки Facebook
            // https://developers.facebook.com/docs/graph-api/using-graph-api/error-handling
            var errorResponse = new
            {
                error = new
                {
                    message = "Message describing the error",
                    type = "OAuthException",
                    code = 190,
                    error_subcode = 460,
                    error_user_title = "A title",
                    error_user_msg = "A message",
                    fbtrace_id = "EJplcsCHuLu"
                }
            };
            return JsonConvert.SerializeObject(errorResponse);
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Bad_OAuth2ServiceException_ExchangeAccessTokenConnectionError()
        {
            // arrange
            string shortAccessToken = "short_access_token";
            _mockUriHelper.SetupSequence(x => x.AddQueryString(It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>>()))
                .Returns(GetAccessTokenRequestUri())
                .Returns(GetExchangeRequestUri());
            _mockHttpClient.SetupSequence(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(GetAccessTokenResponse(shortAccessToken))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));
            _mockJsonHelper.Setup(x => x.Parse(It.IsAny<string>()))
                .Returns(JObject.Parse(GetAccessTokenJson(shortAccessToken)));

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(
                () => _target.RequestAccessTokenAsync("test_code"));

            // assert
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(shortAccessToken, "accessToken"), Times.Once());
            _mockUriHelper.Verify(x => x.AddQueryString(AccessTokenEndpoint,
                It.Is<IDictionary<string, string>>(queryParams =>
                VerifyAccessTokenRequestParams(queryParams))), Times.Once());
            _mockUriHelper.Verify(x => x.AddQueryString(AccessTokenEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyExchangeRequestParams(queryParams))), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request =>
                VerifyAccessTokenRequest(request)), default(CancellationToken)), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request =>
                VerifyExchangeRequest(request)), default(CancellationToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetAccessTokenJson(shortAccessToken)), Times.Once());
            Assert.Equal($"Не удалось подключиться к 'Facebook' для обмена краткосрочного маркера доступа на " +
                $"долгосрочный.", ex.Message);
            Assert.True(string.IsNullOrWhiteSpace(_target.AccessToken));
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Bad_OAuth2ServiceException_ErrorExchangeAccessTokenResponse()
        {
            // arrange
            string shortAccessToken = "short_access_token";
            _mockUriHelper.SetupSequence(x => x.AddQueryString(It.IsAny<string>(),
                It.IsAny<IDictionary<string, string>>()))
                .Returns(GetAccessTokenRequestUri())
                .Returns(GetExchangeRequestUri());
            _mockHttpClient.SetupSequence(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(GetAccessTokenResponse(shortAccessToken))
                .ReturnsAsync(GetErrorResponse());
            _mockJsonHelper.SetupSequence(x => x.Parse(It.IsAny<string>()))
                .Returns(JObject.Parse(GetAccessTokenJson(shortAccessToken)))
                .Returns(JObject.Parse(GetErrorJson()));

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(
                () => _target.RequestAccessTokenAsync("test_code"));

            // assert
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(shortAccessToken, "accessToken"), Times.Once());
            _mockUriHelper.Verify(x => x.AddQueryString(AccessTokenEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyAccessTokenRequestParams(queryParams))), Times.Once());
            _mockUriHelper.Verify(x => x.AddQueryString(AccessTokenEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyExchangeRequestParams(queryParams))), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request =>
                VerifyAccessTokenRequest(request)), default(CancellationToken)), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request =>
                VerifyExchangeRequest(request)), default(CancellationToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetAccessTokenJson(shortAccessToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetErrorJson()), Times.Once());
            Assert.Equal("Не удалось обменять краткосрочный маркер доступа на долгосрочный.", ex.Message);
            Assert.Equal(7, ex.Data.Count);
            Assert.True(ex.Data.Contains("message"));
            Assert.True(ex.Data.Contains("type"));
            Assert.True(ex.Data.Contains("code"));
            Assert.True(ex.Data.Contains("error_subcode"));
            Assert.True(ex.Data.Contains("error_user_title"));
            Assert.True(ex.Data.Contains("error_user_msg"));
            Assert.True(ex.Data.Contains("fbtrace_id"));
            Assert.True(string.IsNullOrWhiteSpace(_target.AccessToken));
        }

        [Fact]
        public async Task RequestUserInfoAsync_Good()
        {
            // arrange
            _mockUriHelper.Setup(x => x.AddQueryString(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(GetUserInfoRequestUri());
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(GetUserInfoResponse());
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
            Assert.Equal("test_name", _target.UserInfo.Name);
            Assert.Equal("test_email", _target.UserInfo.Email);
            Assert.Equal("test_url", _target.UserInfo.Picture);
            Assert.Equal("test_access_token", _target.UserInfo.AccessToken);
            Assert.Equal("Facebook", _target.UserInfo.Provider);
        }

        private string GetUserInfoRequestUri()
        {
            return $"{UserInfoEndpoint}?fields=id,name,email,picture&access_token=test_access_token";
        }

        private HttpResponseMessage GetUserInfoResponse()
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(GetUserInfoJson())
            };
        }

        private string GetUserInfoJson()
        {
            var response = new
            {
                id = "test_id",
                name = "test_name",
                email = "test_email",
                picture = new { data = new { url = "test_url" } }
            };
            return JsonConvert.SerializeObject(response);
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

        private bool VerifyUserInfoRequestParams(IDictionary<string, string> queryParams)
        {
            return queryParams.ContainsKey("fields") && queryParams["fields"] == "id,name,email,picture"
                && queryParams.ContainsKey("access_token") && queryParams["access_token"] == "test_access_token";
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
            Assert.Equal($"Не удалось подключиться к 'Facebook' для получения информации о пользователе.", ex.Message);
            Assert.Null(_target.UserInfo);
        }

        [Fact]
        public async Task RequestUserInfoAsync_Bad_OAuth2ServiceException_ErrorUserInfoResponse()
        {
            // arrange
            _mockUriHelper.Setup(x => x.AddQueryString(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(GetUserInfoRequestUri());
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(GetErrorResponse());
            _mockJsonHelper.Setup(x => x.Parse(It.IsAny<string>())).Returns(JObject.Parse(GetErrorJson()));
            _target.AccessToken = "test_access_token";

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestUserInfoAsync());

            // assert
            _mockUriHelper.Verify(x => x.AddQueryString(UserInfoEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyUserInfoRequestParams(queryParams))), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request => 
                VerifyUserInfoRequest(request)), default(CancellationToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetErrorJson()), Times.Once());
            Assert.Equal("Не удалось получить информацию о пользователе.", ex.Message);
            Assert.Equal(7, ex.Data.Count);
            Assert.True(ex.Data.Contains("message"));
            Assert.True(ex.Data.Contains("type"));
            Assert.True(ex.Data.Contains("code"));
            Assert.True(ex.Data.Contains("error_subcode"));
            Assert.True(ex.Data.Contains("error_user_title"));
            Assert.True(ex.Data.Contains("error_user_msg"));
            Assert.True(ex.Data.Contains("fbtrace_id"));
            Assert.Null(_target.UserInfo);
        }
    }
}
