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
    public abstract class Handler : IHandler
    {
        protected IGuard _guard;
        protected ICollection<string> _errors = new List<string>();

        public virtual IHandler Next { get; set; }

        public Handler() : this(null)
        {
        }

        public Handler(IGuard guard)
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

        protected virtual string GetErrorMessage()
        {
            return _errors.Aggregate(new StringBuilder(), (builder, error) => builder.AppendLine(error)).ToString();
        }

        protected abstract Task<IAuthResult> InternalHandleAsync(IContext context);

        protected abstract bool CanHandle(IContext context);
    }
}
