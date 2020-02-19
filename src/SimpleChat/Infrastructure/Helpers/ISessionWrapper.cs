using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    /// <summary>
    /// Представляет собой обёртку над <see cref="Microsoft.AspNetCore.Http.ISession"/> для возможности мокать вызовы
    /// методов расширения класса <see cref="Microsoft.AspNetCore.Http.ISession"/>.
    /// </summary>
    public interface ISessionWrapper
    {
        /// <inheritdoc cref="Microsoft.AspNetCore.Http.ISession.IsAvailable"/>
        bool IsAvailable { get; }

        /// <inheritdoc cref="Microsoft.AspNetCore.Http.ISession.Keys"/>
        IEnumerable<string> Keys { get; }

        /// <inheritdoc cref="Microsoft.AspNetCore.Http.ISession.LoadAsync"/>
        Task LoadAsync();

        /// <summary>
        /// Gets a string value from current session by key.
        /// </summary>
        /// <param name="key">Key to get string value from current session.</param>
        string GetString(string key);

        /// <summary>
        /// Store the string value in the current session by key.
        /// </summary>
        /// <param name="key">Key to store string value in current session.</param>
        /// <param name="value">String value to store.</param>
        void SetString(string key, string value);

        /// <inheritdoc cref="Microsoft.AspNetCore.Http.ISession.CommitAsync"/>
        Task CommitAsync();

        /// <inheritdoc cref="Microsoft.AspNetCore.Http.ISession.Clear"/>
        void Clear();
    }
}
