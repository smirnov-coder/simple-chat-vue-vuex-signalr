using System.Collections.Generic;

namespace SimpleChat.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Добавляет новое или обновляет существующее в словаре <see cref="IDictionary{TKey, TValue}"/> значение типа
        /// <typeparamref name="TValue"/> по заданному ключу типа <typeparamref name="TKey"/>.
        /// </summary>
        /// <typeparam name="TKey">
        /// Тип данных значения ключа словаря <see cref="IDictionary{TKey, TValue}"/>.
        /// </typeparam>
        /// <typeparam name="TValue">Тип данных, хранимых в словаре <see cref="IDictionary{TKey, TValue}"/>.</typeparam>
        /// <param name="key">
        /// Значение ключа, по которому доступны данные типа <typeparamref name="TValue"/> в словаре
        /// <see cref="IDictionary{TKey, TValue}"/>.
        /// </param>
        /// <param name="value">Значение данных типа <typeparamref name="TValue"/> для обновления или добавления по
        /// заданному ключу типа <typeparamref name="TKey"/>.</param>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }
    }
}
