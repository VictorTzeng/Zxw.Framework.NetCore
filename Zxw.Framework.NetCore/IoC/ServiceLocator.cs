using System;
using AspectCore.Extensions.DependencyInjection;
using AspectCore.Injector;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Zxw.Framework.NetCore.Extensions;

namespace Zxw.Framework.NetCore.IoC
{
    public static class ServiceLocator
    {
        internal static IServiceProvider Current { get; set; }

        public static T Resolve<T>()
        {
            return (T)Current.GetService(typeof(T));
        }
    }
}
