using Moq;
using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleChat.Tests.Unit.Handlers
{
    public class HandlersTestHelper
    {
        public static void AddError(StringBuilder builder, bool fail, string message, ICollection<string> errors)
        {
            if (fail)
            {
                errors.Add(message);
                builder.AppendLine(message);
            }
        }

        public static void SetupMockValidatorWithErrors(Mock<IValidator> mockValidator, IContext context,
            bool returns, string error, StringBuilder errorCollector)
        {
            mockValidator.Setup(x => x.Validate(context, It.IsAny<ICollection<string>>()))
                .Callback(new Action<IContext, ICollection<string>>(
                    (IContext theContext, ICollection<string> errors) => AddError(
                        errorCollector, !returns, error, errors)))
                .Returns(returns);
        }
    }
}
