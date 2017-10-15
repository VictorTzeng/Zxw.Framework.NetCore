using Microsoft.AspNetCore.Mvc.Filters;
using Zxw.Framework.NetCore.Helpers;

namespace Zxw.Framework.NetCore.Filters
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            var type = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
            Log4NetHelper.WriteError(type, filterContext.Exception);
        }
    }
}
