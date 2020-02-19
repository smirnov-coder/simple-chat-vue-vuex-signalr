using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    /// <inheritdoc cref="ISessionWrapper"/>
    public class SessionWrapper : ISessionWrapper
    {
        private IGuard _guard;
        private ISession _session;

        public SessionWrapper(IHttpContextAccessor httpContextAccessor) : this(httpContextAccessor, null)
        {
        }

        public SessionWrapper(IHttpContextAccessor httpContextAccessor, IGuard guard)
        {
            _guard = guard ?? new Guard();
            _guard.EnsureObjectParamIsNotNull(httpContextAccessor, nameof(httpContextAccessor));

            _session = httpContextAccessor.HttpContext?.Session ?? throw new InvalidOperationException(
                $"Свойство '{nameof(HttpContext.Session)}' объекта '{nameof(HttpContext)}' равно null.");

            if (!_session.IsAvailable)
                _session.LoadAsync(default).Wait();
        }

        public bool IsAvailable => _session.IsAvailable;

        public IEnumerable<string> Keys => _session.Keys;

        public void Clear() => _session.Clear();

        public Task CommitAsync() => _session.CommitAsync();

        public string GetString(string key) => _session.GetString(key);

        public Task LoadAsync() => _session.LoadAsync();

        public void SetString(string key, string value) => _session.SetString(key, value);
    }
}
