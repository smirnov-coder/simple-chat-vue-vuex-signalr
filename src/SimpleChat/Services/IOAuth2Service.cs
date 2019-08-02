using SimpleChat.Models;
using System.Threading.Tasks;

namespace SimpleChat.Services
{
    public interface IOAuth2Service
    {
        string RedirectUri { get; set; }

        string AccessToken { get; set; }

        ExternalUserInfo UserInfo { get; }

        Task RequestAccessTokenAsync(string code);

        Task RequestUserInfoAsync();
    }
}
