using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Hubs
{
    public class User
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Avatar { get; set; }

        public string Provider { get; set; }

        public HashSet<string> ConnectionIds { get; set; } = new HashSet<string>();
    }
}
