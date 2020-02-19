using Moq;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Handlers;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using SimpleChat.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Handlers
{
    public class ConfigureOAuth2ServiceTests
    {
        private Mock<OAuth2ServiceValidator> _mockOAuth2ServiceValidator;
        private Mock<AccessTokenClaimTypeValidator> _mockAccessTokenClaimTypeValidator;
        private Mock<UserClaimsValidator> _mockUserClaimsValidator;
        private Mock<IOAuth2Service> _mockOAuth2Service;
        private ContextBuilder _contextBuilder;
        private IContext _testContext;
        private ConfigureOAuth2ServiceForAuthentication _target;

        private IList<Claim> _testUserClaims = new List<Claim>
        {
            new Claim(TestConstants.TestAccessTokeClaimType, TestConstants.TestAccessToken)
        };

        public ConfigureOAuth2ServiceTests()
        {
            _mockOAuth2ServiceValidator = new Mock<OAuth2ServiceValidator>();
            _mockAccessTokenClaimTypeValidator = new Mock<AccessTokenClaimTypeValidator>();
            _mockUserClaimsValidator = new Mock<UserClaimsValidator>();
            _mockOAuth2Service = new Mock<IOAuth2Service>();
            Mock<IGuard> mockGuard = new Mock<IGuard>();
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockOAuth2ServiceValidator.Object,
                "oauth2ServiceValidator")).Returns(_mockOAuth2ServiceValidator.Object);
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockAccessTokenClaimTypeValidator.Object,
                "accessTokenClaimTypeValidator")).Returns(_mockAccessTokenClaimTypeValidator.Object);
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockUserClaimsValidator.Object,
                "userClaimsValidator")).Returns(_mockUserClaimsValidator.Object);
            _contextBuilder = new ContextBuilder();
            _testContext = _contextBuilder
                .WithOAuth2Service(_mockOAuth2Service.Object)
                .WithAccessTokenClaimType(TestConstants.TestAccessTokeClaimType)
                .WithUserClaims(_testUserClaims)
                .Build();
            _target = new ConfigureOAuth2ServiceForAuthentication(
                _mockOAuth2ServiceValidator.Object,
                _mockAccessTokenClaimTypeValidator.Object,
                _mockUserClaimsValidator.Object,
                mockGuard.Object);
        }

        [Fact]
        public void Constructor_Good()
        {
            // arrange
            var mockGuard = new Mock<IGuard>();

            // act
            new ConfigureOAuth2ServiceForAuthentication(
                _mockOAuth2ServiceValidator.Object,
                _mockAccessTokenClaimTypeValidator.Object,
                _mockUserClaimsValidator.Object,
                mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockOAuth2ServiceValidator.Object,
                "oauth2ServiceValidator"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockAccessTokenClaimTypeValidator.Object,
                "accessTokenClaimTypeValidator"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockUserClaimsValidator.Object,
                "userClaimsValidator"), Times.Once());
        }

        [Fact]
        public async Task HandleAsync_Good()
        {
            // arrange
            var mockAuthResult = new Mock<IAuthResult>();
            var mockHandler = new Mock<HandlerBase>();
            mockHandler.Setup(x => x.HandleAsync(It.IsAny<IContext>())).ReturnsAsync(mockAuthResult.Object);
            _mockOAuth2ServiceValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _mockAccessTokenClaimTypeValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _mockUserClaimsValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _target.Next = mockHandler.Object;

            // act
            IAuthResult result = await _target.HandleAsync(_testContext);

            // assert
            _mockOAuth2Service.VerifySet(x => x.AccessToken = TestConstants.TestAccessToken, Times.Once());
            mockHandler.Verify(x => x.HandleAsync(_testContext), Times.Once());
            Assert.Same(mockAuthResult.Object, result);
        }

        [Theory]
        [MemberData(nameof(TestValidationResults))]
        public async Task HandleAsync_Bad_InvalidOperationException(
            bool isOAuth2ServiceValid,
            bool isAccessTokenClaimTypeValid,
            bool isUserClaimsValid)
        {
            // arrange
            string
                oauth2ServiceError = "error_1",
                accessTokenClaimTypeError = "error_2",
                userClaimsError = "error_3";
            StringBuilder expectedMessage = new StringBuilder();
            HandlersTestHelper.SetupMockValidatorWithErrors(_mockOAuth2ServiceValidator.As<IValidator>(), _testContext,
                isOAuth2ServiceValid, oauth2ServiceError, expectedMessage);
            HandlersTestHelper.SetupMockValidatorWithErrors(_mockAccessTokenClaimTypeValidator.As<IValidator>(),
                _testContext, isAccessTokenClaimTypeValid, accessTokenClaimTypeError, expectedMessage);
            HandlersTestHelper.SetupMockValidatorWithErrors(_mockUserClaimsValidator.As<IValidator>(), _testContext,
                isUserClaimsValid, userClaimsError, expectedMessage);

            // act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _target.HandleAsync(_testContext));

            // assert
            Assert.Equal(expectedMessage.ToString(), ex.Message);
        }

        public static IEnumerable<object[]> TestValidationResults()
        {
            return new[]
            {
                new object[] { false, true, true },
                new object[] { true, false, true },
                new object[] { true, true, false },
                new object[] { false, false, true },
                new object[] { true, false, false },
                new object[] { false, true, false },
                new object[] { false, false, false }
            };
        }
    }
}
