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

    // Для возможности легко замокать соответствующие сервисы.
    /// TODO: заменить на использование AutoMocker
    public interface IFacebookOAuth2Service : IOAuth2Service
    {
    }

    public interface IVKontakteOAuth2Service : IOAuth2Service
    {
    }

    public interface IOdnoklassnikiOAuth2Service : IOAuth2Service
    {
    }
}
