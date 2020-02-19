using SimpleChat.Controllers.Core;
using SimpleChat.Controllers.Handlers;
using SimpleChat.Infrastructure.Helpers;
using SimpleChat.Models;
using System.Threading.Tasks;

namespace SimpleChat.Tests.Unit.Handlers
{
    internal class HandlerUnderTest : HandlerBase
    {
        public HandlerUnderTest(IGuard guard) : base(guard)
        {
            _errors = new string[] { "error_1", "error_2", "error_3" };
        }

        public bool CanHandleReturns { get; set; }

        protected override bool CanHandle(IContext context)
        {
            return CanHandleReturns;
        }

        public IAuthResult InternalHandleAsyncReturns { get; set; }

        protected override Task<IAuthResult> InternalHandleAsync(IContext context)
        {
            return Task.FromResult(InternalHandleAsyncReturns);
        }
    }
}
