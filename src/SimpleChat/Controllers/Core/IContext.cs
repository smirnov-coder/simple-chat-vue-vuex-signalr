using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Core
{
    public interface IContext
    {
        bool ContainsKey(string key);

        void Set(string key, object value);

        object Get(string key);
    }
}
