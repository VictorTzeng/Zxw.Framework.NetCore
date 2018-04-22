using Microsoft.Extensions.Caching.Distributed;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.Helpers
{
    public class DistributedCacheHelper
    {
        public static IDistributedCache GetInstance() => AspectCoreContainer.Resolve<IDistributedCache>();
    }
}
