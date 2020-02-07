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
    public class ObjectValidatorTests
    {
        private IContext _testContext = new Context();
        ICollection<string> _testErrors = new List<string>();
        private ObjectValidator<TestClass1> _target;

        public ObjectValidatorTests()
        {
            Mock<IGuard> mockGuard = new Mock<IGuard>();
            mockGuard.Setup(x => x.EnsureStringParamIsNotNullOrEmpty(TestConstants.TestKey, TestConstants.KeyParamName))
                .Returns(TestConstants.TestKey);
            _target = new ObjectValidator<TestClass1>(TestConstants.TestKey, mockGuard.Object);
        }

        [Fact]
        public void Validate_Good()
        {
            // arrange
            _testContext.Set(TestConstants.TestKey, new TestClass1());

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
            _testContext.Set(TestConstants.TestKey, new TestClass2());

            // act
            bool result = _target.Validate(_testContext, _testErrors);

            // assert
            Assert.False(result);
            Assert.Single(_testErrors);
            Assert.Equal($"Неверный тип значения по ключу 'test_key' в контексте. Ожидаемый тип: " +
                $"{typeof(TestClass1).FullName}. Фактический тип: {typeof(TestClass2).FullName}.", _testErrors.First());
        }
    }

    internal class TestClass1
    {
    }

    internal class TestClass2
    {
    }
}
