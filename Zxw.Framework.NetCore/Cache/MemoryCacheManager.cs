using Microsoft.Extensions.Caching.Memory;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.Cache
{
    public class MemoryCacheManager
    {
        public static IMemoryCache GetInstance() => ServiceLocator.Resolve<IMemoryCache>();
    }
}
