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
    public class FetchUserClaimsTests
    {
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<IdentityUserValidator> _mockIdentityUserValidator;
        private ContextBuilder _contextBuilder;
        private IContext _testContext;
        private FetchUserClaims _target;

        private IdentityUser _testIdentityUser = new IdentityUser();

        public FetchUserClaimsTests()
        {
            _mockUserManager = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            _mockIdentityUserValidator = new Mock<IdentityUserValidator>();
            var mockGuard = new Mock<IGuard>();
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockUserManager.Object,
                "userManager")).Returns(_mockUserManager.Object);
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockIdentityUserValidator.Object,
                "identityUserValidator")).Returns(_mockIdentityUserValidator.Object);
            _contextBuilder = new ContextBuilder();
            _testContext = _contextBuilder.WithIdentityUser(_testIdentityUser).Build();
            _target = new FetchUserClaims(
                _mockUserManager.Object,
                _mockIdentityUserValidator.Object,
                mockGuard.Object);
        }

        [Fact]
        public void Constructor_Good()
        {
            // arrange
            var mockGuard = new Mock<IGuard>();

            // act
            new FetchUserClaims(_mockUserManager.Object, _mockIdentityUserValidator.Object, mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockUserManager.Object,
                "userManager"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockIdentityUserValidator.Object,
                "identityUserValidator"), Times.Once());
        }

        [Fact]
        public async Task HandleAsync_Good()
        {
            // arrange
            var testUserClaims = new List<Claim>
            {
                new Claim("test_type", "test_value")
            };
            var mockAuthResult = new Mock<IAuthResult>();
            var mockHandler = new Mock<Handler>();
            mockHandler.Setup(x => x.HandleAsync(It.IsAny<IContext>())).ReturnsAsync(mockAuthResult.Object);
            _mockUserManager.Setup(x => x.GetClaimsAsync(_testIdentityUser)).ReturnsAsync(testUserClaims);
            _mockIdentityUserValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _target.Next = mockHandler.Object;

            // act
            IAuthResult result = await _target.HandleAsync(_testContext);

            // assert
            mockHandler.Verify(x => x.HandleAsync(_testContext), Times.Once());
            Assert.Same(mockAuthResult.Object, result);
            Assert.True(_testContext.ContainsKey(UserClaimsValidator.ContextKey));
            Assert.Same(testUserClaims, _testContext.Get(UserClaimsValidator.ContextKey));
        }

        [Theory]
        [MemberData(nameof(TestUserClaims))]
        public async Task HandleAsync_Bad_ErrorResult(IList<Claim> userClaims)
        {
            // arrange
            var mockAuthResult = new Mock<IAuthResult>();
            var mockHandler = new Mock<Handler>();
            _mockUserManager.Setup(x => x.GetClaimsAsync(_testIdentityUser)).ReturnsAsync(userClaims);
            _mockIdentityUserValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _target.Next = mockHandler.Object;

            // act
            IAuthResult authResult = await _target.HandleAsync(_testContext);

            // assert
            mockHandler.Verify(x => x.HandleAsync(It.IsAny<IContext>()), Times.Never());
            Assert.IsType<ErrorResult>(authResult);
            var result = authResult as ErrorResult;
            Assert.Equal(TestConstants.ErrorResultType, result.Type);
            Assert.Equal("Не удалось извлечь из хранилища информацию, необходимую для аутентификации пользователя.", 
                result.Message);
            Assert.Empty(result.Errors);
        }

        public static IEnumerable<object[]> TestUserClaims()
        {
            return new[]
            {
                new object[] { null },
                new object[] { new List<Claim>() }
            };
        }

        [Fact]
        public async Task HandleAsync_Bad_InvalidOperationException()
        {
            // arrange
            string testError = "test_error";
            _mockIdentityUserValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Callback(new Action<IContext, ICollection<string>>(
                    (IContext context, ICollection<string> errors) => errors.Add(testError)))
                .Returns(false);

            // act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _target.HandleAsync(_testContext));

            // assert
            Assert.Equal($"{testError}{Environment.NewLine}", ex.Message);
        }
    }
}
