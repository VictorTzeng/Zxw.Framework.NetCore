using System;
using AspectCore.Injector;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace Zxw.Framework.NetCore.IoC
{
    public static class ServiceLocator
    {
        public static IServiceProvider Current { get; set; }

        public static T Resolve<T>()
        {
            return (T)Current.GetService(typeof(T));
        }

        public static void BuildServiceLocator(this IServiceCollection services, ServiceProviderOptions options = null)
        {
            options ??= new ServiceProviderOptions();
            Current = services.BuildServiceProvider(options);
        }
    }
}
