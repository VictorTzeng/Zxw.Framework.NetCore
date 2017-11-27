using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Senparc.Weixin.MP;

namespace Zxw.Framework.NetCore.WeChat
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
    public class CheckLoginAttribute:ActionFilterAttribute
    {
        public OAuthScope Scope;
        public string RedirectUrl;

        public CheckLoginAttribute(OAuthScope scope = OAuthScope.snsapi_base, string redirectUrl = null)
        {
            Scope = scope;
            RedirectUrl = redirectUrl;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var type = context.ActionDescriptor.GetType();
            type.GetCustomAttributes(typeof(AllowAnonymousAttribute), true);
            base.OnActionExecuting(context);
        }
    }
}
