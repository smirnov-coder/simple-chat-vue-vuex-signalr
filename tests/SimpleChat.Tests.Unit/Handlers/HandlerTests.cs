using Moq;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Handlers;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Handlers
{
    public class HandlerTests
    {
        private Mock<IGuard> _mockGuard = new Mock<IGuard>();
        private Mock<IHandler> _mockHandler = new Mock<IHandler>();
        private IContext _testContext = Mock.Of<IContext>();
        private IAuthResult _testAuthResult = Mock.Of<IAuthResult>();
        private HandlerUnderTest _target;

        public HandlerTests()
        {
            _target = new HandlerUnderTest(_mockGuard.Object);
        }

        [Fact]
        public async Task HandleAsync_Good_BreaksChain()
        {
            // arrange
            _target.Next = _mockHandler.Object;
            _target.CanHandleReturns = true;
            _target.InternalHandleAsyncReturns = _testAuthResult;

            // act
            IAuthResult result = await _target.HandleAsync(_testContext);

            // assert
            VerifyGuard();
            _mockHandler.Verify(x => x.HandleAsync(_testContext), Times.Never());
            Assert.Same(_testAuthResult, result);
        }

        private void VerifyGuard()
        {
            _mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_testContext, "context"));
        }

        [Fact]
        public async Task HandleAsync_Good_PassThrough()
        {
            // arrange
            _mockHandler.Setup(x => x.HandleAsync(_testContext)).ReturnsAsync(_testAuthResult);
            _target.Next = _mockHandler.Object;
            _target.CanHandleReturns = true;
            _target.InternalHandleAsyncReturns = null;

            // act
            IAuthResult result = await _target.HandleAsync(_testContext);

            // assert
            VerifyGuard();
            _mockHandler.Verify(x => x.HandleAsync(_testContext), Times.Once());
            Assert.Same(_testAuthResult, result);
        }

        [Fact]
        public async Task HandleAsync_Bad_InvalidOperationException()
        {
            // arrange
            _target.CanHandleReturns = false;

            // act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _target.HandleAsync(_testContext));

            // assert
            VerifyGuard();
            string newLineSymbols = Environment.NewLine;
            Assert.Equal($"error_1{newLineSymbols}error_2{newLineSymbols}error_3{newLineSymbols}", ex.Message);
        }
    }
}
