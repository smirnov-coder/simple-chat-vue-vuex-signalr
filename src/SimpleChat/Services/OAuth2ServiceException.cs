using System;
using System.Runtime.Serialization;

namespace SimpleChat.Services
{
    /// <summary>
    /// Представляет собой ошибку, возникшую в процессе работы OAuth2-сервиса <see cref="IOAuth2Service"/>.
    /// </summary>
    [Serializable]
    public class OAuth2ServiceException : Exception
    {
        public OAuth2ServiceException() { }

        public OAuth2ServiceException(string message) : base(message) { }

        public OAuth2ServiceException(string message, Exception inner) : base(message, inner) { }

        protected OAuth2ServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
