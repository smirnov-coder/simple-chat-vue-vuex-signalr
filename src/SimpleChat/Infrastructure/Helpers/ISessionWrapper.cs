using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    public interface ISessionWrapper
    {
        bool IsAvailable { get; }

        IEnumerable<string> Keys { get; }

        Task LoadAsync();

        string GetString(string key);

        void SetString(string key, string value);

        Task CommitAsync();

        void Clear();
    }
}
