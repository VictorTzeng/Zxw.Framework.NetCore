using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.Website.IRepositories;

namespace Zxw.Framework.Website.Controllers.Filters
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RequestFilter:ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var repository= (ISysMenuRepository)context.HttpContext.RequestServices.GetService(typeof(ISysMenuRepository));
            var identity = context.RouteData.Values["controller"] + "/" + context.RouteData.Values["action"];
            if (repository.Count(m => m.Identity.Equals(identity, StringComparison.OrdinalIgnoreCase) && m.Activable) <= 0)
            {
                if (context.HttpContext.Request.IsAjaxRequest())
                {
                    context.Result = new JsonResult(new { success = false, msg = "您请求的地址不存在，或者已被停用." });
                    context.HttpContext.Response.StatusCode = HttpStatusCode.NotFound.GetHashCode();
                }
                else
                {
                    context.Result = new ViewResult(){ViewName = "NotFound"};
                    context.HttpContext.Response.StatusCode = HttpStatusCode.NotFound.GetHashCode();
                }
            }
            base.OnActionExecuting(context);
        }
    }
}
