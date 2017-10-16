using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using log4net;
using Zxw.Framework.Helpers;

namespace Zxw.Framework.Filters
{
    public abstract class GlobalExceptionFilter : IExceptionFilter
    {
        public virtual void OnException(ExceptionContext filterContext)
        {
            var type = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;
            Log4NetHelper.WriteError(type, filterContext.Exception);
        }
    }
}
