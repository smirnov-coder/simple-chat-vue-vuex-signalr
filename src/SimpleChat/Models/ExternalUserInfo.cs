using System;

namespace SimpleChat.Models
{
    public class ExternalUserInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Picture { get; set; }

        public string Provider { get; set; }

        public string AccessToken { get; set; }
    }
}
