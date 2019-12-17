using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zxw.Framework.NetCore.Extensions
{
    public static class MvcExtension
    {
        public static T GetService<T>(this HttpContext httpContext) => (T)httpContext.RequestServices.GetService(typeof(T));
        public static T GetService<T>(this ActionContext actionContext) => actionContext.HttpContext.GetService<T>();
        public static T GetService<T>(this Controller controller) => controller.HttpContext.GetService<T>();
    }
}
