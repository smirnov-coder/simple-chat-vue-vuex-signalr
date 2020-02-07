using Microsoft.AspNetCore.Identity;
using Moq;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Handlers;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Handlers
{
    public class CreateAuthenticatedResultTests
    {
        private Mock<IdentityUserValidator> _mockIdentityUserValidator;
        private Mock<UserInfoValidator> _mockUserInfoValidator;
        private Mock<ProviderValidator> _mockProviderValidator;
        private ContextBuilder _contextBuilder;
        private IContext _testContext;
        private CreateAuthenticatedResult _target;

        public CreateAuthenticatedResultTests()
        {
            _mockIdentityUserValidator = new Mock<IdentityUserValidator>();
            _mockUserInfoValidator = new Mock<UserInfoValidator>();
            _mockProviderValidator = new Mock<ProviderValidator>();
            var mockGuard = new Mock<IGuard>();
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockIdentityUserValidator.Object,
                "identityUserValidator")).Returns(_mockIdentityUserValidator.Object);
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockUserInfoValidator.Object,
                "userInfoValidator")).Returns(_mockUserInfoValidator.Object);
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockProviderValidator.Object,
                "providerValidator")).Returns(_mockProviderValidator.Object);
            _contextBuilder = new ContextBuilder();
            _testContext = _contextBuilder
                .WithIdentityUser(new IdentityUser { Id = TestConstants.TestId })
                .WithExternalUserInfo(new ExternalUserInfo
                {
                    Name = TestConstants.TestName,
                    Picture = TestConstants.TestUrl
                })
                .WithProvider(TestConstants.TestProvider)
                .Build();

            _target = new CreateAuthenticatedResult(
                _mockIdentityUserValidator.Object,
                _mockUserInfoValidator.Object,
                _mockProviderValidator.Object,
                mockGuard.Object);
        }

        [Fact]
        public void Constructor_Good()
        {
            // arrange
            var mockGuard = new Mock<IGuard>();

            // act
            new CreateAuthenticatedResult(
                _mockIdentityUserValidator.Object,
                _mockUserInfoValidator.Object,
                _mockProviderValidator.Object,
                mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockIdentityUserValidator.Object,
                "identityUserValidator"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockUserInfoValidator.Object,
                "userInfoValidator"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockProviderValidator.Object,
                "providerValidator"), Times.Once());
        }

        [Fact]
        public async Task HandleAsync_Good()
        {
            // arrange
            var mockHandler = new Mock<IHandler>();
            _mockIdentityUserValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _mockUserInfoValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _mockProviderValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _target.Next = mockHandler.Object;

            // act
            IAuthResult authResult = await _target.HandleAsync(_testContext);

            // assert
            mockHandler.Verify(x => x.HandleAsync(It.IsAny<IContext>()), Times.Never());
            Assert.IsType<AuthenticatedResult>(authResult);
            var result = authResult as AuthenticatedResult;
            Assert.Equal("auth_check", result.Type);
            Assert.True(result.IsAuthenticated);
            Assert.Equal(TestConstants.TestId, result.User.Id);
            Assert.Equal(TestConstants.TestName, result.User.Name);
            Assert.Equal(TestConstants.TestUrl, result.User.Avatar);
            Assert.Equal(TestConstants.TestProvider, result.User.Provider);
        }

        [Theory]
        [MemberData(nameof(TestValidationResults))]
        public async Task HandleAsync_Bad_InvalidOperationException(
            bool isIdentityUserValid,
            bool isUserInfoValid,
            bool isProviderValid)
        {
            // arrange
            string
                identityUserError = "error_1",
                userInfoError = "error_2",
                providerError = "error_3";
            StringBuilder expectedMessage = new StringBuilder();
            var anyCollection = It.Ref<ICollection<string>>.IsAny;
            HandlersTestHelper.SetupMockValidatorWithErrors(_mockIdentityUserValidator.As<IValidator>(), _testContext,
                isIdentityUserValid, identityUserError, expectedMessage);
            HandlersTestHelper.SetupMockValidatorWithErrors(_mockUserInfoValidator.As<IValidator>(), _testContext,
                isUserInfoValid, userInfoError, expectedMessage);
            HandlersTestHelper.SetupMockValidatorWithErrors(_mockProviderValidator.As<IValidator>(), _testContext,
                isProviderValid, providerError, expectedMessage);

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
