using Microsoft.AspNetCore.Http;
using System;
using Zxw.Framework.NetCore.Extensions;

namespace Zxw.Framework.NetCore.Web
{
    public class WebContext : IWebContext
    {
        public HttpContext CoreContext { get; }

        public WebContext(IHttpContextAccessor accessor)
        {
            CoreContext = accessor?.HttpContext ?? throw new ArgumentNullException($"参数{nameof(accessor)}为null，请先在Startup.cs文件中的ConfigServices方法里使用AddHttpContextAccessor注入IHttpContextAccessor对象。");
        }

        public virtual T GetService<T>()
        {
            return CoreContext.GetService<T>();
        }
    }
}
