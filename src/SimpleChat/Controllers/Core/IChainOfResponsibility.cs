using SimpleChat.Controllers.Handlers;
using SimpleChat.Models;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Core
{
    public interface IChainOfResponsibility
    {
        void AddHandler(IHandler handler);

        Task<IAuthResult> RunAsync(IContext context);
    }

    // Для возможности легко замокать при тестировании.
    public interface IAuthenticationFlow : IChainOfResponsibility
    {
    }

    public interface ISignInFlow : IChainOfResponsibility
    {
    }

    public interface IConfirmSignInFlow : IChainOfResponsibility
    {
    }
}
