using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AutoMapper;
using Newtonsoft.Json;
using Zxw.Framework.NetCore.Cache;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Helpers;

namespace Zxw.Framework.NetCore.Attributes
{
    /// <summary>
    /// 缓存属性。
    /// <para>
    /// 在方法上标记此属性后，通过该方法取得的数据将被缓存。在缓存有效时间范围内，往后通过此方法取得的数据都是从缓存中取出的。
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CachedAttribute : AbstractInterceptorAttribute
    {
        /// <summary>
        /// 缓存有限期，单位：秒。默认值：600。
        /// </summary>
        public int Expiration { get; set; } = 10 * 60;

        public string CacheKey { get; set; } = null;

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            var cache = (IDistributedCacheManager)context.ServiceProvider.GetService(typeof(IDistributedCacheManager));
            var parameters = context.ServiceMethod.GetParameters();
            //判断Method是否包含ref / out参数
            if (parameters.Any(it => it.IsIn || it.IsOut))
            {
                await next(context);
            }
            else
            {
                var key = string.IsNullOrEmpty(CacheKey)
                    ? new CacheKey(context.ServiceMethod, parameters, context.Parameters).GetRedisCacheKey()
                    : CacheKey;
                var cachedValue = await cache.GetAsync(key);
                if (cachedValue == null)
                {
                    await context.Invoke(next);
                    dynamic returnValue = context.ReturnValue;

                    if (context.ServiceMethod.IsReturnTask())
                    {
                        returnValue = returnValue.Result;
                    }

                    await cache.SetAsync(CacheKey, returnValue, Expiration);
                }
                else
                {
                    if (context.ServiceMethod.IsReturnTask())
                    {
                        context.ReturnValue = Task.FromResult(cachedValue);
                    }
                    else
                    {
                        context.ReturnValue = cachedValue;
                    }
                }
            }
        }
    }
}
