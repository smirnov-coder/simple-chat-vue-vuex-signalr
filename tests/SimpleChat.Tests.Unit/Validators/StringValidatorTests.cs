using Moq;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace SimpleChat.Tests.Unit.Validators
{
    public class StringValidatorTests
    {
        private IContext _testContext = new Context();
        ICollection<string> _testErrors = new List<string>();
        private StringValidator _target;

        public StringValidatorTests()
        {
            Mock<IGuard> mockGuard = new Mock<IGuard>();
            mockGuard.Setup(x => x.EnsureStringParamIsNotNullOrEmpty(TestConstants.TestKey, TestConstants.KeyParamName))
                .Returns(TestConstants.TestKey);
            _target = new StringValidator(TestConstants.TestKey, mockGuard.Object);
        }

        [Fact]
        public void Validate_Good()
        {
            // arrange
            _testContext.Set(TestConstants.TestKey, TestConstants.TestValue);

            // act
            bool result = _target.Validate(_testContext, _testErrors);

            // assert
            Assert.True(result);
            Assert.Empty(_testErrors);
        }

        [Fact]
        public void Validate_Bad_InvalidValueType()
        {
            // arrange
            _testContext.Set(TestConstants.TestKey, new object());

            // act
            bool result = _target.Validate(_testContext, _testErrors);

            // assert
            Assert.False(result);
            Assert.Single(_testErrors);
            Assert.Equal("Неверный тип значения по ключу 'test_key' в контексте. Ожидаемый тип: System.String. " +
                "Фактический тип: System.Object.", _testErrors.First());
        }

        [Theory]
        [InlineData(""), InlineData(" ")]
        public void Validate_Bad_ValueIsEmptyString(string value)
        {
            // arrange
            _testContext.Set(TestConstants.TestKey, value);

            // act
            bool result = _target.Validate(_testContext, _testErrors);

            // assert
            Assert.False(result);
            Assert.Single(_testErrors);
            Assert.Equal("Значение по ключу 'test_key' равно пустой строке.", _testErrors.First());
        }
    }
}
