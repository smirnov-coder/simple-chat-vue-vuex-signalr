using Microsoft.AspNetCore.Http;
using Moq;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SimpleChat.Tests.Unit.Helpers
{
    public class SessionWrapperTests
    {
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        private Mock<HttpContext> _mockHttpContext = new Mock<HttpContext>();
        private Mock<ISession> _mockSession = new Mock<ISession>();
        private Mock<IGuard> _mockGuard = new Mock<IGuard>();

        public SessionWrapperTests()
        {
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);
        }

        [Fact]
        public void Constructor_Good_SessionLoaded()
        {
            // arrange
            _mockSession.Setup(x => x.IsAvailable).Returns(true);
            _mockHttpContext.Setup(x => x.Session).Returns(_mockSession.Object);

            // act
            new SessionWrapper(_mockHttpContextAccessor.Object, _mockGuard.Object);

            // assert
            VerifyGuard();
            _mockSession.Verify(x => x.LoadAsync(default), Times.Never);
        }

        private void VerifyGuard()
        {
            _mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockHttpContextAccessor.Object, "httpContextAccessor"),
                Times.Once);
        }

        [Fact]
        public void Constructor_Good_SessionNotLoaded()
        {
            // arrange
            _mockSession.Setup(x => x.IsAvailable).Returns(false);
            _mockHttpContext.Setup(x => x.Session).Returns(_mockSession.Object);

            // act
            new SessionWrapper(_mockHttpContextAccessor.Object, _mockGuard.Object);

            // assert
            VerifyGuard();
            _mockSession.Verify(x => x.LoadAsync(default), Times.Once);
        }

        [Fact]
        public void Constructor_Bad_InvalidOperationException()
        {
            // arrange
            _mockHttpContext.Setup(x => x.Session).Returns(default(ISession));

            // act
            var ex = Assert.Throws<InvalidOperationException>(
                () => new SessionWrapper(_mockHttpContextAccessor.Object, _mockGuard.Object));

            // assert
            VerifyGuard();
            Assert.Equal("Свойство 'Session' объекта 'HttpContext' равно null.", ex.Message);
        }
    }
}
