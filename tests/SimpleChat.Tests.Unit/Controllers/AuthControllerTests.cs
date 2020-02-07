using Microsoft.AspNetCore.Mvc;
using Moq;
using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Controllers
{
    public class AuthControllerTests
    {
        private Mock<IAuthenticationFlow> _mockAuthenticationFlow;
        private Mock<ISignInFlow> _mockSignInFlow;
        private Mock<IConfirmSignInFlow> _mockConfirmSignInFlow;
        private Mock<IContextBuilder> _mockContextBuilder;
        private AuthControllerUnderTest _target;

        public AuthControllerTests()
        {
            _mockAuthenticationFlow = new Mock<IAuthenticationFlow>();
            _mockSignInFlow = new Mock<ISignInFlow>();
            _mockConfirmSignInFlow = new Mock<IConfirmSignInFlow>();
            _mockContextBuilder = new Mock<IContextBuilder>();
            _target = new AuthControllerUnderTest(_mockAuthenticationFlow.Object, _mockSignInFlow.Object,
                _mockConfirmSignInFlow.Object, _mockContextBuilder.Object);
        }

        private IContext _testContext = new Context();
        private IAuthResult _testAuthResult = new SignInSuccessResult();

        [Fact]
        public async Task AuthenticateAsync_Good()
        {
            // arrange
            var testRequestUser = new ClaimsPrincipal();
            var testAuthResult = new AuthenticatedResult();
            _mockContextBuilder.Setup(x => x.WithRequestUser(testRequestUser)).Returns(_mockContextBuilder.Object);
            _mockContextBuilder.Setup(x => x.Build()).Returns(_testContext);
            _mockAuthenticationFlow.Setup(x => x.RunAsync(_testContext)).ReturnsAsync(testAuthResult);
            _target.GetCurrentUserReturns = testRequestUser;

            // act
            IAuthResult result = await _target.AuthenticateAsync();

            // assert
            Assert.Same(testAuthResult, result);
        }

        private const string ViewName = "_CallbackPartial";
        private const string TestError = "test_error";
        private const string TestReason = "test_reason";
        private const string TestDescription = "test_description";

        [Fact]
        public async Task SignInWithFacebookAsync_Bad_ExternalLoginErrorResult()
        {
            // act
            PartialViewResult actionResult = await _target.SignInWithFacebookAsync(TestConstants.TestCode,
                TestConstants.TestState, TestError, TestReason, TestDescription);

            // assert
            VerifyBadSignInResult(actionResult, ExternalProvider.Facebook);
            var result = actionResult.Model as ExternalLoginErrorResult;
            Assert.Equal(3, result.Errors.Count());
            Assert.Equal(TestError, result.Errors.ElementAt(0));
            Assert.Equal(TestReason, result.Errors.ElementAt(1));
            Assert.Equal(TestDescription, result.Errors.ElementAt(2));
        }

        private void VerifyBadSignInResult(PartialViewResult actionResult, string provider)
        {
            Assert.Equal(ViewName, actionResult.ViewName);
            Assert.IsType<ExternalLoginErrorResult>(actionResult.Model);
            var result = actionResult.Model as ExternalLoginErrorResult;
            Assert.Equal(TestConstants.ExternalLoginErrorResultType, result.Type);
            Assert.Equal($"Ошибка окна входа через '{provider}'.", result.Message);
        }

        [Fact]
        public async Task SignInWithFacebookAsync_Good()
        {
            // arrange
            SetupMocks(ExternalProvider.Facebook, CustomClaimTypes.FacebookName, CustomClaimTypes.FacebookAvatar);

            // act
            PartialViewResult actionResult = await _target.SignInWithFacebookAsync(TestConstants.TestCode,
                TestConstants.TestState, string.Empty, string.Empty, string.Empty);

            // assert
            VerifyGoodSignInResult(actionResult);
        }

        private void SetupMocks(string provider, string nameClaimType, string avatarClaimType)
        {
            _mockContextBuilder.Setup(x => x.WithProvider(provider)).Returns(_mockContextBuilder.Object);
            _mockContextBuilder.Setup(x => x.WithState(TestConstants.TestState)).Returns(_mockContextBuilder.Object);
            _mockContextBuilder.Setup(x => x.WithAuthorizationCode(TestConstants.TestCode))
                .Returns(_mockContextBuilder.Object);
            _mockContextBuilder.Setup(x => x.WithNameClaimType(nameClaimType)) .Returns(_mockContextBuilder.Object);
            _mockContextBuilder.Setup(x => x.WithAvatarClaimType(avatarClaimType)).Returns(_mockContextBuilder.Object);
            _mockContextBuilder.Setup(x => x.Build()).Returns(_testContext);
            _mockSignInFlow.Setup(x => x.RunAsync(_testContext)).ReturnsAsync(_testAuthResult);
        }

        private void VerifyGoodSignInResult(PartialViewResult actionResult)
        {
            Assert.Equal(ViewName, actionResult.ViewName);
            Assert.IsType<SignInSuccessResult>(actionResult.Model);
            Assert.Same(_testAuthResult, actionResult.Model as SignInSuccessResult);
        }

        [Fact]
        public async Task SignInWithVKontakteAsync_Bad_ExternalLoginErrorResult()
        {
            // act
            PartialViewResult actionResult = await _target.SignInWithVKontakteAsync(TestConstants.TestCode,
                TestConstants.TestState, TestError, TestDescription);

            // assert
            VerifyBadSignInResult(actionResult, ExternalProvider.VKontakte);
            var result = actionResult.Model as ExternalLoginErrorResult;
            Assert.Equal(2, result.Errors.Count());
            Assert.Equal(TestError, result.Errors.First());
            Assert.Equal(TestDescription, result.Errors.Last());
        }

        [Fact]
        public async Task SignInWithVKontakteAsync_Good()
        {
            // arrange
            SetupMocks(ExternalProvider.VKontakte, CustomClaimTypes.VKontakteName, CustomClaimTypes.VKontakteAvatar);

            // act
            PartialViewResult actionResult = await _target.SignInWithVKontakteAsync(TestConstants.TestCode,
                TestConstants.TestState, string.Empty, string.Empty);

            // assert
            VerifyGoodSignInResult(actionResult);
        }

        [Fact]
        public async Task SignInWithOdnoklassnikiAsync_Bad_ExternalLoginErrorResult()
        {
            // act
            PartialViewResult actionResult = await _target.SignInWithOdnoklassnikiAsync(TestConstants.TestCode,
                TestConstants.TestState, TestError);

            // assert
            VerifyBadSignInResult(actionResult, ExternalProvider.Odnoklassniki);
            var result = actionResult.Model as ExternalLoginErrorResult;
            Assert.Single(result.Errors);
            Assert.Equal(TestError, result.Errors.First());
        }

        [Fact]
        public async Task SignInWithOdnoklassnikiAsync_Good()
        {
            // arrange
            SetupMocks(ExternalProvider.Odnoklassniki, CustomClaimTypes.OdnoklassnikiName,
                CustomClaimTypes.OdnoklassnikiAvatar);

            // act
            PartialViewResult actionResult = await _target.SignInWithOdnoklassnikiAsync(TestConstants.TestCode,
                TestConstants.TestState, string.Empty);

            // assert
            VerifyGoodSignInResult(actionResult);
        }

        [Fact]
        public async Task ConfirmSigInAsync_Good()
        {
            // arrange
            _mockContextBuilder.Setup(x => x.WithSessionId(TestConstants.TestId)).Returns(_mockContextBuilder.Object);
            _mockContextBuilder.Setup(x => x.WithConfirmationCode(TestConstants.TestCode))
                .Returns(_mockContextBuilder.Object);
            _mockContextBuilder.Setup(x => x.Build()).Returns(_testContext);
            _mockConfirmSignInFlow.Setup(x => x.RunAsync(_testContext)).ReturnsAsync(_testAuthResult);

            // act
            IAuthResult result = await _target.ConfirmSigInAsync(TestConstants.TestId, TestConstants.TestCode);

            // assert
            Assert.Same(_testAuthResult, result);
        }
    }
}
