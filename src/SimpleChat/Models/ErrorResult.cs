using System.Collections.Generic;

namespace SimpleChat.Models
{
    /// <summary>
    /// Результат ошибки, возникшей в ходе выполнения запроса к <see cref="Controllers.AuthController"/>.
    /// </summary>
    public class ErrorResult : IAuthResult
    {
        public virtual string Type { get; } = "error";

        /// <summary>
        /// Дружественное к пользователю сообщение об ошибке.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Коллекция сообщений о внутренних ошибках.
        /// </summary>
        public IEnumerable<string> Errors { get; set; }

        public ErrorResult(string message, IEnumerable<string> errors = null)
        {
            Message = message;
            Errors = errors ?? new List<string>();
        }
    }
}
