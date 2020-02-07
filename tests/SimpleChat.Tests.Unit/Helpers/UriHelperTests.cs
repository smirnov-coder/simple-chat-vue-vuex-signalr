using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Moq;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SimpleChat.Tests.Unit.Helpers
{
    public class UriHelperTests
    {
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        private Mock<LinkGenerator> _mockLinkGenerator = new Mock<LinkGenerator>();
        private Mock<IGuard> _mockGuard = new Mock<IGuard>();
        private UriHelper _target;

        public UriHelperTests()
        {
            _target = new UriHelper(_mockHttpContextAccessor.Object, _mockLinkGenerator.Object, _mockGuard.Object);
        }

        [Theory]
        [InlineData("https://example.com"), InlineData(""), InlineData(" ")]
        public void AddQueryString_Good(string uri)
        {
            // arrange
            var testParams = new Dictionary<string, string>
            {
                ["param_1"] = "value_1",
                ["param_2"] = "value_2"
            };

            // act
            string result = _target.AddQueryString(uri, testParams);

            // assert
            _mockGuard.Verify(x => x.EnsureObjectParamIsNotNull((IDictionary<string, string>)testParams, "queryParams"),
                Times.Once());
            Assert.Equal(uri + "?param_1=value_1&param_2=value_2", result);
        }
    }
}
