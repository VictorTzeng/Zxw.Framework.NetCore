using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Zxw.Framework.NetCore.Models;
using Zxw.Framework.NetCore.Web;

namespace Zxw.Framework.NetCore.Mvc
{
    public abstract class BaseController:Controller
    {
        protected readonly IWebContext WebContext;
        public BaseController(IWebContext webContext)
        {
            WebContext = webContext;
        }

        protected IActionResult Json2(object content, string dateFormat = "yyyy-MM-dd HH:mm:ss") => Json(content, new JsonSerializerSettings()
        {
            DateFormatString = dateFormat,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });

        protected IActionResult SucceedJson(string msg)
        {
            return Json2(ExcutedResult.SuccessResult(msg: msg));
        }
        protected IActionResult SucceedJson(object rows)
        {
            return Json2(ExcutedResult.SuccessResult(rows: rows));
        }

        protected IActionResult FailedJson(string msg, int? code = null)
        {
            return Json2(ExcutedResult.FailedResult(msg, code));
        }
    }
}
