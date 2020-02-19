using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    /// <inheritdoc cref="IGuard"/>
    public class Guard : IGuard
    {
        public T EnsureObjectParamIsNotNull<T>(T value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName, "Значение не может быть равно 'null'.");
            return value;
        }

        public string EnsureStringParamIsNotNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Значение не может быть пустой строкой или равно 'null'.", paramName);
            return value;
        }

        public string EnsureStringPropertyIsNotNullOrEmpty(string value, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(errorMessage);
            return value;
        }
    }
}
