using System;

namespace SimpleChat.Infrastructure.Helpers
{
    /// <summary>
    /// Представляет собой компонент, инкапсулирующий логику валидации входных параметром методов, согласно технике
    /// защитного программирования.
    /// </summary>
    public interface IGuard
    {
        /// <summary>
        /// Проверяет, что строковое значение входного параметра метода не равно пустой строке.
        /// </summary>
        /// <param name="value">Проверяемое строковое значение.</param>
        /// <param name="paramName">Имя проверяемого параметра метода.</param>
        /// <returns>Исходное проверяемое строковое значение.</returns>
        /// <exception cref="ArgumentException"/>
        string EnsureStringParamIsNotNullOrEmpty(string value, string paramName);

        /// <summary>
        /// Проверяет, что значение ссылочного типа <typeparamref name="T"/> параметра метода не равно null.
        /// </summary>
        /// <typeparam name="T">Тип данных проверяемого значения.</typeparam>
        /// <param name="value">Проверяемое значение типа <typeparamref name="T"/>.</param>
        /// <param name="paramName">Имя проверяемого параметра метода.</param>
        /// <returns>Исходное проверяемое значение типа <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException"/>
        T EnsureObjectParamIsNotNull<T>(T value, string paramName);

        /// <summary>
        /// Проверяет, что строковое значение свойства не равно пустой строке.
        /// </summary>
        /// <param name="value">Проверяемое строковое значение свойства.</param>
        /// <param name="errorMessage">
        /// Сообщение об ошибке, помещаемое в исключение типа <see cref="InvalidOperationException"/> в случае, если
        /// проверяемое значение равно пустой строке.
        /// </param>
        /// <returns>Исходное проверяемое строковое значение.</returns>
        /// <exception cref="InvalidOperationException"/>
        string EnsureStringPropertyIsNotNullOrEmpty(string value, string errorMessage);
    }
}
