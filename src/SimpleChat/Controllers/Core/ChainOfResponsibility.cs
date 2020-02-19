using SimpleChat.Controllers.Handlers;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Core
{
    /// <inheritdoc cref="IChainOfResponsibility"/>
    public class ChainOfResponsibility : IChainOfResponsibility
    {
        protected List<IHandler> _handlers;
        protected IGuard _guard;

        public ChainOfResponsibility() : this(null)
        {
        }

        public ChainOfResponsibility(IGuard guard = null)
        {
            _guard = guard ?? new Guard();
            _handlers = new List<IHandler>();
        }
        
        public virtual void AddHandler(IHandler handler)
        {
            _guard.EnsureObjectParamIsNotNull(handler, nameof(handler));

            if (_handlers.Any())
                _handlers.Last().Next = handler;
            _handlers.Add(handler);
        }
        
        public virtual async Task<IAuthResult> RunAsync(IContext context)
        {
            _guard.EnsureObjectParamIsNotNull(context, nameof(context));
            if (!_handlers.Any())
                throw new InvalidOperationException("Коллекция обработчиков пуста.");

            IAuthResult result = null;
            try
            {
                result = await _handlers.First().HandleAsync(context);
            }
            catch (Exception ex)
            {
                // Надо бы залогировать ошибку.
                result = new ErrorResult("Что-то пошло не так. Повторите попытку позднее или обратитесь " +
                    "к администратору.");
            }
            return result;
        }
    }
}
