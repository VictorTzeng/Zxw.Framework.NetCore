using System;
using DotNetCore.CAP;
using DotNetCore.CAP.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            //services.Replace(new ServiceDescriptor(typeof(ICapTransaction), typeof(DefaultCapDbTransaction)));
            //services.AddSingleton<IConsumerServiceSelector, DefaultConsumerServiceSelector>();
            //services.AddSingleton<IEventPublisher, DefaultEventPublisher>();
            //services.AddScoped<IScopedTransaction, DefaultCapScopedTransaction>();
            services.AddCap(capAction);
            return services;
        }
    }
}
