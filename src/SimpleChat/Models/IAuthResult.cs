using System;

namespace SimpleChat.Models
{
    /// <summary>
    /// Представляет собой результат метода действия контроллера <see cref="Controllers.AuthController"/>.
    /// </summary>
    public interface IAuthResult
    {
        /// <summary>
        /// Строковое представление типа результата.
        /// </summary>
        string Type { get; }
    }
}
