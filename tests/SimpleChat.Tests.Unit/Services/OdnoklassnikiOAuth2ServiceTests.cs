using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using SimpleChat.Infrastructure.Extensions;
using SimpleChat.Services;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Services
{
    public class OdnoklassnikiOAuth2ServiceTests
    {
        private Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private Mock<HttpClient> _mockHttpClient = new Mock<HttpClient>();
        private OdnoklassnikiOAuth2Service _target;

        public OdnoklassnikiOAuth2ServiceTests()
        {
            _mockConfiguration.Setup(x => x["Authentication:Odnoklassniki:ApplicationId"]).Returns("test_client_id");
            _mockConfiguration.Setup(x => x["Authentication:Odnoklassniki:ApplicationSecretKey"]).Returns("test_client_secret");
            _mockConfiguration.Setup(x => x["Authentication:Odnoklassniki:ApplicationKey"]).Returns("test_public_key");
            _target = new OdnoklassnikiOAuth2Service(_mockConfiguration.Object)
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
            return request.Method == HttpMethod.Post
                && requestUri == "https://api.ok.ru/oauth/token.do"
                && queryParams.ContainsKey("client_id") && queryParams["client_id"] == "test_client_id"
                && queryParams.ContainsKey("client_secret") && queryParams["client_secret"] == "test_client_secret"
                && queryParams.ContainsKey("redirect_uri") && queryParams["redirect_uri"] == "test_redirect_uri"
                && queryParams.ContainsKey("code") && queryParams["code"] == "test_code"
                && queryParams.ContainsKey("grant_type") && queryParams["grant_type"] == "authorization_code";
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
            // Пример ошибки OAuth Одноклассники
            // https://apiok.ru/ext/oauth/server
            var response = new
            {
                error = "invalid_request",
                error_description = "Expired code"
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
            Assert.Equal("test_name", _target.UserInfo.Name);
            Assert.Equal("test_email", _target.UserInfo.Email);
            Assert.Equal("test_url", _target.UserInfo.Picture);
            Assert.Equal("test_access_token", _target.UserInfo.AccessToken);
            Assert.Equal("Одноклассники", _target.UserInfo.Provider);
        }

        private HttpResponseMessage GetSuccessUserInfoResponse()
        {
            var response = new
            {
                uid = "test_id",
                name = "test_name",
                email = "test_email",
                pic50x50 = "test_url"
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
                && requestUri == "https://api.ok.ru/api/users/getCurrentUser"
                && queryParams.ContainsKey("fields") && queryParams["fields"] == "uid,name,email,pic50x50"
                && queryParams.ContainsKey("access_token") && queryParams["access_token"] == "test_access_token"
                && queryParams.ContainsKey("application_key") && queryParams["application_key"] == "test_public_key"
                && queryParams.ContainsKey("format") && queryParams["format"] == "json"
                && queryParams.ContainsKey("sig") && VerifyRequestSignature(queryParams["sig"]);
        }

        private bool VerifyRequestSignature(string signature)
        {
            using (var md5 = MD5.Create())
            {
                string sessionSecretKey = md5.ComputeHash("test_access_token" + "test_client_secret");
                string signatureSource = ""
                    + $"application_key=test_public_key"
                    + $"fields=uid,name,email,pic50x50"
                    + $"format=json"
                    + $"{sessionSecretKey}";
                string hash = md5.ComputeHash(signatureSource);
                return hash == signature;
            }
        }

        [Fact]
        public async Task RequestUserInfoAsync_Bad_OAuth2ServiceException_ErrorUserInfoResponse()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(GetErrorUserInfoResponse());
            _target.AccessToken = "test_access_token";

            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestUserInfoAsync());

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
            // https://apiok.ru/dev/errors
            var response = new
            {
                error_code = 100,
                error_msg = "PARAM : Either session_key or uid must be specified",
                error_data = "1",
                error_field = "uid"
            };
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(response))
            };
        }
    }
}
