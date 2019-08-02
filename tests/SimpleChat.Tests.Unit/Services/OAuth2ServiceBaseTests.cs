using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public OAuth2ServiceBaseUnderTest(IConfiguration configuration)
            : base(configuration, "test:client_id", "test:client_secret", "test_provider")
        { }

        public OAuth2ServiceBaseUnderTest(IConfiguration configuration, string clientIdKey, string clientSecretKey, string providerName)
            : base(configuration, clientIdKey, clientSecretKey, providerName)
        { }

        protected override void CollectErrorData(IDictionary data, JObject dataSource)
        {
            if (dataSource.ContainsKey("error"))
                data.Add("error", (string)dataSource["error"]);
        }

        protected override HttpRequestMessage CreateAccessTokenRequest(string code) => AccessTokenRequest;

        public HttpRequestMessage AccessTokenRequest { get; set; } = new HttpRequestMessage(HttpMethod.Get, $"www.example.com?code=test_code");

        protected override HttpRequestMessage CreateUserInfoRequest() => UserInfoRequest;

        public HttpRequestMessage UserInfoRequest { get; set; } = new HttpRequestMessage(HttpMethod.Get, "www.example.com?access_token=test_access_token");

        protected override Task HandleUserInfoResponseAsync(JObject userInfoResponse)
        {
            UserInfo = new ExternalUserInfo
            {
                Id = (string)userInfoResponse["id"],
                Name = (string)userInfoResponse["name"],
                Email = (string)userInfoResponse["email"],
                AccessToken = AccessToken,
                Picture = (string)userInfoResponse["picture"],
                Provider = _providerName
            };
            return Task.CompletedTask;
        }

        protected override bool IsErrorAccessTokenResponse(JObject parsedResponse)
        {
            return parsedResponse.ContainsKey("error");
        }
    }

    // Tests
    public class OAuth2ServiceBaseTests
    {
        private Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private Mock<HttpClient> _mockHttpClient = new Mock<HttpClient>();
        private OAuth2ServiceBaseUnderTest _target;

        public OAuth2ServiceBaseTests()
        {
            _mockConfiguration.Setup(x => x["test:client_id"]).Returns("test_client_id");
            _mockConfiguration.Setup(x => x["test:client_secret"]).Returns("test_client_secret");
            _target = new OAuth2ServiceBaseUnderTest(_mockConfiguration.Object)
            {
                HttpClient = _mockHttpClient.Object
            };
        }

        [Theory]
        [MemberData(nameof(GetTestStringValues))]
        public void Constructor_Bad_ArgumentException(string clientIdKey, string clientSecretKey, string providerName)
        {
            Assert.Throws<ArgumentException>(() => _target = new OAuth2ServiceBaseUnderTest(_mockConfiguration.Object,
                clientIdKey, clientSecretKey, providerName));
        }

        public static IEnumerable<object[]> GetTestStringValues()
        {
            return new List<object[]>
            {
                new object[] { null, "a", "a" },
                new object[] { "a", null, "a" },
                new object[] { "", "a", null },
                new object[] { "", "a", "a" },
                new object[] { "a", "", "a" },
                new object[] { "", "a", "" },
                new object[] { " ", "a", "a" },
                new object[] { "a", " ", "a" },
                new object[] { "", "a", " " },
            };
        }

        [Fact]
        public void Constructor_Bad_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _target = new OAuth2ServiceBaseUnderTest(null));
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public void RedirectUri_Bad_ArgumentException(string redirectUri)
        {
            Assert.Throws<ArgumentException>(() => _target.RedirectUri = redirectUri);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public void AccessToken_Bad_ArgumentException(string accessToken)
        {
            Assert.Throws<ArgumentException>(() => _target.AccessToken = accessToken);
        }

        [Fact]
        public void HttpClient_Bad_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _target.HttpClient = null);
        }

        [Theory]
        [InlineData(null), InlineData(""), InlineData(" ")]
        public async Task RequestAccessTokenAsync_Bad_ArgumentException(string code)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _target.RequestAccessTokenAsync(code));
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Bad_InvalidOperationException()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _target.RequestAccessTokenAsync("test_code"));

            Assert.Equal("Не задан адрес обратного вызова (RedirectUri) для OAuth-службы 'test_provider'.", ex.Message);
        }

        [Theory]
        [MemberData(nameof(GetTestStatusCodes))]
        public async Task RequestAccessTokenAsync_Bad_OAuth2ServiceException_ConnectionError(HttpStatusCode statusCode)
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(statusCode));
            _target.RedirectUri = "test_redirect_uri";

            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestAccessTokenAsync("test_code"));

            Assert.Equal("Не удалось подключиться к 'test_provider' для обмена кода авторизации на маркер доступа.", ex.Message);
        }

        public static IEnumerable<object[]> GetTestStatusCodes()
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
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetTestErrorResponse());
            _target.RedirectUri = "test_redirect_uri";

            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestAccessTokenAsync("test_code"));

            Assert.Equal("Не удалось обменять код авторизации на маркер доступа.", ex.Message);
            Assert.Single(ex.Data);
            Assert.Equal("test_error", (string)ex.Data["error"]);
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Good()
        {
            _mockHttpClient.Setup(x => x.SendAsync(_target.AccessTokenRequest, default(CancellationToken)))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"access_token\":\"test_access_token\"}")
                });
            _target.RedirectUri = "test_redirect_uri";

            await _target.RequestAccessTokenAsync("test_code");

            Assert.Equal("test_access_token", _target.AccessToken);
        }

        [Fact]
        public async Task RequestUserInfoAsync_Bad_InvalidOperationException()
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _target.RequestUserInfoAsync());

            Assert.Equal("Для выполнения операции необходим маркер доступа (AccessToken).", ex.Message);
        }

        [Theory]
        [MemberData(nameof(GetTestStatusCodes))]
        public async Task RequestUserInfoAsync_Bad_OAuth2ServiceException_ConnectionError(HttpStatusCode statusCode)
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(statusCode));
            _target.RedirectUri = "test_redirect_uri";
            _target.AccessToken = "test_access_token";

            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestUserInfoAsync());

            Assert.Equal("Не удалось подключиться к 'test_provider' для получения информации о пользователе.", ex.Message);
        }

        [Fact]
        public async Task RequestUserInfoAsync_Bad_OAuth2ServiceException_ErrorUserInfoResponse()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetTestErrorResponse());
            _target.RedirectUri = "test_redirect_uri";
            _target.AccessToken = "test_access_token";

            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestUserInfoAsync());

            Assert.Equal("Не удалось получить информацию о пользователе.", ex.Message);
            Assert.Single(ex.Data);
            Assert.Equal("test_error", (string)ex.Data["error"]);
        }

        [Fact]
        public async Task RequestUserInfoAsync_Good()
        {
            _mockHttpClient.Setup(x => x.SendAsync(_target.UserInfoRequest, default(CancellationToken)))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(GetTestUserInfoJson())
                });
            _target.RedirectUri = "test_redirect_uri";
            _target.AccessToken = "test_access_token";

            await _target.RequestUserInfoAsync();

            Assert.Equal("test_id", _target.UserInfo.Id);
            Assert.Equal("test_name", _target.UserInfo.Name);
            Assert.Equal("test_email", _target.UserInfo.Email);
            Assert.Equal("test_access_token", _target.UserInfo.AccessToken);
            Assert.Equal("test_picture", _target.UserInfo.Picture);
            Assert.Equal("test_provider", _target.UserInfo.Provider);
        }

        private string GetTestUserInfoJson()
        {
            var userInfo = new
            {
                id = "test_id",
                name = "test_name",
                email = "test_email",
                picture = "test_picture"
            };
            return JsonConvert.SerializeObject(userInfo);
        }

        private HttpResponseMessage GetTestErrorResponse()
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"error\":\"test_error\"}")
            };
        }
    }
}
