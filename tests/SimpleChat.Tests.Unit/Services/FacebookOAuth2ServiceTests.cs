using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using SimpleChat.Services;
using System;
using System.Collections.Generic;
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
        private FacebookOAuth2Service _target;
        private const string ACCESS_TOKEN_ENDPOINT = "https://graph.facebook.com/v3.3/oauth/access_token";

        public FacebookOAuth2ServiceTests()
        {
            _mockConfiguration.Setup(x => x["Authentication:Facebook:AppId"]).Returns("test_client_id");
            _mockConfiguration.Setup(x => x["Authentication:Facebook:AppSecret"]).Returns("test_client_secret");
            _target = new FacebookOAuth2Service(_mockConfiguration.Object)
            {
                HttpClient = _mockHttpClient.Object,
                RedirectUri = "test_redirect_uri"
            };
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Good()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(request => IsAccessTokenRequest(request)), default(CancellationToken)))
                .ReturnsAsync(GetSuccessAccessTokenResponse("short_access_token"));
            _mockHttpClient.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(request => IsExchangeAccessTokenRequest(request)), default(CancellationToken)))
                .ReturnsAsync(GetSuccessAccessTokenResponse("long_access_token"));

            await _target.RequestAccessTokenAsync("test_code");

            _mockHttpClient.Verify(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)), Times.Exactly(2));
            Assert.Equal("long_access_token", _target.AccessToken);
        }

        private bool IsAccessTokenRequest(HttpRequestMessage request)
        {
            var queryParams = QueryHelpers.ParseNullableQuery(request.RequestUri.Query);
            string requestUri = $"{request.RequestUri.Scheme}://{request.RequestUri.Host}{request.RequestUri.AbsolutePath}";
            return request.Method == HttpMethod.Get
                && requestUri == ACCESS_TOKEN_ENDPOINT
                && queryParams.ContainsKey("client_id") && queryParams["client_id"] == "test_client_id"
                && queryParams.ContainsKey("client_secret") && queryParams["client_secret"] == "test_client_secret"
                && queryParams.ContainsKey("redirect_uri") && queryParams["redirect_uri"] == "test_redirect_uri"
                && queryParams.ContainsKey("code") && queryParams["code"] == "test_code"
                && queryParams.ContainsKey("auth_type") && queryParams["auth_type"] == "rerequest";
        }

        private HttpResponseMessage GetSuccessAccessTokenResponse(string accessToken)
        {
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent($"{{\"access_token\":\"{accessToken}\"}}")
            };
        }

        private bool IsExchangeAccessTokenRequest(HttpRequestMessage request)
        {
            var queryParams = QueryHelpers.ParseNullableQuery(request.RequestUri.Query);
            string requestUri = $"{request.RequestUri.Scheme}://{request.RequestUri.Host}{request.RequestUri.AbsolutePath}";
            return request.Method == HttpMethod.Get
                && requestUri == ACCESS_TOKEN_ENDPOINT
                && queryParams.ContainsKey("client_id") && queryParams["client_id"] == "test_client_id"
                && queryParams.ContainsKey("client_secret") && queryParams["client_secret"] == "test_client_secret"
                && queryParams.ContainsKey("grant_type") && queryParams["grant_type"] == "fb_exchange_token"
                && queryParams.ContainsKey("fb_exchange_token") && queryParams["fb_exchange_token"] == "short_access_token"
                && queryParams.ContainsKey("access_token") && queryParams["access_token"] == "short_access_token";
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Bad_OAuth2ServiceException_ErrorAccessTokenResponse()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(request => IsAccessTokenRequest(request)), default(CancellationToken)))
                .ReturnsAsync(GetErrorResponse());

            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestAccessTokenAsync("test_code"));

            _mockHttpClient.Verify(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)), Times.Once());
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
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(errorResponse))
            };
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Bad_OAuth2ServiceException_ErrorExchangeAccessTokenResponse()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(request => IsAccessTokenRequest(request)), default(CancellationToken)))
                .ReturnsAsync(GetSuccessAccessTokenResponse("short_access_token"));
            _mockHttpClient.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(request => IsExchangeAccessTokenRequest(request)), default(CancellationToken)))
                .ReturnsAsync(GetErrorResponse());

            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestAccessTokenAsync("test_code"));

            _mockHttpClient.Verify(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)), Times.Exactly(2));
            Assert.Equal("Не удалось обменять краткосрочный маркер доступа на долгострочный.", ex.Message);
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
            Assert.Equal("Facebook", _target.UserInfo.Provider);
        }

        private HttpResponseMessage GetSuccessUserInfoResponse()
        {
            var response = new
            {
                id = "test_id",
                name = "test_name",
                email = "test_email",
                picture = new { data = new { url = "test_url" } }
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
                && requestUri == "https://graph.facebook.com/me"
                && queryParams.ContainsKey("fields") && queryParams["fields"] == "id,name,email,picture"
                && queryParams.ContainsKey("access_token") && queryParams["access_token"] == "test_access_token";
        }

        [Fact]
        public async Task RequestUserInfoAsync_Bad_OAuth2ServiceException_ErrorUserInfoResponse()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(GetErrorResponse());
            _target.AccessToken = "test_access_token";

            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestUserInfoAsync());

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
