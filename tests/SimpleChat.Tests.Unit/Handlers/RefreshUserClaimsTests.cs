using Microsoft.AspNetCore.Identity;
using Moq;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Handlers;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Handlers
{
    public class RefreshUserClaimsTests
    {
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<IdentityUserValidator> _mockIdentityUserValidator;
        private Mock<UserClaimsValidator> _mockUserClaimsValidator;
        private Mock<UserInfoValidator> _mockUserInfoValidator;
        private Mock<NameClaimTypeValidator> _mockNameClaimTypeValidator;
        private Mock<AvatarClaimTypeValidator> _mockAvatarClaimTypeValidator;
        private ContextBuilder _contextBuilder;
        private IContext _testContext;
        private RefreshUserClaims _target;

        public RefreshUserClaimsTests()
        {
            _mockUserManager = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            _mockIdentityUserValidator = new Mock<IdentityUserValidator>();
            _mockUserClaimsValidator = new Mock<UserClaimsValidator>();
            _mockUserInfoValidator = new Mock<UserInfoValidator>();
            _mockNameClaimTypeValidator = new Mock<NameClaimTypeValidator>();
            _mockAvatarClaimTypeValidator = new Mock<AvatarClaimTypeValidator>();
            var mockGuard = new Mock<IGuard>();
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockUserManager.Object,
                "userManager")).Returns(_mockUserManager.Object);
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockIdentityUserValidator.Object,
                "identityUserValidator")).Returns(_mockIdentityUserValidator.Object);
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockUserClaimsValidator.Object,
                "userClaimsValidator")).Returns(_mockUserClaimsValidator.Object);
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockUserInfoValidator.Object,
                "userInfoValidator")).Returns(_mockUserInfoValidator.Object);
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockNameClaimTypeValidator.Object,
                "nameClaimTypeValidator")).Returns(_mockNameClaimTypeValidator.Object);
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockAvatarClaimTypeValidator.Object,
                "avatarClaimTypeValidator")).Returns(_mockAvatarClaimTypeValidator.Object);
            _contextBuilder = new ContextBuilder();
            _testContext = _contextBuilder
                .WithIdentityUser(new IdentityUser())
                .WithUserClaims(new List<Claim>
                {
                    new Claim(TestConstants.TestNameClaimType, TestConstants.TestName),
                    new Claim(TestConstants.TestAvatarClaimType, TestConstants.TestUrl)
                })
                .WithExternalUserInfo(new ExternalUserInfo
                {
                    Name = TestConstants.NewName,
                    Picture = TestConstants.NewUrl
                })
                .WithNameClaimType(TestConstants.TestNameClaimType)
                .WithAvatarClaimType(TestConstants.TestAvatarClaimType)
                .Build();
            _target = new RefreshUserClaims(
                _mockUserManager.Object,
                _mockIdentityUserValidator.Object,
                _mockUserClaimsValidator.Object,
                _mockUserInfoValidator.Object,
                _mockNameClaimTypeValidator.Object,
                _mockAvatarClaimTypeValidator.Object,
                mockGuard.Object);
        }

        [Fact]
        public void Constructor_Good()
        {
            // arrange
            var mockGuard = new Mock<IGuard>();

            // act
            new RefreshUserClaims(
                _mockUserManager.Object,
                _mockIdentityUserValidator.Object,
                _mockUserClaimsValidator.Object,
                _mockUserInfoValidator.Object,
                _mockNameClaimTypeValidator.Object,
                _mockAvatarClaimTypeValidator.Object,
                mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockUserManager.Object,
                "userManager"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockIdentityUserValidator.Object,
                "identityUserValidator"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockUserClaimsValidator.Object,
                "userClaimsValidator"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockUserInfoValidator.Object,
                "userInfoValidator"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockNameClaimTypeValidator.Object,
                "nameClaimTypeValidator"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockAvatarClaimTypeValidator.Object,
                "avatarClaimTypeValidator"), Times.Once());
        }

        [Fact]
        public async Task HandleAsync_Good()
        {
            // arrange 
            var testIdentityUser = _testContext.Get(IdentityUserValidator.ContextKey) as IdentityUser;
            var mockAuthResult = new Mock<IAuthResult>();
            var mockHandler = new Mock<HandlerBase>();
            mockHandler.Setup(x => x.HandleAsync(It.IsAny<IContext>())).ReturnsAsync(mockAuthResult.Object);
            _mockIdentityUserValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _mockUserClaimsValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _mockUserInfoValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _mockNameClaimTypeValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _mockAvatarClaimTypeValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _target.Next = mockHandler.Object;

            // act
            IAuthResult result = await _target.HandleAsync(_testContext);

            // assert
            _mockUserManager.Verify(x => x.ReplaceClaimAsync(testIdentityUser, 
                It.Is<Claim>(claim => claim.Type == TestConstants.TestNameClaimType),
                It.Is<Claim>(claim => claim.Type == TestConstants.TestNameClaimType
                    && claim.Value == TestConstants.NewName)), Times.Once());
            _mockUserManager.Verify(x => x.ReplaceClaimAsync(testIdentityUser,
                It.Is<Claim>(claim => claim.Type == TestConstants.TestAvatarClaimType),
                It.Is<Claim>(claim => claim.Type == TestConstants.TestAvatarClaimType
                    && claim.Value == TestConstants.NewUrl)), Times.Once());
            mockHandler.Verify(x => x.HandleAsync(_testContext), Times.Once());
            Assert.Same(mockAuthResult.Object, result);
        }

        [Theory]
        [MemberData(nameof(TestValidationResults))]
        public async Task HandleAsync_Bad_InvalidOperationException(
            bool isIdentityUserValid,
            bool isUserClaimsValid,
            bool isUserInfoValid,
            bool isNameClaimTypeValid,
            bool isAvatarClaimTypeValid)
        {
            // arrange
            string
                identityUserError = "error_1",
                userClaimsError = "error_2",
                userInfoError = "error_3",
                nameClaimTypeError = "error_4",
                avatarClaimTypeError = "error_5";
            var expectedMessage = new StringBuilder();
            HandlersTestHelper.SetupMockValidatorWithErrors(_mockIdentityUserValidator.As<IValidator>(), _testContext,
                isIdentityUserValid, identityUserError, expectedMessage);
            HandlersTestHelper.SetupMockValidatorWithErrors(_mockUserClaimsValidator.As<IValidator>(), _testContext,
                isUserClaimsValid, userClaimsError, expectedMessage);
            HandlersTestHelper.SetupMockValidatorWithErrors(_mockUserInfoValidator.As<IValidator>(), _testContext,
                isUserInfoValid, userInfoError, expectedMessage);
            HandlersTestHelper.SetupMockValidatorWithErrors(_mockNameClaimTypeValidator.As<IValidator>(), _testContext,
                isNameClaimTypeValid, nameClaimTypeError, expectedMessage);
            HandlersTestHelper.SetupMockValidatorWithErrors(_mockAvatarClaimTypeValidator.As<IValidator>(),
                _testContext, isAvatarClaimTypeValid, avatarClaimTypeError, expectedMessage);

            // act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _target.HandleAsync(_testContext));

            // assert
            Assert.Equal(expectedMessage.ToString(), ex.Message);
        }

        /// TODO: должно быть 2^5
        public static IEnumerable<object[]> TestValidationResults()
        {
            return new[]
            {
                new object[] { false, true, true, true, true },
                new object[] { true, false, true, true, true },
                new object[] { true, true, false, true, true },
                new object[] { true, true, true, false, true },
                new object[] { true, true, true, true, false },
                new object[] { false, false, true, true, true },
                new object[] { true, false, false, true, true },
                new object[] { true, true, false, false, true },
                new object[] { true, true, true, false, false },
                new object[] { false, true, true, true, false },
                new object[] { false, false, false, true, true },
                new object[] { true, false, false, false, true },
                new object[] { true, true, false, false, false },
                new object[] { false, true, true, false, false },
                new object[] { false, false, true, true, false },
                new object[] { false, false, false, false, true },
                new object[] { true, false, false, false, false },
                new object[] { false, true, false, false, false },
                new object[] { false, false, true, false, false },
                new object[] { false, false, false, true, false },
                new object[] { false, false, false, false, false },
            };
        }
    }
}
