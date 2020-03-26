using System;
using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Zxw.Framework.NetCore.EventBus
{
    public static class Extensions
    {
        /// <summary>
        /// 增加CAP事件总线
        /// </summary>
        /// <param name="services"></param>
        /// <param name="capAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddEventBus(this IServiceCollection services, Action<CapOptions> capAction)
        {
            services.AddCap(capAction);
            // 替换cap默认的消费者服务查找器
            services.AddSingleton<IConsumerServiceSelector, DefaultConsumerServiceSelector>();
            return services;
        }
    }
}
