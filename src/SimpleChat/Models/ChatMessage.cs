using SimpleChat.Hubs;

namespace SimpleChat.Models
{
    /// <summary>
    /// Сообщение чата <see cref="ChatHub/>.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Автор сообщения.
        /// </summary>
        public Author Author { get; set; }

        /// <summary>
        /// Текст сообщения.
        /// </summary>
        public string Text { get; set; }
    }

    /// <summary>
    /// Автор сообщения чата <see cref="ChatMessage"/>.
    /// </summary>
    public class Author
    {
        /// <summary>
        /// Полное имя пользователя.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Аватар пользователя (веб-путь к файлу изображения).
        /// </summary>
        public string Avatar { get; set; }
    }
}
