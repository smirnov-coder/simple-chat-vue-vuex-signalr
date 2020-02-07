using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    public interface IUriHelper
    {
        string AddQueryString(string uri, IDictionary<string, string> queryParams);

        string GetControllerActionUri(string controller, string action);
    }
}
