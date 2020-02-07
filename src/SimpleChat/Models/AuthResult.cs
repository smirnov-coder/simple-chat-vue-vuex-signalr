using System;
using System.Collections.Generic;

namespace SimpleChat.Models
{
    public interface IAuthResult
    {
        string Type { get; }
    }

    public class NotAuthenticatedResult : IAuthResult
    {
        public string Type { get; } = "auth_check";

        public virtual bool IsAuthenticated { get; } = false;
    }

    public class AuthenticatedResult : NotAuthenticatedResult
    {
        public override bool IsAuthenticated { get; } = true;

        public UserInfo User { get; set; }
    }

    public class EmailRequiredResult : IAuthResult
    {
        public string Type { get; } = "email_required";

        public string Message { get; set; }

        public EmailRequiredResult(string provider)
        {
            Message = $"Для того, чтобы войти на наш сайт, необходимо предоставить доступ к адресу электронной " +
                $"почты Вашего аккаунта в социальной сети '{provider}'.";
        }
    }

    public class ConfirmSignInResult : IAuthResult
    {
        public string Type { get; } = "confirm_sign_in";

        public string SessionId { get; set; }

        public string Email { get; set; }

        public string Provider { get; set; }

        public ConfirmSignInResult(string sessionId, string email, string provider)
        {
            SessionId = sessionId;
            Email = email;
            Provider = provider;
        }
    }

    public class ExternalLoginErrorResult : ErrorResult
    {
        public override string Type { get; } = "external_login_error";

        public ExternalLoginErrorResult(string message, IEnumerable<string> errors = null)
            : base(message, errors) { }
    }

    public class SignInSuccessResult : IAuthResult
    {
        public string Type { get; } = "success";

        public string AccessToken { get; set; }

        public SignInSuccessResult()
        {
        }

        public SignInSuccessResult(string accessToken) => AccessToken = accessToken;
    }

    public class ErrorResult : IAuthResult
    {
        public virtual string Type { get; } = "error";

        public string Message { get; set; }

        public IEnumerable<string> Errors { get; set; }

        public ErrorResult(string message, IEnumerable<string> errors = null)
        {
            Message = message;
            Errors = errors ?? new List<string>();
        }
    }
}
