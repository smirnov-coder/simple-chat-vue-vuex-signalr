using Microsoft.IdentityModel.Tokens;
using Moq;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace SimpleChat.Tests.Unit.Helpers
{
    public class JwtHelperTests
    {
        private Mock<JwtOptions> _mockOptions = new Mock<JwtOptions>();
        private Mock<JwtSecurityTokenHandler> _mockTokenHandler = new Mock<JwtSecurityTokenHandler>();
        private readonly TimeSpan TestLifetime = TimeSpan.FromDays(10);
        private Mock<SymmetricSecurityKey> _mockSecurityKey = new Mock<SymmetricSecurityKey>(new byte[1024]);
        private JwtHelper _target;

        private const string TestIssuer = "test_issuer";
        private const string TestAudience = "test_audience";
        private const string TestJwt = "test_jwt";

        public JwtHelperTests()
        {
            _mockOptions.Setup(x => x.Issuer).Returns(TestIssuer);
            _mockOptions.Setup(x => x.Audience).Returns(TestAudience);
            _mockOptions.Setup(x => x.Lifetime).Returns(TestLifetime);
            _mockOptions.Setup(x => x.GetSymmetricSecurityKey()).Returns(_mockSecurityKey.Object);
            _target = new JwtHelper(_mockOptions.Object, _mockTokenHandler.Object);
        }

        [Fact]
        public void CreateEncodedJwt_Good()
        {
            // arrange
            var testClaims = new Claim[]
            {
                new Claim(ClaimTypes.Name, TestConstants.TestName)
            };
            _mockTokenHandler.Setup(x => x.WriteToken(It.IsAny<JwtSecurityToken>())).Returns(TestJwt);

            // act
            string result = _target.CreateEncodedJwt(testClaims);

            // assert
            _mockOptions.Verify(x => x.Issuer, Times.Once());
            _mockOptions.Verify(x => x.Audience, Times.Once());
            _mockOptions.Verify(x => x.Lifetime, Times.Once());
            _mockOptions.Verify(x => x.GetSymmetricSecurityKey(), Times.Once());
            _mockTokenHandler.Verify(x => x.WriteToken(It.Is<JwtSecurityToken>(
                token => VerifyJwtSecurityToken(token))));
            Assert.Equal(TestJwt, result);
        }

        private bool VerifyJwtSecurityToken(JwtSecurityToken token)
        {
            return token.Issuer == TestIssuer
                && token.Audiences.Count() == 1
                && token.Audiences.First() == TestAudience
                && token.SignatureAlgorithm == SecurityAlgorithms.HmacSha256
                && token.SigningCredentials.Key == _mockSecurityKey.Object
                && token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name
                    && claim.Value == TestConstants.TestName) != null;
        }

        [Fact]
        public void ValidateToken_Good()
        {
            // arrange
            SecurityToken token = new JwtSecurityToken();
            _mockTokenHandler.Setup(x => x.ValidateToken(It.IsAny<string>(), It.IsAny<TokenValidationParameters>(),
                out token));

            // act
            _target.ValidateToken(TestJwt);

            // assert
            _mockTokenHandler.Verify(x => x.ValidateToken(TestJwt, It.Is<TokenValidationParameters>(
                parameters => VerifyValidationParameters(parameters)), out token), Times.Once());
            Assert.NotNull(token);
        }

        private bool VerifyValidationParameters(TokenValidationParameters parameters)
        {
            return parameters.ValidateIssuer
                && parameters.ValidIssuer == TestIssuer
                && parameters.ValidateAudience
                && parameters.ValidAudience == TestAudience
                && parameters.ValidateLifetime
                && parameters.ValidateIssuerSigningKey
                && parameters.IssuerSigningKey == _mockSecurityKey.Object;
        }

        [Fact]
        public void GetValidationParameters_Good()
        {
            // act
            var result = _target.GetValidationParameters();

            // assert
            Assert.True(VerifyValidationParameters(result));
        }
    }
}
