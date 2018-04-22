using Microsoft.Extensions.Caching.Memory;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.Helpers
{
    public class MemoryCacheHelper
    {
        public static IMemoryCache GetInstance() => AspectCoreContainer.Resolve<IMemoryCache>();
    }
}
