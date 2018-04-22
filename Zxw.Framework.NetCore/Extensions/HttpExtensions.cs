using System;
using Microsoft.AspNetCore.Http;

namespace Zxw.Framework.NetCore.Extensions
{
    public static class HttpExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request==null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.Headers.ContainsKey("X-Requested-With") &&
                   request.Headers["X-Requested-With"].Equals("XMLHttpRequest");
        }
    }
}
