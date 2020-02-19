using SimpleChat.Controllers.Core;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    /// <summary>
    /// Абстрактный базовый класс обработчика, реализующий интерфейс <see cref="IHandler"/>.
    /// </summary>
    /// <inheritdoc cref="IHandler"/>
    public abstract class HandlerBase : IHandler
    {
        protected IGuard _guard;
        protected ICollection<string> _errors = new List<string>();

        public virtual IHandler Next { get; set; }

        public HandlerBase() : this(null)
        {
        }

        public HandlerBase(IGuard guard)
        {
            _guard = guard ?? new Guard();
        }

        public virtual async Task<IAuthResult> HandleAsync(IContext context)
        {
            _guard.EnsureObjectParamIsNotNull(context, nameof(context));

            IAuthResult handleResult = null;
            bool canHandle = CanHandle(context);
            if (canHandle)
                handleResult = await InternalHandleAsync(context);
            else
                throw new InvalidOperationException(GetErrorMessage());

            return await (handleResult == null ? Next?.HandleAsync(context) : Task.FromResult(handleResult));
        }

        /// <summary>
        /// Склеивает в одну строку и возвращает все сообщения об ошибках, разделённые символами перевода на новую
        /// строку.
        /// </summary>
        protected virtual string GetErrorMessage()
        {
            return _errors.Aggregate(new StringBuilder(), (builder, error) => builder.AppendLine(error)).ToString();
        }

        /// <summary>
        /// Асинхронно выполняет обработку данных, хранящихся в контексте <see cref="IContext"/>, специфичную для
        /// каждого производного класса.
        /// </summary>
        /// <inheritdoc cref="IHandler.HandleAsync"/>
        protected abstract Task<IAuthResult> InternalHandleAsync(IContext context);

        /// <summary>
        /// Показывает, может ли конкретный обработчик выполнить обработку данных контекста.
        /// </summary>
        /// <param name="context">Контекст выполняемой операции.</param>
        /// <returns>true, если данные контекста пригодны для обработки конкретным обработчиком; иначе false</returns>
        protected abstract bool CanHandle(IContext context);
    }
}
