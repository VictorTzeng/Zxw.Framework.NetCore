using System;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.Caching.Distributed;
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
    public class RedisCacheAttribute : AbstractInterceptorAttribute
    {
        /// <summary>
        /// 缓存有限期，单位：分钟。默认值：10。
        /// </summary>
        public int Expiration { get; set; } = 10;

        public string CacheKey { get; set; } = null;

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
                var key = string.IsNullOrEmpty(CacheKey)
                    ? new CacheKey(context.ServiceMethod, parameters, context.Parameters).GetRedisCacheKey()
                    : CacheKey;
                var value =  await DistributedCacheManager.GetAsync(key);
                if (value != null)
                {
                    if (context.ServiceMethod.IsReturnTask())
                    {
                        context.ReturnValue = Task.FromResult(JsonConvert.DeserializeObject(value,
                            context.ServiceMethod.ReturnType.GenericTypeArguments[0]));
                    }
                    else
                    {
                        context.ReturnValue = JsonConvert.DeserializeObject(value, context.ServiceMethod.ReturnType);
                    }
                }
                else
                {
                    await context.Invoke(next);
                    object returnValue = context.ReturnValue;
                    if (context.ServiceMethod.IsReturnTask())
                    {
                        returnValue = returnValue.GetType().GetField("Result").GetValue(returnValue);
                    }
                    await DistributedCacheManager.SetAsync(key, returnValue, Expiration);
                    //await next(context);
                }
            }
        }
    }
}
