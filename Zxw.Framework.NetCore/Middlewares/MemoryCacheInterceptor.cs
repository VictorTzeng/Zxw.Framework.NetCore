using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dora.Interception;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Linq;
using Zxw.Framework.NetCore.Helpers;

namespace Zxw.Framework.NetCore.Middlewares
{
    public class MemoryCacheInterceptor
    {
        private readonly InterceptDelegate _next;
        public MemoryCacheInterceptor(InterceptDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(InvocationContext context, IMemoryCache cache, IOptions<MemoryCacheEntryOptions> optionsAccessor)
        {
            //判断Method是否包含ref/out参数
            if (!context.Method.GetParameters().All(it => it.IsIn))
            {
                await _next(context);
            }

            var key = new CacheKey(context.Method, context.Arguments);
            if (cache.TryGetValue(key, out object value))
            {
                context.ReturnValue = value;
            }
            else
            {
                await _next(context);
                cache.Set(key, context.ReturnValue, optionsAccessor.Value);
            }
        }
    }
}
