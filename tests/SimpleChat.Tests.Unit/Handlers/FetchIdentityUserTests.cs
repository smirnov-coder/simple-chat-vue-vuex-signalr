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
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Handlers
{
    public class FetchIdentityUserTests
    {
        private Mock<UserManager<IdentityUser>> _mockUserManager;
        private Mock<UserNameValidator> _mockUserNameValidator;
        private ContextBuilder _contextBuilder;
        private IContext _testContext;
        private FetchIdentityUser _target;

        public FetchIdentityUserTests()
        {
            _mockUserManager = new Mock<UserManager<IdentityUser>>(Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            _mockUserNameValidator = new Mock<UserNameValidator>();
            var mockGuard = new Mock<IGuard>();
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockUserManager.Object,
                "userManager")).Returns(_mockUserManager.Object);
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockUserNameValidator.Object,
                "userNameValidator")).Returns(_mockUserNameValidator.Object);
            _contextBuilder = new ContextBuilder();
            _testContext = _contextBuilder.WithUserName(TestConstants.TestUserName).Build();
            _target = new FetchIdentityUser(
                _mockUserManager.Object,
                _mockUserNameValidator.Object,
                mockGuard.Object);
        }

        [Fact]
        public void Constructor_Good()
        {
            // arrange
            var mockGuard = new Mock<IGuard>();

            // act
            new FetchIdentityUser(_mockUserManager.Object, _mockUserNameValidator.Object, mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockUserManager.Object,
                "userManager"), Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockUserNameValidator.Object,
                "userNameValidator"), Times.Once());
        }

        [Fact]
        public async Task HandleAsync_Good()
        {
            // arrange
            var testIdentityUser = new IdentityUser();
            var mockAuthResult = new Mock<IAuthResult>();
            var mockHandler = new Mock<HandlerBase>();
            mockHandler.Setup(x => x.HandleAsync(It.IsAny<IContext>())).ReturnsAsync(mockAuthResult.Object);
            _mockUserManager.Setup(x => x.FindByNameAsync(TestConstants.TestUserName)).ReturnsAsync(testIdentityUser);
            _mockUserNameValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _target.Next = mockHandler.Object;

            // act
            IAuthResult result = await _target.HandleAsync(_testContext);

            // assert
            mockHandler.Verify(x => x.HandleAsync(_testContext), Times.Once());
            Assert.Same(mockAuthResult.Object, result);
            Assert.True(_testContext.ContainsKey(IdentityUserValidator.ContextKey));
            Assert.Same(testIdentityUser, _testContext.Get(IdentityUserValidator.ContextKey));
        }

        [Fact]
        public async Task HandleAsync_Bad_ErrorResult()
        {
            // arrange
            var mockAuthResult = new Mock<IAuthResult>();
            var mockHandler = new Mock<HandlerBase>();
            _mockUserManager.Setup(x => x.FindByNameAsync(TestConstants.TestUserName))
                .ReturnsAsync(default(IdentityUser));
            _mockUserNameValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _target.Next = mockHandler.Object;

            // act
            IAuthResult authResult = await _target.HandleAsync(_testContext);

            // assert
            mockHandler.Verify(x => x.HandleAsync(It.IsAny<IContext>()), Times.Never());
            Assert.IsType<ErrorResult>(authResult);
            var result = authResult as ErrorResult;
            Assert.Equal(TestConstants.ErrorResultType, result.Type);
            Assert.Equal("Пользователь не найден.", result.Message);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task HandleAsync_Bad_InvalidOperationException()
        {
            // arrange
            string testError = "test_error";
            _mockUserNameValidator.Setup(x => x.Validate(_testContext, It.IsAny<ICollection<string>>()))
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
