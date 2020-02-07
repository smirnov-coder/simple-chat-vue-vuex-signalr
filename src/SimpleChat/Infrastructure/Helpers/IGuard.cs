using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    public interface IGuard
    {
        string EnsureStringParamIsNotNullOrEmpty(string value, string paramName);

        T EnsureObjectParamIsNotNull<T>(T value, string paramName);

        string EnsureStringPropertyIsNotNullOrEmpty(string value, string errorMessage);
    }
}
