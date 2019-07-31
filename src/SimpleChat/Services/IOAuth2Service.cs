using SimpleChat.Models;
using System.Threading.Tasks;

namespace SimpleChat.Services
{
    public interface IOAuth2Service
    {
        string RedirectUri { get; set; }

        Task<string> GetAccessTokenAsync(string code);

        Task<ExternalUserInfo> GetUserInfoAsync(string accessToken);
    }
}
