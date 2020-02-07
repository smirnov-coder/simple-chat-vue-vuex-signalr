using Moq;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Handlers;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using SimpleChat.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Handlers
{
    public class RequestUserInfoTests
    {
        private Mock<OAuth2ServiceValidator> _mockValidator = new Mock<OAuth2ServiceValidator>();
        private ContextBuilder _contextBuilder = new ContextBuilder();
        private RequestUserInfo _target;

        public RequestUserInfoTests()
        {
            var mockGuard = new Mock<IGuard>();
            mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockValidator.Object,
                "oauth2ServiceValidator")).Returns(_mockValidator.Object);
            _target = new RequestUserInfo(_mockValidator.Object, mockGuard.Object);
        }

        [Fact]
        public void Constructor_Good()
        {
            // arrange
            var mockGuard = new Mock<IGuard>();

            // act
            new RequestUserInfo(_mockValidator.Object, mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockValidator.Object,
                "oauth2ServiceValidator"), Times.Once());
        }

        [Fact]
        public async Task HandleAsync_Good()
        {
            // arrange
            var testUserInfo = new ExternalUserInfo { Email = "test_email" };
            var mockOAuth2Service = new Mock<IOAuth2Service>();
            mockOAuth2Service.Setup(x => x.UserInfo).Returns(testUserInfo);
            IContext testContext = _contextBuilder.WithOAuth2Service(mockOAuth2Service.Object).Build();
            var mockAuthResult = new Mock<IAuthResult>();
            var mockHandler = new Mock<Handler>();
            mockHandler.Setup(x => x.HandleAsync(It.IsAny<IContext>())).ReturnsAsync(mockAuthResult.Object);
            _mockValidator.Setup(x => x.Validate(testContext, It.IsAny<ICollection<string>>())).Returns(true);
            _target.Next = mockHandler.Object;

            // act
            IAuthResult result = await _target.HandleAsync(testContext);

            // assert
            mockOAuth2Service.Verify(x => x.RequestUserInfoAsync(), Times.Once());
            mockHandler.Verify(x => x.HandleAsync(testContext), Times.Once());
            Assert.Same(mockAuthResult.Object, result);
            Assert.Same(testUserInfo, testContext.Get(UserInfoValidator.ContextKey));
            Assert.Equal("test_email", testContext.Get(UserNameValidator.ContextKey) as string);
        }

        [Fact]
        public async Task HandleAsync_Bad_InvalidOperationException()
        {
            // arrange
            string testError = "test_error";
            IContext testContext = _contextBuilder.Build();
            _mockValidator.Setup(x => x.Validate(testContext, It.IsAny<ICollection<string>>()))
                .Callback(new Action<IContext, ICollection<string>>(
                    (IContext context, ICollection<string> errors) => errors.Add(testError)))
                .Returns(false);

            // act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _target.HandleAsync(testContext));

            // assert
            Assert.Equal($"{testError}{Environment.NewLine}", ex.Message);
        }

        [Fact]
        public async Task HandleAsync_Bad_ExternalLoginErrorResult()
        {
            // arrange
            string
                testMessage = "test_message",
                testExternalLoginError = "test_external_login_error";
            var mockOAuth2Service = new Mock<IOAuth2Service>();
            IContext testContext = _contextBuilder.WithOAuth2Service(mockOAuth2Service.Object).Build();
            var mockHandler = new Mock<Handler>();
            var exception = new OAuth2ServiceException(testMessage);
            exception.Data.Add("message", testExternalLoginError);
            mockOAuth2Service.Setup(x => x.RequestUserInfoAsync()).ThrowsAsync(exception);
            _mockValidator.Setup(x => x.Validate(testContext, It.IsAny<ICollection<string>>())).Returns(true);
            _target.Next = mockHandler.Object;

            // act
            IAuthResult authResult = await _target.HandleAsync(testContext);

            // assert
            mockHandler.Verify(x => x.HandleAsync(It.IsAny<IContext>()), Times.Never());
            Assert.IsType<ExternalLoginErrorResult>(authResult);
            var result = authResult as ExternalLoginErrorResult;
            Assert.Equal(TestConstants.ExternalLoginErrorResultType, result.Type);
            Assert.Equal(testMessage, result.Message);
            Assert.Single(result.Errors);
            Assert.Equal(testExternalLoginError, result.Errors.First());
        }

        //[Fact]
        //public async Task HandleAsync_Bad_ErrorResult()
        //{
        //    // arrange
        //    var mockOAuth2Service = new Mock<IOAuth2Service>();
        //    IFlowContext testContext = _contextBuilder.WithOAuth2Service(mockOAuth2Service.Object).Build();
        //    var mockHandler = new Mock<Handler>();
        //    mockOAuth2Service.Setup(x => x.RequestUserInfoAsync()).ThrowsAsync(new Exception());
        //    _mockValidator.Setup(x => x.Validate(testContext, It.IsAny<ICollection<string>>())).Returns(true);
        //    _target.Next = mockHandler.Object;

        //    // act
        //    IAuthResult authResult = await _target.HandleAsync(testContext);

        //    // assert
        //    mockHandler.Verify(x => x.HandleAsync(testContext), Times.Never());
        //    Assert.IsType<ErrorResult>(authResult);
        //    var result = authResult as ErrorResult;
        //    Assert.Equal(TestConstants.ErrorResultType, result.Type);
        //    Assert.Equal("Что-то пошло не так. Обратитесь к администратору.", result.Message);
        //    Assert.Empty(result.Errors);
        //}
    }
}
