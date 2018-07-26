using System;
using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using AspectCore.Injector;
using Microsoft.Extensions.DependencyInjection;

namespace Zxw.Framework.NetCore.IoC
{
    public class AspectCoreContainer
    {
        private static IServiceResolver resolver { get; set; }
        public static IServiceProvider BuildServiceProvider(IServiceCollection services, Action<IAspectConfiguration> configure = null)
        {
            if(services==null)throw new ArgumentNullException(nameof(services));
            services.AddDynamicProxy(configure);
            services.AddAspectCoreContainer();
            return resolver = services.ToServiceContainer().Build();
        }

        public static T Resolve<T>()
        {
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver), "调用此方法时必须先调用BuildServiceProvider！");
            return resolver.Resolve<T>();
        }
    }
}
