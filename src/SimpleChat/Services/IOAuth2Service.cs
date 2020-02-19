using SimpleChat.Models;
using System.Threading.Tasks;

namespace SimpleChat.Services
{
    /// <summary>
    /// Инкапсулирует доступ к API внешнего OAuth2-провайдера.
    /// </summary>
    public interface IOAuth2Service
    {
        /// <summary>
        /// Адрес, на который будет перенаправлен браузер пользователя после авторизации во всплывающем окне на стороне
        /// внешнего OAuth2-провайдера.
        /// </summary>
        string RedirectUri { get; set; }

        /// <summary>
        /// Маркер доступа для выполнения запросов к API внешнего OAuth2-провайдера.
        /// </summary>
        string AccessToken { get; set; }

        /// <summary>
        /// Информация о пользователе, полученная от внешнего OAuth2-провайдера.
        /// </summary>
        ExternalUserInfo UserInfo { get; }

        /// <summary>
        /// Асинхронно производит обмен кода авторизации на маркер доступа.
        /// </summary>
        /// <param name="code">
        /// Код авторизации, полученный после авторизации на стороне внешнего OAuth2-провайдера.
        /// </param>
        Task RequestAccessTokenAsync(string code);

        /// <summary>
        /// Асинхронно запрашивает у внешнего OAtuh2-провайдера информацию о пользователе.
        /// </summary>
        Task RequestUserInfoAsync();
    }

    /// <summary>
    /// Инкапсулирует доступ к API OAuth2-службы соц. сети "Facebook".
    /// </summary>
    public interface IFacebookOAuth2Service : IOAuth2Service
    {
    }

    /// <summary>
    /// Инкапсулирует доступ к API OAuth2-службы соц. сети "ВКонтакте".
    /// </summary>
    public interface IVKontakteOAuth2Service : IOAuth2Service
    {
    }

    /// <summary>
    /// Инкапсулирует доступ к API OAuth2-службы соц. сети "Одноклассники".
    /// </summary>
    public interface IOdnoklassnikiOAuth2Service : IOAuth2Service
    {
    }
}
