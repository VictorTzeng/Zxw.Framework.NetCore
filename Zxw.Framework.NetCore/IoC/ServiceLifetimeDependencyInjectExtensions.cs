using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Zxw.Framework.NetCore.Extensions;
using Zxw.Framework.NetCore.Helpers;

namespace Zxw.Framework.NetCore.IoC
{
    public static class ServiceLifetimeDependencyInjectExtensions
    {
        public static IServiceCollection RegisterServiceLifetimeDependencies(this IServiceCollection services)
        {
            RuntimeHelper.GetAllAssemblies().ToList().ForEach(a =>
            {
                a.GetTypes().Where(t => t.IsClass).ToList().ForEach(t =>
                {
                    var serviceType = t.GetInterface($"I{t.Name}");
                    if ((serviceType??t).GetInterface(typeof(ISingletonDependency).Name)!=null)
                    {
                        if (serviceType != null)
                        {
                            services.AddSingleton(serviceType, t);
                        }
                        else
                        {
                            services.AddSingleton(t);
                        }
                    }
                    else if ((serviceType ?? t).GetInterface(typeof(IScopedDependency).Name)!=null)
                    {
                        if (serviceType != null)
                        {
                            services.AddScoped(serviceType, t);
                        }
                        else
                        {
                            services.AddScoped(t);
                        }
                    }
                    else if ((serviceType ?? t).GetInterface(typeof(ITransientDependency).Name)!=null)
                    {
                        if (serviceType != null)
                        {
                            services.AddTransient(serviceType, t);
                        }
                        else
                        {
                            services.AddTransient(t);
                        }
                    }
                });
            });
            return services;
        }
    }
}
