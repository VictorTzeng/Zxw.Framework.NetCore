using Microsoft.AspNetCore.Http;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.Web
{
    public interface IWebContext: ISingletonDependency
    {
        HttpContext CoreContext { get; }
        T GetService<T>();
    }
}
