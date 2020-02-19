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
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Services
{
    public class OdnoklassnikiOAuth2ServiceTests
    {
        private Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private Mock<HttpClient> _mockHttpClient = new Mock<HttpClient>();
        private Mock<IGuard> _mockGuard = new Mock<IGuard>();
        private Mock<IJsonHelper> _mockJsonHelper = new Mock<IJsonHelper>();
        private Mock<IUriHelper> _mockUriHelper = new Mock<IUriHelper>();
        private Mock<IMD5Hasher> _mockHasher = new Mock<IMD5Hasher>();
        private const string AccessTokenEndpoint = "https://api.ok.ru/oauth/token.do";
        private const string UserInfoEndpoint = "https://api.ok.ru/api/users/getCurrentUser";
        private OdnoklassnikiOAuth2Service _target;

        public OdnoklassnikiOAuth2ServiceTests()
        {
            _mockConfiguration.Setup(x => x["Authentication:Odnoklassniki:ApplicationId"]).Returns("test_client_id");
            _mockConfiguration.Setup(x => x["Authentication:Odnoklassniki:ApplicationSecretKey"])
                .Returns("test_client_secret");
            _mockConfiguration.Setup(x => x["Authentication:Odnoklassniki:ApplicationKey"]).Returns("test_public_key");
            _mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockConfiguration.Object, "configuration"))
                .Returns(_mockConfiguration.Object);
            _mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockHttpClient.Object, "HttpClient"))
                .Returns(_mockHttpClient.Object);
            _mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockJsonHelper.Object, "jsonHelper"))
                .Returns(_mockJsonHelper.Object);
            _mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockUriHelper.Object, "uriHelper"))
                .Returns(_mockUriHelper.Object);
            _mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockHasher.Object, "MD5Hasher"))
                .Returns(_mockHasher.Object);
            _mockGuard.Setup(x => x.EnsureStringParamIsNotNullOrEmpty(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((value, paramName) => value);
            _target = new OdnoklassnikiOAuth2Service(_mockConfiguration.Object, _mockUriHelper.Object,
                _mockJsonHelper.Object, _mockGuard.Object)
            {
                HttpClient = _mockHttpClient.Object,
                MD5Hasher = _mockHasher.Object,
                RedirectUri = "test_redirect_uri"
            };
        }

        [Fact]
        public void Constructor_Good()
        {
            // arrange
            string
                clientIdKey = "Authentication:Odnoklassniki:ApplicationId",
                clientSecretKey = "Authentication:Odnoklassniki:ApplicationSecretKey",
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
            var target = new OdnoklassnikiOAuth2Service(mockConfiguration.Object, _mockUriHelper.Object,
                _mockJsonHelper.Object, mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientIdKey, "clientIdKey"), Times.Once());
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientSecretKey, "clientSecretKey"), Times.Once);
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty("Одноклассники", "provider"), Times.Once());
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientId, "ClientId"), Times.Once());
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(clientSecret, "ClientSecret"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(mockConfiguration.Object, "configuration"), Times.Once);
            mockConfiguration.Verify(x => x[clientIdKey], Times.Once());
            mockConfiguration.Verify(x => x[clientSecretKey], Times.Once());
            mockConfiguration.Verify(x => x["Authentication:Odnoklassniki:ApplicationKey"], Times.Once());
            Assert.NotNull(target.HttpClient);
            Assert.NotNull(target.MD5Hasher);
        }

        [Fact]
        public void MD5Hasher_Good()
        {
            // arrange
            IMD5Hasher testHasher = new MD5Hasher();

            // act
            _target.MD5Hasher = testHasher;

            // assert
            _mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(testHasher, "MD5Hasher"), Times.Once());
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Good()
        {
            // arrange
            _mockUriHelper.Setup(x => x.AddQueryString(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(GetAccessTokenRequestUri());
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetAccessTokenResponse());
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
                "client_id=test_client_id&" +
                "client_secret=test_client_secret&" +
                "redirect_uri=test_redirect_uri&" +
                "code=test_code&" +
                "grant_type=authorization_code";
        }

        private HttpResponseMessage GetAccessTokenResponse()
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(GetAccessTokenJson())
            };
        }

        private string GetAccessTokenJson() => "{\"access_token\":\"test_access_token\"}";

        private bool VerifyAccessTokenRequestParams(IDictionary<string, string> queryParams)
        {
            return queryParams.ContainsKey("client_id") && queryParams["client_id"] == "test_client_id"
                && queryParams.ContainsKey("client_secret") && queryParams["client_secret"] == "test_client_secret"
                && queryParams.ContainsKey("redirect_uri") && queryParams["redirect_uri"] == "test_redirect_uri"
                && queryParams.ContainsKey("code") && queryParams["code"] == "test_code"
                && queryParams.ContainsKey("grant_type") && queryParams["grant_type"] == "authorization_code";
        }

        private bool VerifyAccessTokenRequest(HttpRequestMessage request)
        {
            var queryParams = QueryHelpers.ParseNullableQuery(request.RequestUri.Query);
            string requestUri = $"{request.RequestUri.Scheme}://{request.RequestUri.Host}{request.RequestUri.AbsolutePath}";
            return request.Method == HttpMethod.Post
                && requestUri == AccessTokenEndpoint
                && VerifyAccessTokenRequestParams(queryParams.ToDictionary(entry => entry.Key, entry => (string)entry.Value));
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
            Assert.Equal($"Не удалось подключиться к 'Одноклассники' для обмена кода авторизации на маркер доступа.",
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
                .ReturnsAsync(GetErrorAccessTokenResponse());
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

        private HttpResponseMessage GetErrorAccessTokenResponse()
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(GetErrorAccessTokenJson())
            };
        }

        private string GetErrorAccessTokenJson()
        {
            // Пример ошибки OAuth Одноклассники
            // https://apiok.ru/ext/oauth/server
            var response = new
            {
                error = "invalid_request",
                error_description = "Expired code"
            };
            return JsonConvert.SerializeObject(response);
        }

        [Fact]
        public async Task RequestUserInfoAsync_Good()
        {
            // arrange
            _mockHasher.SetupSequence(x => x.ComputeHash(It.IsAny<string>()))
                .Returns("test_session_secret_key")
                .Returns("test_signature");
            _mockUriHelper.Setup(x => x.AddQueryString(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(GetUserInfoRequestUri());
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetSuccessUserInfoResponse());
            _mockJsonHelper.Setup(x => x.Parse(It.IsAny<string>())).Returns(JObject.Parse(GetUserInfoJson()));
            _target.AccessToken = "test_access_token";

            // act
            await _target.RequestUserInfoAsync();

            // assert
            _mockHasher.Verify(x => x.ComputeHash("test_access_token" + "test_client_secret"), Times.Once());
            _mockHasher.Verify(x => x.ComputeHash(GetSignatureSource()), Times.Once());
            _mockUriHelper.Verify(x => x.AddQueryString(UserInfoEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyUserInfoRequestParams(queryParams))), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request => 
                VerifyUserInfoRequest(request)), default(CancellationToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetUserInfoJson()), Times.Once());
            Assert.Equal("test_id", _target.UserInfo.Id);
            Assert.Equal("test_name", _target.UserInfo.Name);
            Assert.Equal("test_email", _target.UserInfo.Email);
            Assert.Equal("test_url", _target.UserInfo.Picture);
            Assert.Equal("test_access_token", _target.UserInfo.AccessToken);
            Assert.Equal("Одноклассники", _target.UserInfo.Provider);
        }

        private string GetUserInfoRequestUri()
        {
            return $"{UserInfoEndpoint}?"
                + "application_key=test_public_key&"
                + "fields=uid,name,email,pic50x50&"
                + "format=json&"
                + "access_token=test_access_token&"
                + "sig=test_signature";
        }

        private HttpResponseMessage GetSuccessUserInfoResponse()
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
                uid = "test_id",
                name = "test_name",
                email = "test_email",
                pic50x50 = "test_url"
            };
            return JsonConvert.SerializeObject(response);
        }

        private string GetSignatureSource()
        {
            return "application_key=test_public_key"
                + "fields=uid,name,email,pic50x50"
                + "format=json"
                + "test_session_secret_key";
        }

        private bool VerifyUserInfoRequestParams(IDictionary<string, string> queryParams)
        {
            return queryParams.ContainsKey("fields") && queryParams["fields"] == "uid,name,email,pic50x50"
                && queryParams.ContainsKey("access_token") && queryParams["access_token"] == "test_access_token"
                && queryParams.ContainsKey("application_key") && queryParams["application_key"] == "test_public_key"
                && queryParams.ContainsKey("format") && queryParams["format"] == "json"
                && queryParams.ContainsKey("sig") && queryParams["sig"] == "test_signature";
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
            _mockHasher.SetupSequence(x => x.ComputeHash(It.IsAny<string>()))
                .Returns("test_session_secret_key")
                .Returns("test_signature");
            _mockUriHelper.Setup(x => x.AddQueryString(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(GetUserInfoRequestUri());
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));
            _target.AccessToken = "test_access_token";

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestUserInfoAsync());

            // assert
            _mockHasher.Verify(x => x.ComputeHash("test_access_token" + "test_client_secret"), Times.Once());
            _mockHasher.Verify(x => x.ComputeHash(GetSignatureSource()), Times.Once());
            _mockUriHelper.Verify(x => x.AddQueryString(UserInfoEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyUserInfoRequestParams(queryParams))), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request =>
                VerifyUserInfoRequest(request)), default(CancellationToken)), Times.Once());
            Assert.Equal($"Не удалось подключиться к 'Одноклассники' для получения информации о пользователе.",
                ex.Message);
            Assert.Null(_target.UserInfo);
        }

        [Fact]
        public async Task RequestUserInfoAsync_Bad_OAuth2ServiceException_ErrorUserInfoResponse()
        {
            // arrange
            _mockHasher.SetupSequence(x => x.ComputeHash(It.IsAny<string>()))
                .Returns("test_session_secret_key")
                .Returns("test_signature");
            _mockUriHelper.Setup(x => x.AddQueryString(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(GetUserInfoRequestUri());
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetErrorUserInfoResponse());
            _mockJsonHelper.Setup(x => x.Parse(It.IsAny<string>())).Returns(JObject.Parse(GetErrorUserInfoJson()));
            _target.AccessToken = "test_access_token";

            // act
            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestUserInfoAsync());

            // assert
            _mockHasher.Verify(x => x.ComputeHash("test_access_token" + "test_client_secret"), Times.Once());
            _mockHasher.Verify(x => x.ComputeHash(GetSignatureSource()), Times.Once());
            _mockUriHelper.Verify(x => x.AddQueryString(UserInfoEndpoint, It.Is<IDictionary<string, string>>(
                queryParams => VerifyUserInfoRequestParams(queryParams))), Times.Once());
            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request =>
                VerifyUserInfoRequest(request)), default(CancellationToken)), Times.Once());
            _mockJsonHelper.Verify(x => x.Parse(GetErrorUserInfoJson()), Times.Once());
            Assert.Equal("Не удалось получить информацию о пользователе.", ex.Message);
            Assert.Equal(4, ex.Data.Count);
            Assert.True(ex.Data.Contains("error_code"));
            Assert.True(ex.Data.Contains("error_msg"));
            Assert.True(ex.Data.Contains("error_data"));
            Assert.True(ex.Data.Contains("error_field"));
            Assert.Null(_target.UserInfo);
        }

        private HttpResponseMessage GetErrorUserInfoResponse()
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(GetErrorUserInfoJson())
            };
        }

        private string GetErrorUserInfoJson()
        {
            // https://apiok.ru/dev/errors
            var response = new
            {
                error_code = 100,
                error_msg = "PARAM : Either session_key or uid must be specified",
                error_data = "1",
                error_field = "uid"
            };
            return JsonConvert.SerializeObject(response);
        }
    }
}
