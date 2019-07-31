using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleChat.Hubs
{
    public class UserCollection
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

        public User GetUser(string userId) => _users.FirstOrDefault(user => user.Id == userId);
    }

    public class User
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Avatar { get; set; }

        public string Provider { get; set; }

        public HashSet<string> ConnectionIds { get; set; } = new HashSet<string>();
    }
}
