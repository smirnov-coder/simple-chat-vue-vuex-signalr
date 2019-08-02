using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using SimpleChat.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Services
{
    public class LinkedInOAuth2ServiceTests
    {
        private Mock<IConfiguration> _mockConfiguration = new Mock<IConfiguration>();
        private Mock<HttpClient> _mockHttpClient = new Mock<HttpClient>();
        private LinkedInOAuth2Service _target;

        public LinkedInOAuth2ServiceTests()
        {
            _mockConfiguration.Setup(x => x["Authentication:LinkedIn:ClientId"]).Returns("test_client_id");
            _mockConfiguration.Setup(x => x["Authentication:LinkedIn:ClientSecret"]).Returns("test_client_secret");
            _target = new LinkedInOAuth2Service(_mockConfiguration.Object)
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
            string requestUri = $"{request.RequestUri.Scheme}://{request.RequestUri.Host}{request.RequestUri.AbsolutePath}";
            string urlEncodedContent =
                "client_id=test_client_id&" +
                "client_secret=test_client_secret&" +
                "redirect_uri=test_redirect_uri&" +
                "code=test_code&" +
                "grant_type=authorization_code";
            return request.Method == HttpMethod.Post
                && requestUri == "https://www.linkedin.com/oauth/v2/accessToken"
                && request.Content.ReadAsStringAsync().Result == urlEncodedContent;
        }

        [Fact]
        public async Task RequestAccessTokenAsync_Bad_OAuth2ServiceException_ErrorAccessTokenResponse()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(GetErrorResponse());

            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestAccessTokenAsync("test_code"));

            _mockHttpClient.Verify(x => x.SendAsync(It.Is<HttpRequestMessage>(request => VerifyAccessTokenRequest(request)), default(CancellationToken)), Times.Once());
            Assert.Equal("Не удалось обменять код авторизации на маркер доступа.", ex.Message);
            Assert.Equal(3, ex.Data.Count);
            Assert.True(ex.Data.Contains("message"));
            Assert.True(ex.Data.Contains("serviceErrorCode"));
            Assert.True(ex.Data.Contains("status"));
            Assert.True(string.IsNullOrWhiteSpace(_target.AccessToken));
        }

        private HttpResponseMessage GetErrorResponse()
        {
            // Пример ошибки OAuth LinkedIn
            // https://docs.microsoft.com/en-us/linkedin/shared/api-guide/concepts/error-handling?context=linkedin/context
            var response = new
            {
                message = "Empty oauth2_access_token",
                serviceErrorCode = 401,
                status = 401
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
            _mockHttpClient.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(request => IsUserInfoRequest(request)), default(CancellationToken)))
                .ReturnsAsync(GetSuccessUserInfoResponse());
            _mockHttpClient.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(request => IsEmailRequest(request)), default(CancellationToken)))
                .ReturnsAsync(GetSuccessEmailResponse());
            _target.AccessToken = "test_access_token";

            await _target.RequestUserInfoAsync();

            _mockHttpClient.Verify(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)), Times.Exactly(2));
            Assert.Equal("test_id", _target.UserInfo.Id);
            Assert.Equal("test_first_name test_last_name", _target.UserInfo.Name);
            Assert.Equal("test_email", _target.UserInfo.Email);
            Assert.Equal("test_url", _target.UserInfo.Picture);
            Assert.Equal("test_access_token", _target.UserInfo.AccessToken);
            Assert.Equal("LinkedIn", _target.UserInfo.Provider);
        }

        private bool IsUserInfoRequest(HttpRequestMessage request)
        {
            var queryParams = QueryHelpers.ParseNullableQuery(request.RequestUri.Query);
            string requestUri = $"{request.RequestUri.Scheme}://{request.RequestUri.Host}{request.RequestUri.AbsolutePath}";
            return request.Method == HttpMethod.Get
                && requestUri == "https://api.linkedin.com/v2/me"
                && request.Headers.Authorization.Equals(AuthenticationHeaderValue.Parse("Bearer test_access_token"))
                && queryParams.ContainsKey("projection")
                && queryParams["projection"] == "(id,localizedFirstName,localizedLastName,profilePicture(displayImage~:playableStreams))";
        }

        private bool IsEmailRequest(HttpRequestMessage request)
        {
            var queryParams = QueryHelpers.ParseNullableQuery(request.RequestUri.Query);
            string requestUri = $"{request.RequestUri.Scheme}://{request.RequestUri.Host}{request.RequestUri.AbsolutePath}";
            return request.Method == HttpMethod.Get
                && requestUri == "https://api.linkedin.com/v2/clientAwareMemberHandles"
                && request.Headers.Authorization.Equals(AuthenticationHeaderValue.Parse("Bearer test_access_token"))
                && queryParams.ContainsKey("q") && queryParams["q"] == "members"
                && queryParams.ContainsKey("projection") && queryParams["projection"] == "(elements*(primary,type,handle~))";
        }

        private HttpResponseMessage GetSuccessUserInfoResponse()
        {
            string json = "{\"id\":\"test_id\"," +
                "\"localizedFirstName\":\"test_first_name\"," +
                "\"localizedLastName\":\"test_last_name\"," +
                "\"profilePicture\":{\"displayImage~\":{\"elements\":[{\"identifiers\":[{\"identifier\":\"test_url\"}]}]}}}";
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            };
        }

        private HttpResponseMessage GetSuccessEmailResponse()
        {
            string json = "{\"elements\":[{\"type\":\"EMAIL\",\"primary\":\"true\",\"handle~\":{\"emailAddress\":\"test_email\"}}]}";
            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            };
        }

        [Fact]
        public async Task RequestUserInfoAsync_Bad_OAuth2ServiceException_ErrorUserInfoResponse()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)))
                .ReturnsAsync(GetErrorResponse());
            _target.AccessToken = "test_access_token";

            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestUserInfoAsync());

            _mockHttpClient.Verify(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)), Times.Once());
            Assert.Equal("Не удалось получить информацию о пользователе.", ex.Message);
            Assert.Equal(3, ex.Data.Count);
            Assert.True(ex.Data.Contains("message"));
            Assert.True(ex.Data.Contains("serviceErrorCode"));
            Assert.True(ex.Data.Contains("status"));
            Assert.Null(_target.UserInfo);
        }

        [Fact]
        public async Task RequestUserInfoAsync_Bad_OAuth2ServiceException_ErrorEmailResponse()
        {
            _mockHttpClient.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(request => IsUserInfoRequest(request)), default(CancellationToken)))
                .ReturnsAsync(GetSuccessUserInfoResponse());
            _mockHttpClient.Setup(x => x.SendAsync(It.Is<HttpRequestMessage>(request => IsEmailRequest(request)), default(CancellationToken)))
                .ReturnsAsync(GetErrorResponse());
            _target.AccessToken = "test_access_token";

            var ex = await Assert.ThrowsAsync<OAuth2ServiceException>(() => _target.RequestUserInfoAsync());

            _mockHttpClient.Verify(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), default(CancellationToken)), Times.Exactly(2));
            Assert.Equal("Не удалось получить адрес электронной почты пользователя.", ex.Message);
            Assert.Equal(3, ex.Data.Count);
            Assert.True(ex.Data.Contains("message"));
            Assert.True(ex.Data.Contains("serviceErrorCode"));
            Assert.True(ex.Data.Contains("status"));
            Assert.Null(_target.UserInfo);
        }
    }
}
