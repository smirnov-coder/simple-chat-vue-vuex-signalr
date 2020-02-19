namespace SimpleChat.Models
{
    /// <summary>
    /// Результат успешного входа на сайт через внешний OAuth2-провайдер.
    /// </summary>
    public class SignInSuccessResult : IAuthResult
    {
        public string Type { get; } = "success";

        /// <summary>
        /// Маркер доступа для выполнения запросов к API.
        /// </summary>
        public string AccessToken { get; set; }

        public SignInSuccessResult()
        {
        }

        public SignInSuccessResult(string accessToken) => AccessToken = accessToken;
    }
}
