using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.Helpers;
using System.Linq;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.Attributes
{
    /// <summary>
    /// 缓存属性。
    /// <para>
    /// 在方法上标记此属性后，通过该方法取得的数据将被缓存。在缓存有效时间范围内，往后通过此方法取得的数据都是从缓存中取出的。
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MemoryCacheAttribute : AbstractInterceptorAttribute
    {
        //[FromContainer]
        private readonly IMemoryCache _cache = AutofacContainer.Resolve<IMemoryCache>();

        //[FromContainer]
        private readonly IOptions<MemoryCacheEntryOptions> _optionsAccessor = AutofacContainer.Resolve<IOptions<MemoryCacheEntryOptions>>();


        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            var parameters = context.ServiceMethod.GetParameters();
            //判断Method是否包含ref / out参数
            if (parameters.Any(it => it.IsIn || it.IsOut))
            {
                await next(context);
            }
            else
            {
                var key = new CacheKey(context.ServiceMethod, parameters);
                if (_cache.TryGetValue(key, out object value))
                {
                    context.ReturnValue = value;
                }
                else
                {
                    await context.Invoke(next);
                    _cache.Set(key, context.ReturnValue, _optionsAccessor.Value);
                    await next(context);
                }                
            }
        }
    }
}
