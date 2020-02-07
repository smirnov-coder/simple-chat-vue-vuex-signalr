using System.Collections.Generic;

namespace SimpleChat.Hubs
{
    public interface IUserCollection
    {
        bool AddConnection(string userId, string connectionId);

        bool AddUser(User user);

        User GetUser(string userId);

        IEnumerable<User> GetUsers();

        void RemoveConnection(string userId, string connectionId);

        void RemoveUser(string userId);
    }
}
