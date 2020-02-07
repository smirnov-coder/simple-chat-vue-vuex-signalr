using Microsoft.AspNetCore.Http;
using Moq;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleChat.Tests.Unit.Helpers
{
    public class SessionHelperTests
    {
        private Mock<ISessionWrapper> _mockSessionWrapper = new Mock<ISessionWrapper>();
        private Mock<IJsonHelper> _mockJsonHelper = new Mock<IJsonHelper>();
        private Mock<IGuard> _mockGuard = new Mock<IGuard>();
        private SessionHelper _target;

        public SessionHelperTests()
        {
            _mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockSessionWrapper.Object, "sessionWrapper"))
                .Returns(_mockSessionWrapper.Object);
            _mockGuard.Setup(x => x.EnsureObjectParamIsNotNull(_mockJsonHelper.Object, "jsonHelper"))
                .Returns(_mockJsonHelper.Object);
            _target = new SessionHelper(_mockSessionWrapper.Object, _mockJsonHelper.Object, _mockGuard.Object);
        }

        [Fact]
        public void Constructor_Good()
        {
            // arrange
            var mockGuard = new Mock<IGuard>();

            // act
            new SessionHelper(_mockSessionWrapper.Object, _mockJsonHelper.Object, mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockSessionWrapper.Object, "sessionWrapper"),
                Times.Once());
            mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(_mockJsonHelper.Object, "jsonHelper"), Times.Once());
        }

        private const string TestSessionId = "test_session_id";
        private const string TestJson = "test_json";
        private const string SessionIdParamName = "sessionId";

        [Fact]
        public void FetchUserInfo_Good()
        {
            // arrange
            var testUserInfo = new ExternalUserInfo();
            _mockSessionWrapper.Setup(x => x.GetString(TestSessionId)).Returns(TestJson);
            _mockJsonHelper.Setup(x => x.DeserializeObject<ExternalUserInfo>(TestJson)).Returns(testUserInfo);

            // act
            ExternalUserInfo result = _target.FetchUserInfo(TestSessionId);

            // assert
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(TestSessionId, SessionIdParamName));
            Assert.Same(testUserInfo, result);
        }

        [Fact]
        public async Task SaveUserInfoAsync_Good()
        {
            // arrange
            string expectedSessionId = string.Empty;
            var testUserInfo = new ExternalUserInfo();
            _mockJsonHelper.Setup(x => x.SerializeObject(testUserInfo)).Returns(TestJson);
            _mockSessionWrapper.Setup(x => x.SetString(It.IsAny<string>(), TestJson))
                .Callback(new Action<string, string>((key, value) => expectedSessionId = key));

            // act
            string result = await _target.SaveUserInfoAsync(testUserInfo);

            // assert
            _mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(testUserInfo, "userInfo"));
            _mockSessionWrapper.Verify(x => x.CommitAsync(), Times.Once);
            Assert.Equal(expectedSessionId, result);
            Assert.True(Guid.TryParse(result, out _));
        }

        [Fact]
        public void ClearSession_Good()
        {
            // act
            _target.ClearSession();

            // assert
            _mockSessionWrapper.Verify(x => x.Clear(), Times.Once);
        }

        [Fact]
        public void SessionExists_Good()
        {
            // arrange
            var testKeys = new string[] { TestSessionId };
            _mockSessionWrapper.Setup(x => x.Keys).Returns(testKeys);

            // act
            bool result = _target.SessionExists(TestSessionId);

            // assert
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(TestSessionId, SessionIdParamName), Times.Once);
            Assert.True(result);
        }
    }
}
