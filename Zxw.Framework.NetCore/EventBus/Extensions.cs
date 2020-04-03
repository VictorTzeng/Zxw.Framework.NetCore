using System;
using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using Microsoft.Extensions.DependencyInjection;
using Zxw.Framework.NetCore.Transaction;

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
            services.AddSingleton<IConsumerServiceSelector, DefaultConsumerServiceSelector>();
            services.AddSingleton<IEventPublisher, DefaultEventPublisher>();
            services.AddScoped<IScopedTransaction, DefaultCapScopedTransaction>();

            return services;
        }
    }
}
