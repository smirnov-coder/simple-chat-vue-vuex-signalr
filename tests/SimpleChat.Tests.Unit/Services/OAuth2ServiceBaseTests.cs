using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using SimpleChat.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Services
{
    class OAuth2ServiceBaseUnderTest : OAuth2ServiceBase
    {
        public OAuth2ServiceBaseUnderTest(
            IConfiguration configuration,
            IUriHelper uriHelper,
            IJsonHelper jsonHelper,
            IGuard guard)
            : base("test:client_id", "test:client_secret", "test_provider", configuration, uriHelper, jsonHelper, guard)
        {
        }

        public OAuth2ServiceBaseUnderTest(
            string clientIdKey,
            string clientSecretKey,
            string providerName,
            IConfiguration configuration,
            IUriHelper uriHelper,
            IJsonHelper jsonHelper,
            IGuard guard)
            : base(clientIdKey, clientSecretKey, providerName, configuration, uriHelper, jsonHelper, guard)
        {
        }

        protected override void CollectErrorData(IDictionary data, JObject dataSource)
        {
            data.Add("error", "test_error");
        }

        protected override HttpRequestMessage CreateAccessTokenRequest(string code) => AccessTokenRequest;

        public HttpRequestMessage AccessTokenRequest { get; set; } = new HttpRequestMessage(HttpMethod.Get,
            "www.example.com?code=test_code");

        protected override HttpRequestMessage CreateUserInfoRequest() => UserInfoRequest;

        public HttpRequestMessage UserInfoRequest { get; set; } = new HttpRequestMessage(HttpMethod.Get,
            "www.example.com?access_token=test_access_token");

        protected override Task HandleUserInfoResponseAsync(JObject userInfoResponse)
        {
            UserInfo = new ExternalUserInfo
            {
                Id = "test_id",
                Name = "test_name",
                Email = "test_email",
                AccessToken = AccessToken,
                Picture = "test_url",
                Provider = "test_provider"
            };
            return Task.CompletedTask;
        }

        public bool IsErrorResponse { get; set; }

        protected override bool IsErrorAccessTokenResponse(JObject parsedResponse) => IsErrorResponse;

        protected override bool IsErrorUserInfoResponse(JObject parsedResponse) => IsErrorResponse;
    }

    // Tests
    public class OAuth2ServiceBaseTests
    {
        private Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private Mock<IGuard> _mockGuard = new Mock<IGuard>();
        private Mock<IJsonHelper> _mockJsonHelper = new Mock<IJsonHelper>();
        private Mock<IUriHelper> _mockUriHelper = new Mock<IUriHelper>();
        private Mock<HttpClient> _mockHttpClient = new Mock<HttpClient>();
        private OAuth2ServiceBaseUnderTest _target;

        public OAuth2ServiceBaseTests()
        {
            _mockConfiguration.Setup(x => x["test:client_id"]).Returns("test_client_id");
            _mockConfiguration.Setup(x => x["test:client_secret"]).Returns("test_client_secret");
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
            _target = new OAuth2ServiceBaseUnderTest(_mockConfiguration.Object, _mockUriHelper.Object,
                _mockJsonHelper.Object, _mockGuard.Object)
            {
                HttpClient = _mockHttpClient.Object,
            };
        }

        [Fact]
        public void Constructor_Good()
        {
            // arrange
            string
                clientIdKey = "test:client_id",
                clientSecretKey = "test:client_secret",
                clientId = "test_client_id",
                clientSecret = "test_client_secret",
                providerName = "test_provider";
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
            var target = new OAuth2ServiceBaseUnderTest(clientIdKey, clientSecretKey, providerName,
                mockConfiguration.Object, _mockUriHelper.Object, _mockJsonHelper.Object, mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientIdKey, "clientIdKey"), Times.Once());
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientSecretKey, "clientSecretKey"),
                Times.Once());
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(providerName, "providerName"), Times.Once());
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientId, "ClientId"), Times.Once());
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientSecret, "ClientSecret"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(mockConfiguration.Object, "configuration"),
                Times.Once());
            mockConfiguration.Verify(x => x[clientIdKey], Times.Once());
            mockConfiguration.Verify(x => x[clientSecretKey], Times.Once());
            Assert.NotNull(target.HttpClient);
        }

        [Fact]
        public void RedirectUri_Good()
        {
            // act
            _target.RedirectUri = "test_redirect_uri";

            // assert
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty("test_redirect_uri", "RedirectUri"),
                Times.Once());
        }

        [Fact]
        public void AccessToken_Good()
        {
            // act
            _target.AccessToken = "test_access_token";

            // assert
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty("test_access_token", "AccessToken"),
                Times.Once());
        }

        [Fact]
        public void HttpClient_Good()
        {
            // arrange
            var testClient = new HttpClient();

            // act
            _target.HttpClient = testClient;

            // assert
            _mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(testClient, "HttpClient"), Times.Once());
        }

        [Theory]
        [MemberData(nameof(TestStatusCodes))]
        public async Task RequestAccessTokenAsync_Bad_OAuth2ServiceException_ConnectionError(HttpStatusCode statusCode)
        {
            // arrange
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(new HttpResponseMessage(statusCode));
            _target.RedirectUri = "test_redirect_uri";

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(
                () => _target.RequestAccessTokenAsync("test_code"));

            // assert
            VerifyGuardAccessTokenRequest();
            Assert.Equal("Не удалось подключиться к 'test_provider' для обмена кода авторизации на маркер доступа.",
                ex.Message);
        }

        private void VerifyGuardAccessTokenRequest()
        {
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty("test_code", "code"), Times.Once());
            _mockGuard.Verify(x => x.EnsureStringPropertyIsNotNullOrEmpty("test_redirect_uri", 
                "Не задан адрес обратного вызова (RedirectUri) для OAuth-службы 'test_provider'."), Times.Once());
        }

        public static IEnumerable<object[]> TestStatusCodes()
        {
            return new List<object[]>
            {
                new object[] { HttpStatusCode.BadRequest },
                new object[] { HttpStatusCode.InternalServerError },
                new object[] { HttpStatusCode.Unauthorized },
                new object[] { HttpStatusCode.BadGateway },
                new object[] { HttpStatusCode.NotFound },
                new object[] { HttpStatusCode.RequestTimeout },
                new object[] { HttpStatusCode.GatewayTimeout },
                new object[] { HttpStatusCode.Forbidden }
            };
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Bad_OAuth2ServiceException_ErrorAccessTokenResponse()
        {
            // arrange
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetErrorResponse());
            _mockJsonHelper.Setup(x => x.Parse(It.IsAny<string>())).Returns(JObject.Parse(GetErrorJson()));
            _target.RedirectUri = "test_redirect_uri";
            _target.IsErrorResponse = true;

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(
                () => _target.RequestAccessTokenAsync("test_code"));

            // assert
            VerifyGuardAccessTokenRequest();
            _mockHttpClient.Verify(x => x.SendAsync(_target.AccessTokenRequest, default(CancellationToken)),
                Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetErrorJson()), Times.Once());
            Assert.Equal("Не удалось обменять код авторизации на маркер доступа.", ex.Message);
            Assert.Single(ex.Data);
            Assert.Equal("test_error", (string)ex.Data["error"]);
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Good()
        {
            // arrange
            string accessTokenJson = "{\"access_token\":\"test_access_token\"}";
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(accessTokenJson)
                });
            _mockJsonHelper.Setup(x => x.Parse(It.IsAny<string>())).Returns(JObject.Parse(accessTokenJson));
            _target.RedirectUri = "test_redirect_uri";

            // act
            await _target.RequestAccessTokenAsync("test_code");

            // assert
            VerifyGuardAccessTokenRequest();
            _mockHttpClient.Verify(x => x.SendAsync(_target.AccessTokenRequest, default(CancellationToken)),
                Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(accessTokenJson), Times.Once());
            Assert.Equal("test_access_token", _target.AccessToken);
        }

        [Theory]
        [MemberData(nameof(TestStatusCodes))]
        public async Task RequestUserInfoAsync_Bad_OAuth2ServiceException_ConnectionError(HttpStatusCode statusCode)
        {
            // arrange
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(statusCode));
            _target.RedirectUri = "test_redirect_uri";
            _target.AccessToken = "test_access_token";

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestUserInfoAsync());

            // assert
            VerifyGuardUserInfoRequest();
            _mockHttpClient.Verify(x => x.SendAsync(_target.UserInfoRequest, default(CancellationToken)),
                Times.Once());
            Assert.Equal("Не удалось подключиться к 'test_provider' для получения информации о пользователе.",
                ex.Message);
        }

        private void VerifyGuardUserInfoRequest()
        {
            _mockGuard.Verify(x => x.EnsureStringPropertyIsNotNullOrEmpty("test_access_token",
                "Для выполнения операции необходим маркер доступа (AccessToken)."));
        }

        [Fact]
        public async Task RequestUserInfoAsync_Bad_OAuth2ServiceException_ErrorUserInfoResponse()
        {
            // arrange
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetErrorResponse());
            _mockJsonHelper.Setup(x => x.Parse(It.IsAny<string>())).Returns(JObject.Parse(GetErrorJson()));
            _target.RedirectUri = "test_redirect_uri";
            _target.AccessToken = "test_access_token";
            _target.IsErrorResponse = true;

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestUserInfoAsync());

            // assert
            VerifyGuardUserInfoRequest();
            _mockHttpClient.Verify(x => x.SendAsync(_target.UserInfoRequest, default(CancellationToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetErrorJson()), Times.Once());
            Assert.Equal("Не удалось получить информацию о пользователе.", ex.Message);
            Assert.Single(ex.Data);
            Assert.Equal("test_error", (string)ex.Data["error"]);
        }

        [Fact]
        public async Task RequestUserInfoAsync_Good()
        {
            // arrange
            string userInfoJson = JsonConvert.SerializeObject(new
            {
                id = "test_id",
                name = "test_name",
                email = "test_email",
                picture = "test_url"
            });
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(userInfoJson)
                });
            _mockJsonHelper.Setup(x => x.Parse(It.IsAny<string>())).Returns(JObject.Parse(userInfoJson));
            _target.RedirectUri = "test_redirect_uri";
            _target.AccessToken = "test_access_token";

            // act
            await _target.RequestUserInfoAsync();

            // assert
            VerifyGuardUserInfoRequest();
            _mockHttpClient.Verify(x => x.SendAsync(_target.UserInfoRequest, default(CancellationToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(userInfoJson), Times.Once());
            Assert.Equal("test_id", _target.UserInfo.Id);
            Assert.Equal("test_name", _target.UserInfo.Name);
            Assert.Equal("test_email", _target.UserInfo.Email);
            Assert.Equal("test_access_token", _target.UserInfo.AccessToken);
            Assert.Equal("test_url", _target.UserInfo.Picture);
            Assert.Equal("test_provider", _target.UserInfo.Provider);
        }

        private HttpResponseMessage GetErrorResponse()
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(GetErrorJson())
            };
        }

        private string GetErrorJson() => "{\"error\":\"test_error\"}";
    }
}
