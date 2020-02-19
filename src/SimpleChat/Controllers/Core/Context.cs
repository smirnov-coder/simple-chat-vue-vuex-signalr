using SimpleChat.Extensions;
using SimpleChat.Infrastructure.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Core
{
    /// <inheritdoc cref="IContext"/>
    public class Context : IContext
    {
        private Dictionary<string, object> _collection;
        private IGuard _guard;

        public Context() : this(null)
        {
        }

        public Context(IGuard guard)
        {
            _guard = guard ?? new Guard();
            _collection = new Dictionary<string, object>();
        }

        public bool ContainsKey(string key)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(key, nameof(key));
            return _collection.ContainsKey(key);
        }

        public object Get(string key)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(key, nameof(key));
            _collection.TryGetValue(key, out object value);
            return value;
        }

        public void Set(string key, object value)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(key, nameof(key));
            _collection.AddOrUpdate(key, value);
        }
    }
}
