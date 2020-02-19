using SimpleChat.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    /// <inheritdoc cref="ISessionHelper"/>
    public class SessionHelper : ISessionHelper
    {
        private IGuard _guard;
        private ISessionWrapper _session;
        private IJsonHelper _jsonHelper;

        public SessionHelper(ISessionWrapper sessionWrapper, IJsonHelper jsonHelper)
            : this(sessionWrapper, jsonHelper, null)
        {
        }

        public SessionHelper(ISessionWrapper sessionWrapper, IJsonHelper jsonHelper, IGuard guard)
        {
            _guard = guard ?? new Guard();
            _session = _guard.EnsureObjectParamIsNotNull(sessionWrapper, nameof(sessionWrapper));
            _jsonHelper = _guard.EnsureObjectParamIsNotNull(jsonHelper, nameof(jsonHelper));
        }

        public ExternalUserInfo FetchUserInfo(string sessionId)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(sessionId, nameof(sessionId));

            string json = _session.GetString(sessionId);
            return _jsonHelper.DeserializeObject<ExternalUserInfo>(json);
        }

        public async Task<string> SaveUserInfoAsync(ExternalUserInfo userInfo)
        {
            _guard.EnsureObjectParamIsNotNull(userInfo, nameof(userInfo));

            string json = _jsonHelper.SerializeObject(userInfo);
            string sessionId = Guid.NewGuid().ToString();
            _session.SetString(sessionId, json);
            await _session.CommitAsync();
            return sessionId;
        }

        public void ClearSession()
        {
            _session.Clear();
        }

        public bool SessionExists(string sessionId)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(sessionId, nameof(sessionId));

            return _session.Keys.Contains(sessionId);
        }
    }
}
