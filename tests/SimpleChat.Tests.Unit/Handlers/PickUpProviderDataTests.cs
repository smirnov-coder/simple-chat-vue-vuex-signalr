using Microsoft.AspNetCore.Identity;
using Moq;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Handlers;
using SimpleChat.Controllers.Validators;
using SimpleChat.Extensions;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using SimpleChat.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Handlers
{
    public class PickUpProviderDataTests
    {
        private static Mock<IFacebookOAuth2Service> _mockFacebook = new Mock<IFacebookOAuth2Service>();
        private static Mock<IVKontakteOAuth2Service> _mockVKontakte = new Mock<IVKontakteOAuth2Service>();
        private static Mock<IOdnoklassnikiOAuth2Service> _mockOdnoklassniki = new Mock<IOdnoklassnikiOAuth2Service>();

        private Mock<ProviderValidator> _mockProviderValidator;
        private ContextBuilder _contextBuilder;
        private RequestUserBuilder _requestUserBuilder;
        private PickUpProviderData _target;

        public PickUpProviderDataTests()
        {
            _mockProviderValidator = new Mock<ProviderValidator>();
            var mockGuard = new Mock<IGuard>();
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockFacebook.Object,
                "facebook")).Returns(_mockFacebook.Object);
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockVKontakte.Object,
                "vkontakte")).Returns(_mockVKontakte.Object);
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockOdnoklassniki.Object,
                "odnoklassniki")).Returns(_mockOdnoklassniki.Object);
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockProviderValidator.Object,
                "providerValidator")).Returns(_mockProviderValidator.Object);
            _contextBuilder = new ContextBuilder();
            _requestUserBuilder = new RequestUserBuilder();
            _target = new PickUpProviderData(
                _mockFacebook.Object,
                _mockVKontakte.Object,
                _mockOdnoklassniki.Object,
                _mockProviderValidator.Object,
                mockGuard.Object);
        }

        [Fact]
        public void Constructor_Good()
        {
            // arrange
            var mockGuard = new Mock<IGuard>();

            // act
            new PickUpProviderData(_mockFacebook.Object, _mockVKontakte.Object, _mockOdnoklassniki.Object,
                _mockProviderValidator.Object, mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockFacebook.Object,
                "facebook"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockVKontakte.Object,
                "vkontakte"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockOdnoklassniki.Object,
                "odnoklassniki"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockProviderValidator.Object,
                "providerValidator"), Times.Once());
        }

        [Theory]
        [MemberData(nameof(TestProviderData))]
        public async Task HandleAsync_Good(string provider, string nameClaimType, string avatarClaimType, 
            string accessTokenClaimType, IOAuth2Service oauth2Service)
        {
            // arrange
            IContext testContext = _contextBuilder.WithProvider(provider).Build();
            var mockAuthResult = new Mock<IAuthResult>();
            var mockHandler = new Mock<HandlerBase>();
            mockHandler.Setup(x => x.HandleAsync(It.IsAny<IContext>())).ReturnsAsync(mockAuthResult.Object);
            _mockProviderValidator.Setup(x => x.Validate(testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _target.Next = mockHandler.Object;

            // act
            IAuthResult result = await _target.HandleAsync(testContext);

            // assert
            mockHandler.Verify(x => x.HandleAsync(testContext), Times.Once());
            Assert.Same(mockAuthResult.Object, result);
            Assert.Equal(provider, testContext.Get(ProviderValidator.ContextKey));
            Assert.Equal(nameClaimType, testContext.Get(NameClaimTypeValidator.ContextKey));
            Assert.Equal(avatarClaimType, testContext.Get(AvatarClaimTypeValidator.ContextKey));
            Assert.Equal(accessTokenClaimType, testContext.Get(AccessTokenClaimTypeValidator.ContextKey));
            Assert.Equal(oauth2Service, testContext.Get(OAuth2ServiceValidator.ContextKey));
        }

        public static IEnumerable<object[]> TestProviderData()
        {
            return new[]
            {
                new object[] 
                { 
                    ExternalProvider.Facebook, 
                    CustomClaimTypes.FacebookName, 
                    CustomClaimTypes.FacebookAvatar, 
                    CustomClaimTypes.FacebookAccessToken, 
                    _mockFacebook.Object 
                },
                new object[]
                {
                    ExternalProvider.VKontakte,
                    CustomClaimTypes.VKontakteName,
                    CustomClaimTypes.VKontakteAvatar,
                    CustomClaimTypes.VKontakteAccessToken,
                    _mockVKontakte.Object
                },
                new object[]
                {
                    ExternalProvider.Odnoklassniki,
                    CustomClaimTypes.OdnoklassnikiName,
                    CustomClaimTypes.OdnoklassnikiAvatar,
                    CustomClaimTypes.OdnoklassnikiAccessToken,
                    _mockOdnoklassniki.Object
                },
            };
        }

        [Fact]
        public async Task HandleAsync_Bad_InvalidOperationException()
        {
            // arrange
            string testError = "test_error";
            IContext testContext = _contextBuilder.Build();
            var mockHandler = new Mock<HandlerBase>();
            _mockProviderValidator.Setup(x => x.Validate(testContext, It.IsAny<ICollection<string>>()))
                .Callback(new Action<IContext, ICollection<string>>(
                    (IContext context, ICollection<string> errors) => errors.Add(testError)))
                .Returns(false);
            _target.Next = mockHandler.Object;

            // act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _target.HandleAsync(testContext));

            // assert
            mockHandler.Verify(x => x.HandleAsync(It.IsAny<IContext>()), Times.Never());
            Assert.Equal($"{testError}{Environment.NewLine}", ex.Message);
        }

        [Fact]
        public async Task HandleAsync_Bad_ErrorResult_InvalidProvider()
        {
            // arrange
            IContext testContext = _contextBuilder.WithProvider(TestConstants.TestProvider).Build();
            var mockHandler = new Mock<HandlerBase>();
            _mockProviderValidator.Setup(x => x.Validate(testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _target.Next = mockHandler.Object;

            // act
            IAuthResult authResult = await _target.HandleAsync(testContext);

            // assert
            mockHandler.Verify(x => x.HandleAsync(It.IsAny<IContext>()), Times.Never());
            Assert.IsType<ErrorResult>(authResult);
            var result = authResult as ErrorResult;
            Assert.Equal(TestConstants.ErrorResultType, result.Type);
            Assert.Equal("Неизвестный провайдер. Значение: test_provider.", result.Message);
            Assert.Empty(result.Errors);
        }
    }
}
