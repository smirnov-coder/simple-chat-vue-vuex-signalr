using Moq;
using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace SimpleChat.Tests.Unit.Validators
{
    public class ValidatorBaseTests
    {
        private Mock<IGuard> _mockGuard = new Mock<IGuard>();
        private ValidatorBaseUnderTest _target;

        public ValidatorBaseTests()
        {
            _mockGuard.Setup(x => x.EnsureStringParamIsNotNullOrEmpty(TestConstants.TestKey,
                TestConstants.KeyParamName)).Returns(TestConstants.TestKey);
            _target = new ValidatorBaseUnderTest(TestConstants.TestKey, _mockGuard.Object);
        }
        
        private IContext _testContext = new Context();
        private ICollection<string> _testErrors = new List<string>();

        [Fact]
        public void Constructor_Good()
        {
            // arrange
            var mockGuard = new Mock<IGuard>();

            // act
            new ValidatorBaseUnderTest(TestConstants.TestKey, mockGuard.Object);

            // assert
            mockGuard.Verify(x => x.EnsureStringParamIsNotNullOrEmpty(TestConstants.TestKey,
                TestConstants.KeyParamName), Times.Once());
        }

        [Fact]
        public void Validate_Good_ReturnsTrue()
        {
            // arrange
            _testContext.Set(TestConstants.TestKey, TestConstants.TestValue);
            _target.InternalValidateCallback = (context, errors) =>
            {
                Assert.True(context.ContainsKey(TestConstants.TestKey));
                Assert.Empty(errors);
            };

            // act
            bool result = _target.Validate(_testContext, _testErrors);

            // assert
            VerifyGuard(_testContext, _testErrors);
            Assert.True(result);
        }

        private void VerifyGuard(IContext context, ICollection<string> errors)
        {
            _mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(context, "context"), Times.Once());
            _mockGuard.Verify(x => x.EnsureObjectParamIsNotNull(errors, "errors"), Times.Once());
        }

        [Fact]
        public void Validate_Bad_InvalidOperationException()
        {
            // arrange
            _testErrors.Add("test_error");

            // act
            var ex = Assert.Throws<InvalidOperationException>(() => _target.Validate(_testContext, _testErrors));

            // assert
            VerifyGuard(_testContext, _testErrors);
            Assert.Equal("Коллекция 'errors' не пуста.", ex.Message);
        }

        [Fact]
        public void Validate_Good_ReturnsFalse_KeyValueMissing()
        {
            // arrange
            _target.InternalValidateCallback = (context, errors) =>
            {
                Assert.True(false, $"Неожиданный вызов 'InternalValidate'.");
            };

            // act
            bool result = _target.Validate(_testContext, _testErrors);

            // assert
            VerifyGuard(_testContext, _testErrors);
            Assert.False(result);
            Assert.Single(_testErrors);
            Assert.Equal("В контексте отсутствует значение по ключу 'test_key'.", _testErrors.First());
        }

        [Fact]
        public void Validate_Good_ReturnsFalse_ValueIsNull()
        {
            // arrange
            IContext testContext = new Context();
            testContext.Set(TestConstants.TestKey, null);
            _target.InternalValidateCallback = (context, errors) =>
            {
                Assert.True(false, $"Неожиданный вызов 'InternalValidate'.");
            };

            // act
            bool result = _target.Validate(testContext, _testErrors);

            // assert
            VerifyGuard(testContext, _testErrors);
            Assert.False(result);
            Assert.Single(_testErrors);
            Assert.Equal($"Значение по ключу 'test_key' равно null.", _testErrors.First());
        }
    }
}
