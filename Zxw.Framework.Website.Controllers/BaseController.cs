using System.Net.Http;
using AspectCore.Injector;
using Butterfly.Client.Tracing;
using Microsoft.AspNetCore.Mvc;
using Zxw.Framework.Website.Controllers.Filters;

namespace Zxw.Framework.Website.Controllers
{
    [RequestFilter]
    public abstract class BaseController : Controller
    {
        [FromContainer]
        public IServiceTracer Tracer{get;set;}

        [FromContainer]
        public HttpClient HttpClient{get;set;}
    }
}