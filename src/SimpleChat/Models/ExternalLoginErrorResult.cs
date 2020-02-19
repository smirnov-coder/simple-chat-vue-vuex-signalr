using System.Collections.Generic;

namespace SimpleChat.Models
{
    /// <summary>
    /// Результат ошибки обращения к API внешнего OAuth2-провайдера.
    /// </summary>
    public class ExternalLoginErrorResult : ErrorResult
    {
        public override string Type { get; } = "external_login_error";

        public ExternalLoginErrorResult(string message, IEnumerable<string> errors = null)
            : base(message, errors) { }
    }
}
