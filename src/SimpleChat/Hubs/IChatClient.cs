using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimpleChat.Models;

namespace SimpleChat.Hubs
{
    public interface IChatClient
    {
        Task ReceiveMessage(ChatMessage message);

        Task NewUser(User user);

        Task ConnectedUsers(IEnumerable<string> ownConnectionIds, IEnumerable<User> users);

        Task NewUserConnection(string userId, string connectionId);

        Task DisconnectedUser(string userId, string connectionId);

        Task ForceSignOut();
    }
}
