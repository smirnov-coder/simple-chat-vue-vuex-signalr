using SimpleChat.Controllers.Handlers;
using SimpleChat.Models;
using System.Threading.Tasks;

namespace SimpleChat.Controllers.Core
{
    /// <summary>
    /// Реализация классического паттерна "Цепочка обязанностей". Позволяет создать цепочку операций (шагов),
    /// выполняемых при обработке запроса к <see cref="AuthController"/>.
    /// </summary>
    public interface IChainOfResponsibility
    {
        /// <summary>
        /// Добавляет обработчик в цепочку обработчиков.
        /// </summary>
        /// <param name="handler">Обработчик запроса (операция).</param>
        void AddHandler(IHandler handler);

        /// <summary>
        /// Асинхронно запускает выполнение цепочки обработчиков.
        /// </summary>
        /// <param name="context">Контекст запроса к <see cref="AuthController"/>.</param>
        Task<IAuthResult> RunAsync(IContext context);
    }
}
