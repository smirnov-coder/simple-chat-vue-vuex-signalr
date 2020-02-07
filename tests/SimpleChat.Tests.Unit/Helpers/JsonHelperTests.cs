using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace SimpleChat.Tests.Unit.Helpers
{
    public class JsonHelperTests
    {
        private Mock<IGuard> _mockGuard = new Mock<IGuard>();
        private JsonHelper _target;

        public JsonHelperTests()
        {
            _target = new JsonHelper(_mockGuard.Object);
        }

        [Fact]
        public void SerializeObject_Good()
        {
            // arrange
            var testObject = new { test_key = TestConstants.TestValue };

            // act
            string result = _target.SerializeObject(testObject);

            // assert
            _mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(testObject, "value"), Times.Once());
            Assert.Equal(JsonConvert.SerializeObject(testObject), result);
        }

        [Fact]
        public void DeserializeObject_Good()
        {
            // arrange
            string testJson = "[\"test_value\"]";

            // act
            string[] result = _target.DeserializeObject<string[]>(testJson);

            // assert
            VerifyGuard(testJson);
            Assert.Single(result);
            Assert.Equal(TestConstants.TestValue, result.First());
        }

        private void VerifyGuard(string json)
        {
            _mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(json, "json"), Times.Once());
        }

        [Fact]
        public void Parse_Good()
        {
            // arrange
            string testJson = "{\"test_key\":\"test_value\"}";

            // act
            var result = _target.Parse(testJson);

            // assert
            VerifyGuard(testJson);
            Assert.Single(result);
            Assert.Equal(TestConstants.TestKey, ((JProperty)result.First).Name);
            Assert.Equal(TestConstants.TestValue, (string)result[TestConstants.TestKey]);
        }
    }
}
