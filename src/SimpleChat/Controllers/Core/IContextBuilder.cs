using Microsoft.AspNetCore.Identity;
using SimpleChat.Models;
using SimpleChat.Services;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleChat.Controllers.Core
{
    /// <summary>
    /// Вспомогательный класс для создания объекта контекста <see cref="IContext"/>. Реализует классический паттерн
    /// "Строитель" (Builder).
    /// </summary>
    public interface IContextBuilder
    {
        /// <summary>
        /// Создаёт новый объект контекста <see cref="IContext"/> с заданными параметрами.
        /// </summary>
        IContext Build();

        /// <summary>
        /// Добавляет в контекст тип клайма маркера доступа, по которому извлекается маркер доступа к внешнему
        /// OAuth2-провайдеру из коллекции клаймов пользователя.
        /// </summary>
        /// <param name="accessTokenClaimType">Тип клайма маркера доступа.</param>
        IContextBuilder WithAccessTokenClaimType(string accessTokenClaimType);

        /// <summary>
        /// Добавляет в контекст код авторизации, используемый для обмена на маркер доступа, согласно алгоритму
        /// Authorization Code Flow (тут должна быть ссылка на описание).
        /// </summary>
        /// <param name="authorizationCode">Код авторизации.</param>
        IContextBuilder WithAuthorizationCode(string authorizationCode);

        /// <summary>
        /// Добавляет в контекст тип клайма аватара пользователя, по которому извлекается аватар (веб-путь к файлу
        /// изображения) пользователя из данных аутентификации запроса или коллекции клаймов пользователя.
        /// </summary>
        /// <param name="avatarClaimType">Тип клайма аватара пользователя.</param>
        IContextBuilder WithAvatarClaimType(string avatarClaimType);

        /// <summary>
        /// Добавляет в контекст код подтверждения e-mail.
        /// </summary>
        /// <param name="confirmationCode">Код подтверждения e-mail.</param>
        IContextBuilder WithConfirmationCode(string confirmationCode);

        /// <summary>
        /// Добавляет в контекст данные пользователя, полученные от внешнего OAuth2-провайдера.
        /// </summary>
        /// <param name="userInfo">Данные пользователя внешнего OAuth2-провайдера.</param>
        IContextBuilder WithExternalUserInfo(ExternalUserInfo userInfo);

        /// <summary>
        /// Добавляет в контекст Identity-данные пользователя.
        /// </summary>
        /// <param name="identityUser">Identity-данные пользователя.</param>
        IContextBuilder WithIdentityUser(IdentityUser identityUser);

        /// <summary>
        /// Добавляет в контекст тип клайма имени пользователя, по которому извлекается полное имя пользователя из
        /// данных аутентификации запроса или коллекции клаймов пользователя.
        /// </summary>
        /// <param name="nameClaimType">Тип клайма имени пользователя.</param>
        IContextBuilder WithNameClaimType(string nameClaimType);

        /// <summary>
        /// Добавляет в контекст объект OAuth2-сервиса.
        /// </summary>
        /// <param name="oauth2Service">OAuth2-сервис.</param>
        IContextBuilder WithOAuth2Service(IOAuth2Service oauth2Service);

        /// <summary>
        /// Добавляет в контекст имя внешнего OAuth2-провайдера.
        /// </summary>
        /// <param name="provider">Имя внешнего OAuth2-провайдера</param>
        IContextBuilder WithProvider(string provider);

        /// <summary>
        /// Добавляет в контекст данные аутентификации пользователя текущего запроса к контроллеру.
        /// </summary>
        /// <param name="requestUser">Данные аутентификации пользователя.</param>
        IContextBuilder WithRequestUser(ClaimsPrincipal requestUser);

        /// <summary>
        /// Добавляет в контекст идентификатор сессии пользователя, которая используется для краткосрочного хранения
        /// информации, необходимой при подтверждении первого входа на сайт через внешнего OAuth2-провайдера.
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии пользователя.</param>
        IContextBuilder WithSessionId(string sessionId);

        /// <summary>
        /// Добавляет в контекст произвольные данные (состояние), возвращаемые внешним OAuth2-провайдером вместе с кодом
        /// авторизации для верификации запроса.
        /// </summary>
        /// <param name="state">Данные состояния запроса к внешнему OAuth2-провайдеру.</param>
        IContextBuilder WithState(string state);

        /// <summary>
        /// Добавляет в контекст коллекцию клаймов пользователя.
        /// </summary>
        /// <param name="userClaims">Коллекция клаймов пользователя.</param>
        IContextBuilder WithUserClaims(IList<Claim> userClaims);

        /// <summary>
        /// Добавляет в контекст идентификационное имя пользователя (логина, которым выступает e-mail пользователя).
        /// </summary>
        /// <param name="userName">Идентификационное имя пользователя.</param>
        IContextBuilder WithUserName(string userName);
    }
}
