using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace SimpleChat.Tests.Unit.Validators
{
    public class UserClaimsValidatorTests
    {
        [Fact]
        public void Validate_Bad_UserClaimsAreEmpty()
        {
            // arrange
            var contextBuilder = new ContextBuilder();
            IContext testContest = contextBuilder.WithUserClaims(new List<Claim>()).Build();
            ICollection<string> testErrors = new List<string>();
            var target = new UserClaimsValidator();

            // act
            bool result = target.Validate(testContest, testErrors);

            // assert
            Assert.False(result);
            Assert.Single(testErrors);
            Assert.Equal("Коллекция клаймов пользователя пуста.", testErrors.First());
        }
    }
}
