using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.Website.Controllers.Filters;

namespace Zxw.Framework.Website.Controllers
{
    [RequestFilter]
    public abstract class BaseController : Controller
    {
    }
}