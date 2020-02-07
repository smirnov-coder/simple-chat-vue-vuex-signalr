using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    public interface IMD5Hasher
    {
        string ComputeHash(string source);
    }
}
