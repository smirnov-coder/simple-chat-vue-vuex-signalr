using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    public interface ISessionHelper
    {
        Task<string> SaveUserInfoAsync(ExternalUserInfo userInfo);

        ExternalUserInfo FetchUserInfo(string sessionId);

        void ClearSession();

        bool SessionExists(string sessionId);
    }
}
