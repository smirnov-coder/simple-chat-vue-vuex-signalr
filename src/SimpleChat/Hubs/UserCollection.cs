using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleChat.Hubs
{
    /// <inheritdoc cref="IUserCollection"/>
    public class UserCollection : IUserCollection
    {
        private List<User> _users = new List<User>();

        public bool AddUser(User user)
        {
            lock (_users)
            {
                if (_users.Contains(user))
                    return false;
                _users.Add(user);
                return true;
            }
        }

        public void RemoveUser(string userId)
        {
            lock (_users)
            {
                _users.RemoveAll(user => user.Id == userId);
            }
        }

        public bool AddConnection(string userId, string connectionId)
        {
            lock (_users)
            {
                var user = _users.FirstOrDefault(item => item.Id == userId);
                if (user == null)
                    return false;
                return user.ConnectionIds.Add(connectionId);
            }
        }

        public void RemoveConnection(string userId, string connectionId)
        {
            lock (_users)
            {
                var user = _users.FirstOrDefault(item => item.Id == userId);
                if (user != null)
                {
                    user.ConnectionIds.Remove(connectionId);
                    if (user.ConnectionIds.Count == 0)
                        _users.Remove(user);
                }
            }
        }

        public IEnumerable<User> GetUsers() => _users.AsReadOnly();

        public User GetUser(string userId) => GetUsers().FirstOrDefault(user => user.Id == userId);
    }
}
