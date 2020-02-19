using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    /// <summary>
    /// Вспомогательный класс для работы с JSON.
    /// </summary>
    public interface IJsonHelper
    {
        /// <summary>
        /// Производит парсинг JSON в виде строки и возвращает результат в виде объекта <see cref="JObject"/>,
        /// пригодного для выполнения запросов LINQ to JSON.
        /// </summary>
        /// <param name="json">JSON в виде строки.</param>
        JObject Parse(string json);

        /// <summary>
        /// Сериализует объект типа <typeparamref name="T"/> в JSON в виде строки <see cref="string"/>.
        /// </summary>
        /// <typeparam name="T">Тип данных сериализуемого объекта.</typeparam>
        /// <param name="value">Сериализуемый объект типа <typeparamref name="T"/>.</param>
        /// <returns>JSON в виде строки <see cref="string"/>.</returns>
        string SerializeObject<T>(T value);

        /// <summary>
        /// Десериализует JSON в виде строки <see cref="string"/> и возвращает результат в виде объекта типа
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Тип данных результата десериализации.</typeparam>
        /// <param name="json">JSON в виде строки <see cref="string"/>.</param>
        /// <returns>Объект типа <typeparamref name="T"/>.</returns>
        T DeserializeObject<T>(string json);
    }
}
