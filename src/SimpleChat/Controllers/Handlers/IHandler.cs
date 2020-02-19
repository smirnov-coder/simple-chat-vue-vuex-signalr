using SimpleChat.Controllers.Core;
using SimpleChat.Models;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Handlers
{
    /// <summary>
    /// Инкапсулирует логику частичной обработки данных, хранящихся в контексте <see cref="IContext"/>.
    /// </summary>
    public interface IHandler
    {
        /// <summary>
        /// Следующий обработчик в цепочке обработчиков <see cref="IChainOfResponsibility"/>.
        /// </summary>
        IHandler Next { get; set; }

        /// <summary>
        /// Асинхронно выполняет частичную обработку данных, хранящихся в контексте.
        /// </summary>
        /// <param name="context">Контекст выполняемой операции.</param>
        Task<IAuthResult> HandleAsync(IContext context);
    }
}
