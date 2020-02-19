using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SimpleChat.Infrastructure.Constants;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;

namespace SimpleChat.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatHubClient>
    {
        private IGuard _guard;
        private IUserCollection _users;

        public ChatHub(IUserCollection users, IGuard guard = null)
        {
            _guard = guard ?? new Guard();
            _users = _guard.EnsureObjectParamIsNotNull(users, nameof(users));
        }

        /// <summary>
        /// Отправляет сообщение в чат.
        /// </summary>
        /// <param name="message">Новое сообщение.</param>
        [HubMethodName("SendMessage")]
        public async Task SendMessageAsync(ChatMessage message)
        {
            await Clients.All.ReceiveMessage(message);
        }

        public override async Task OnConnectedAsync()
        {
            string userId = Context.UserIdentifier;
            var user = _users.GetUser(userId);
            // Пользователь не подключён к хабу совсем.
            if (user == null)
            {
                await ConnectNewUser();
            }
            // Пользователь уже имеет подключение к хабу.
            else 
            {
                string provider = Context.User.FindFirstValue(CustomClaimTypes.Provider);
                // Пользователь вошёл с другого устройства через ТОГО ЖЕ логин-провайдера.
                if (user.Provider == provider)
                {
                    var connectionIds = _users.GetUsers().Aggregate(new List<string>(), (result, userItem) =>
                    {
                        result.AddRange(userItem.ConnectionIds);
                        return result;
                    });
                    _users.AddConnection(user.Id, Context.ConnectionId);
                    await Clients.Clients(connectionIds).NewUserConnection(user.Id, Context.ConnectionId);
                    user = _users.GetUser(userId);
                    await SendToCurrentUserConnectedUserCollection(user);
                }
                // Пользователь вошёл с другого устройства через ДРУГОГО логин-провайдера.
                else
                {
                    await Clients.User(user.Id).ForceSignOut();
                    _users.RemoveUser(user.Id);
                    await ConnectNewUser();
                }
            }
            await base.OnConnectedAsync();
        }

        private async Task ConnectNewUser()
        {
            var user = new User
            {
                Id = Context.UserIdentifier,
                Name = Context.User.FindFirstValue(CustomClaimTypes.FullName),
                Avatar = Context.User.FindFirstValue(CustomClaimTypes.Avatar),
                Provider = Context.User.FindFirstValue(CustomClaimTypes.Provider),
            };
            user.ConnectionIds.Add(Context.ConnectionId);
            _users.AddUser(user);
            await Clients.Others.NewUser(user);
            await SendToCurrentUserConnectedUserCollection(user);
        }

        private async Task SendToCurrentUserConnectedUserCollection(User user)
        {
            await Clients.Caller.ConnectedUsers(user.ConnectionIds, _users.GetUsers()
                .Where(item => item.Id != user.Id));
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string userId = Context.UserIdentifier;
            string connectionId = Context.ConnectionId;
            _users.RemoveConnection(userId, connectionId);
            await Clients.Others.DisconnectedUser(userId, connectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
