using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleChat.Infrastructure.Helpers
{
    public class UriHelper : IUriHelper
    {
        private IHttpContextAccessor _httpContextAccessor;
        private IGuard _guard;
        private LinkGenerator _linkGenerator;

        public UriHelper(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator)
            : this(httpContextAccessor, linkGenerator, null)
        {
        }

        public UriHelper(IHttpContextAccessor httpContextAccessor, LinkGenerator linkGenerator, IGuard guard)
        {
            _guard = guard ?? new Guard();
            _httpContextAccessor = _guard.EnsureObjectParamIsNotNull(httpContextAccessor, nameof(httpContextAccessor));
            _linkGenerator = _guard.EnsureObjectParamIsNotNull(linkGenerator, nameof(linkGenerator));
        }

        public string AddQueryString(string uri, IDictionary<string, string> queryParams)
        {
            _guard.EnsureObjectParamIsNotNull(queryParams, nameof(queryParams));

            return QueryHelpers.AddQueryString(uri, queryParams);
        }

        public string GetControllerActionUri(string controller, string action)
        {
            _guard.EnsureStringParamIsNotNullOrEmpty(controller, nameof(controller));
            _guard.EnsureStringParamIsNotNullOrEmpty(action, nameof(action));

            string scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            HostString host = _httpContextAccessor.HttpContext.Request.Host;
            return _linkGenerator.GetUriByAction(action, controller, null, scheme, host).ToLower();
        }
    }
}
