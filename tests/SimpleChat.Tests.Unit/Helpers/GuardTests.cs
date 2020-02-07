using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace SimpleChat.Tests.Unit.Helpers
{
    public class GuardTests
    {
        private const string TestParamName = "test_param_name";
        private const string TestValue = "test_value";
        private const string TestMessage = "test_message";
        private Guard _target = new Guard();

        [Fact]
        public void EnsureObjectParamIsNotNull_Good()
        {
            // arrange
            var testObject = new object();

            // act
            var result = _target.EnsureObjectParamIsNotNull(testObject, TestParamName);

            // assert
            Assert.Same(testObject, result);
        }

        [Fact]
        public void EnsureObjectParamIsNotNull_Bad_ArgumentNullException()
        {
            // arrange
            object testObject = null;

            // act
            var ex = Assert.Throws<ArgumentNullException>(() => _target.EnsureObjectParamIsNotNull(testObject,
                TestParamName));

            // assert
            Assert.StartsWith("Значение не может быть равно 'null'.", ex.Message);
            Assert.Equal(TestParamName, ex.ParamName);
        }

        [Fact]
        public void EnsureStringParamIsNotNullOrEmpty_Good()
        {
            // act
            string result = _target.EnsureStringParamIsNotNullOrEmpty(TestValue, TestParamName);

            // assert
            Assert.Same(result, TestValue);
        }

        [Theory]
        [InlineData(""), InlineData(" "), InlineData(null)]
        public void EnsureStringParamIsNotNullOrEmpty_Bad(string value)
        {
            // act
            var ex = Assert.Throws<ArgumentException>(() => _target.EnsureStringParamIsNotNullOrEmpty(value,
                TestParamName));

            // assert
            Assert.StartsWith("Значение не может быть пустой строкой или равно 'null'.", ex.Message);
            Assert.Equal(TestParamName, ex.ParamName);
        }

        [Fact]
        public void EnsureStringPropertyIsNotNullOrEmpty_Good()
        {
            // act
            string result = _target.EnsureStringPropertyIsNotNullOrEmpty(TestValue, TestMessage);

            // assert
            Assert.Same(result, TestValue);
        }

        [Theory]
        [InlineData(""), InlineData(" "), InlineData(null)]
        public void EnsureStringPropertyIsNotNullOrEmpty_Bad_InvalidOperationException(string value)
        {
            // act
            var ex = Assert.Throws<InvalidOperationException>(
                () => _target.EnsureStringPropertyIsNotNullOrEmpty(value, TestMessage));

            // assert
            Assert.Equal(TestMessage, ex.Message);
        }
    }
}
