using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с MD5.
    /// </summary>
    public interface IMD5Hasher
    {
        /// <summary>
        /// Вычисляет MD5-хеш объекта типа <see cref="string"/>.
        /// </summary>
        /// <param name="source">Объект для вычисления MD5-хеша.</param>
        /// <returns>MD5-хеш в виде шестнадцатиричной строки <see cref="string"/>.</returns>
        string ComputeHash(string source);
    }
}
