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
    public class ValidateRequestUserTests
    {
        private Mock<RequestUserValidator> _mockRequestUserValidator;
        private ContextBuilder _contextBuilder;
        private RequestUserBuilder _requestUserBuilder;
        private ValidateRequestUser _target;

        public ValidateRequestUserTests()
        {
            _mockRequestUserValidator = new Mock<RequestUserValidator>();
            var mockGuard = new Mock<IGuard>();
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockRequestUserValidator.Object,
                "requestUserValidator")).Returns(_mockRequestUserValidator.Object);
            _contextBuilder = new ContextBuilder();
            _requestUserBuilder = new RequestUserBuilder();
            _target = new ValidateRequestUser(_mockRequestUserValidator.Object, mockGuard.Object);
        }

        [Fact]
        public void Constructor_Good()
        {
            // arrange
            var mockGuard = new Mock<IGuard>();

            // act
            new ValidateRequestUser(_mockRequestUserValidator.Object, mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockRequestUserValidator.Object,
                "requestUserValidator"), Times.Once());
        }

        [Fact]
        public async Task HandleAsync_Good()
        {
            // arrange
            ClaimsPrincipal testRequestUser = _requestUserBuilder
                .WithNameClaim(TestConstants.TestUserName)
                .WithProviderClaim(TestConstants.TestProvider)
                .Build();
            IContext testContext = _contextBuilder.WithRequestUser(testRequestUser).Build();
            var mockAuthResult = new Mock<IAuthResult>();
            var mockHandler = new Mock<Handler>();
            mockHandler.Setup(x => x.HandleAsync(It.IsAny<IContext>())).ReturnsAsync(mockAuthResult.Object);
            _mockRequestUserValidator.Setup(x => x.Validate(testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _target.Next = mockHandler.Object;

            // act
            IAuthResult result = await _target.HandleAsync(testContext);

            // assert
            mockHandler.Verify(x => x.HandleAsync(testContext), Times.Once());
            Assert.Same(mockAuthResult.Object, result);
            Assert.Equal(TestConstants.TestProvider, testContext.Get(ProviderValidator.ContextKey) as string);
            Assert.Equal(TestConstants.TestUserName, testContext.Get(UserNameValidator.ContextKey) as string);
        }

        [Fact]
        public async Task HandleAsync_Bad_NotAuthenticatedResult()
        {
            // arrange
            var mockAuthResult = new Mock<IAuthResult>();
            var mockHandler = new Mock<Handler>();
            ClaimsPrincipal testRequestUser = _requestUserBuilder.Build();
            IContext testContext = _contextBuilder.WithRequestUser(testRequestUser).Build();
            _mockRequestUserValidator.Setup(x => x.Validate(testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _target.Next = mockHandler.Object;

            // act
            IAuthResult authResult = await _target.HandleAsync(testContext);

            // assert
            mockHandler.Verify(x => x.HandleAsync(It.IsAny<IContext>()), Times.Never());
            Assert.IsType<NotAuthenticatedResult>(authResult);
            var result = authResult as NotAuthenticatedResult;
            Assert.Equal("auth_check", result.Type);
            Assert.False(result.IsAuthenticated);
        }

        [Fact]
        public async Task HandleAsync_Bad_InvalidOperationException()
        {
            // arrange
            string testError = "test_error";
            IContext testContext = _contextBuilder.Build();
            _mockRequestUserValidator.Setup(x => x.Validate(testContext, It.IsAny<ICollection<string>>()))
                .Callback(new Action<IContext, ICollection<string>>(
                    (IContext context, ICollection<string> errors) => errors.Add(testError)))
                .Returns(false);

            // act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _target.HandleAsync(testContext));

            // assert
            Assert.Equal($"{testError}{Environment.NewLine}", ex.Message);
        }

        [Fact]
        public async Task HandleAsync_Bad_ErrorResult()
        {
            // arrange
            ClaimsPrincipal testRequestUser = _requestUserBuilder.WithNameClaim(TestConstants.TestName).Build();
            IContext testContext = _contextBuilder.WithRequestUser(testRequestUser).Build();
            var mockHandler = new Mock<Handler>();
            _mockRequestUserValidator.Setup(x => x.Validate(testContext, It.IsAny<ICollection<string>>()))
                .Returns(true);
            _target.Next = mockHandler.Object;

            // act
            IAuthResult authResult = await _target.HandleAsync(testContext);

            // assert
            mockHandler.Verify(x => x.HandleAsync(It.IsAny<IContext>()), Times.Never());
            Assert.IsType<ErrorResult>(authResult);
            var result = authResult as ErrorResult;
            Assert.Equal(TestConstants.ErrorResultType, result.Type);
            Assert.Equal("Отсутствует клайм 'provider' в JWT.", result.Message);
            Assert.Empty(result.Errors);
        }
    }
}
