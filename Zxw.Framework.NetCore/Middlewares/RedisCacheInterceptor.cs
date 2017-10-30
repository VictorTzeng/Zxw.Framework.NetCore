using Dora.Interception;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Helpers;

namespace Zxw.Framework.NetCore.Middlewares
{
    public class RedisCacheInterceptor
    {
        private readonly InterceptDelegate _next;
        public RedisCacheInterceptor(InterceptDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(InvocationContext context, IDistributedCache cache, IOptions<DistributedCacheEntryOptions> optionsAccessor)
        {
            //判断Method是否包含ref/out参数
            if (!context.Method.GetParameters().All(it => it.IsIn))
            {
                await _next(context);
            }

            var key = new CacheKey(context.Method, context.Arguments).GetHashCode().ToString();
            var value = cache.Get(key);
            if (value!=null)
            {
                context.ReturnValue = value.ToObject();
            }
            else
            {
                await _next(context);
                cache.Set(key, context.ReturnValue.ToBytes(), optionsAccessor.Value);
            }
        }
    }
}
