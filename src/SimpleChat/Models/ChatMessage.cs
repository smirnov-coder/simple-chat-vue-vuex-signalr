using System;

namespace SimpleChat.Models
{
    public class ChatMessage
    {
        public Author Author { get; set; }

        public string Text { get; set; }
    }

    public class Author
    {
        public string Name { get; set; }

        public string Avatar { get; set; }
    }
}
