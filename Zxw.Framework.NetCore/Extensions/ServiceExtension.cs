using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using AspectCore.Injector;
using CSRedis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.NetCore.IoC;

namespace Zxw.Framework.NetCore.Extensions
{
    /// <summary>
    /// IServiceCollection扩展
    /// </summary>
    public static class ServiceExtension
    {
        /// <summary>
        /// 用DI批量注入接口程序集中对应的实现类。
        /// <para>
        /// 需要注意的是，这里有如下约定：
        /// IUserService --> UserService, IUserRepository --> UserRepository.
        /// </para>
        /// </summary>
        /// <param name="service"></param>
        /// <param name="interfaceAssemblyName">接口程序集的名称（不包含文件扩展名）</param>
        /// <returns></returns>
        public static IServiceCollection AddTransientAssembly(this IServiceCollection service, string interfaceAssemblyName)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            if (string.IsNullOrEmpty(interfaceAssemblyName))
                throw new ArgumentNullException(nameof(interfaceAssemblyName));

            var assembly = RuntimeHelper.GetAssembly(interfaceAssemblyName);
            if (assembly == null)
            {
                throw new DllNotFoundException($"the dll \"{interfaceAssemblyName}\" not be found");
            }

            //过滤掉非接口及泛型接口
            var types = assembly.GetTypes().Where(t => t.GetTypeInfo().IsInterface && !t.GetTypeInfo().IsGenericType);

            foreach (var type in types)
            {
                var implementTypeName = type.Name.Substring(1);
                var implementType = RuntimeHelper.GetImplementType(implementTypeName, type);
                if (implementType != null)
                    service.AddTransient(type, implementType);
            }
            return service;
        }
        public static IServiceCollection AddScopedAssembly(this IServiceCollection service, string interfaceAssemblyName)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            if (string.IsNullOrEmpty(interfaceAssemblyName))
                throw new ArgumentNullException(nameof(interfaceAssemblyName));

            var assembly = RuntimeHelper.GetAssembly(interfaceAssemblyName);
            if (assembly == null)
            {
                throw new DllNotFoundException($"the dll \"{interfaceAssemblyName}\" not be found");
            }

            //过滤掉非接口及泛型接口
            var types = assembly.GetTypes().Where(t => t.GetTypeInfo().IsInterface && !t.GetTypeInfo().IsGenericType);

            foreach (var type in types)
            {
                var implementTypeName = type.Name.Substring(1);
                var implementType = RuntimeHelper.GetImplementType(implementTypeName, type);
                if (implementType != null)
                    service.AddScoped(type, implementType);
            }
            return service;
        }
        public static IServiceCollection AddSingletonAssembly(this IServiceCollection service, string interfaceAssemblyName)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            if (string.IsNullOrEmpty(interfaceAssemblyName))
                throw new ArgumentNullException(nameof(interfaceAssemblyName));

            var assembly = RuntimeHelper.GetAssembly(interfaceAssemblyName);
            if (assembly == null)
            {
                throw new DllNotFoundException($"the dll \"{interfaceAssemblyName}\" not be found");
            }

            //过滤掉非接口及泛型接口
            var types = assembly.GetTypes().Where(t => t.GetTypeInfo().IsInterface && !t.GetTypeInfo().IsGenericType);

            foreach (var type in types)
            {
                var implementTypeName = type.Name.Substring(1);
                var implementType = RuntimeHelper.GetImplementType(implementTypeName, type);
                if (implementType != null)
                    service.AddSingleton(type, implementType);
            }
            return service;
        }
        /// <summary>
        /// 用DI批量注入接口程序集中对应的实现类。
        /// </summary>
        /// <param name="service"></param>
        /// <param name="interfaceAssemblyName">接口程序集的名称（不包含文件扩展名）</param>
        /// <param name="implementAssemblyName">实现程序集的名称（不包含文件扩展名）</param>
        /// <returns></returns>
        public static IServiceCollection AddScopedAssembly(this IServiceCollection service, string interfaceAssemblyName, string implementAssemblyName)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            if(string.IsNullOrEmpty(interfaceAssemblyName))
                throw new ArgumentNullException(nameof(interfaceAssemblyName));
            if (string.IsNullOrEmpty(implementAssemblyName))
                throw new ArgumentNullException(nameof(implementAssemblyName));

            var interfaceAssembly = RuntimeHelper.GetAssembly(interfaceAssemblyName);
            if (interfaceAssembly == null)
            {
                throw new DllNotFoundException($"the dll \"{interfaceAssemblyName}\" not be found");
            }

            var implementAssembly = RuntimeHelper.GetAssembly(implementAssemblyName);
            if (implementAssembly == null)
            {
                throw new DllNotFoundException($"the dll \"{implementAssemblyName}\" not be found");
            }

            //过滤掉非接口及泛型接口
            var types = interfaceAssembly.GetTypes().Where(t => t.GetTypeInfo().IsInterface && !t.GetTypeInfo().IsGenericType);

            foreach (var type in types)
            {
                //过滤掉抽象类、泛型类以及非class
                var implementType = implementAssembly.DefinedTypes
                    .FirstOrDefault(t => t.IsClass && !t.IsAbstract && !t.IsGenericType &&
                                         t.GetInterfaces().Any(b => b.Name == type.Name));
                if (implementType != null)
                {
                    service.AddScoped(type, implementType.AsType());
                }
            }

            return service;
        }
        public static IServiceCollection AddTransientAssembly(this IServiceCollection service, string interfaceAssemblyName, string implementAssemblyName)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            if(string.IsNullOrEmpty(interfaceAssemblyName))
                throw new ArgumentNullException(nameof(interfaceAssemblyName));
            if (string.IsNullOrEmpty(implementAssemblyName))
                throw new ArgumentNullException(nameof(implementAssemblyName));

            var interfaceAssembly = RuntimeHelper.GetAssembly(interfaceAssemblyName);
            if (interfaceAssembly == null)
            {
                throw new DllNotFoundException($"the dll \"{interfaceAssemblyName}\" not be found");
            }

            var implementAssembly = RuntimeHelper.GetAssembly(implementAssemblyName);
            if (implementAssembly == null)
            {
                throw new DllNotFoundException($"the dll \"{implementAssemblyName}\" not be found");
            }

            //过滤掉非接口及泛型接口
            var types = interfaceAssembly.GetTypes().Where(t => t.GetTypeInfo().IsInterface && !t.GetTypeInfo().IsGenericType);

            foreach (var type in types)
            {
                //过滤掉抽象类、泛型类以及非class
                var implementType = implementAssembly.DefinedTypes
                    .FirstOrDefault(t => t.IsClass && !t.IsAbstract && !t.IsGenericType &&
                                         t.GetInterfaces().Any(b => b.Name == type.Name));
                if (implementType != null)
                {
                    service.AddTransient(type, implementType.AsType());
                }
            }

            return service;
        }
        public static IServiceCollection AddSingletonAssembly(this IServiceCollection service, string interfaceAssemblyName, string implementAssemblyName)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            if(string.IsNullOrEmpty(interfaceAssemblyName))
                throw new ArgumentNullException(nameof(interfaceAssemblyName));
            if (string.IsNullOrEmpty(implementAssemblyName))
                throw new ArgumentNullException(nameof(implementAssemblyName));

            var interfaceAssembly = RuntimeHelper.GetAssembly(interfaceAssemblyName);
            if (interfaceAssembly == null)
            {
                throw new DllNotFoundException($"the dll \"{interfaceAssemblyName}\" not be found");
            }

            var implementAssembly = RuntimeHelper.GetAssembly(implementAssemblyName);
            if (implementAssembly == null)
            {
                throw new DllNotFoundException($"the dll \"{implementAssemblyName}\" not be found");
            }

            //过滤掉非接口及泛型接口
            var types = interfaceAssembly.GetTypes().Where(t => t.GetTypeInfo().IsInterface && !t.GetTypeInfo().IsGenericType);

            foreach (var type in types)
            {
                //过滤掉抽象类、泛型类以及非class
                var implementType = implementAssembly.DefinedTypes
                    .FirstOrDefault(t => t.IsClass && !t.IsAbstract && !t.IsGenericType &&
                                         t.GetInterfaces().Any(b => b.Name == type.Name));
                if (implementType != null)
                {
                    service.AddSingleton(type, implementType.AsType());
                }
            }

            return service;
        }

        public static IServiceCollection RegisterControllers(this IServiceCollection service,
            string controllerAssemblyName)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            if (string.IsNullOrEmpty(controllerAssemblyName))
                throw new ArgumentNullException(nameof(controllerAssemblyName));
            var controllerAssembly = RuntimeHelper.GetAssembly(controllerAssemblyName);
            if (controllerAssembly == null)
            {
                throw new DllNotFoundException($"the dll \"{controllerAssemblyName}\" not be found");
            }

            //过滤掉非接口及泛型接口
            var types = controllerAssembly.GetTypes().Where(t =>
            {
                var typeInfo = t.GetTypeInfo();
                return typeInfo.IsClass && !typeInfo.IsAbstract && !typeInfo.IsGenericType && t.IsAssignableFrom(typeof(Controller));
            });

            foreach (var type in types)
            {
                service.AddScoped(type);
            }

            return service;
        }
        /// <summary>
        /// 使用CSRedis代替StackChange.Redis
        /// <remarks>
        /// 关于CSRedis项目，请参考<seealso cref="https://github.com/2881099/csredis"/>
        /// </remarks>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="redisConnectionStrings">redis连接字符串。
        /// <remarks>
        /// 如果是单机模式，请只输入一个连接字符串；如果是集群模式，请输入多个连接字符串
        /// </remarks>
        /// </param>
        /// <returns></returns>
        public static IServiceCollection UseCsRedisClient(this IServiceCollection services, params string[] redisConnectionStrings)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (redisConnectionStrings == null || redisConnectionStrings.Length == 0)
            {
                throw new ArgumentNullException(nameof(redisConnectionStrings));
            }
            CSRedisClient redisClient;
            if (redisConnectionStrings.Length == 1)
            {
                //单机模式
                redisClient = new CSRedisClient(redisConnectionStrings[0]);
            }
            else
            {
                //集群模式
                redisClient = new CSRedisClient(NodeRule: null, connectionStrings: redisConnectionStrings);
            }
            //初始化 RedisHelper
            RedisHelper.Initialization(redisClient);
            //注册mvc分布式缓存
            services.AddSingleton<IDistributedCache>(new Microsoft.Extensions.Caching.Redis.CSRedisCache(RedisHelper.Instance));
            return services;
        }
        public static IServiceProvider BuildAutofacServiceProvider(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            return AutofacContainer.Build(services);
        }
        public static IServiceProvider BuildAspectCoreWithAutofacServiceProvider(this IServiceCollection services, Action<IAspectConfiguration> configure = null)
        {
            if(services==null)throw new ArgumentNullException(nameof(services));
            services.ConfigureDynamicProxy(configure);
            services.AddAspectCoreContainer();
            return AutofacContainer.Build(services, configure);
        }

        public static IServiceContainer BuildAspectCoreServiceContainer(this IServiceCollection services,
            Action<IAspectConfiguration> configure = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.ConfigureDynamicProxy(configure);
            services.AddAspectCoreContainer();
            return services.ToServiceContainer();
        }

        public static IServiceProvider BuildAspectCoreServiceProvider(this IServiceCollection services,
            Action<IAspectConfiguration> configure = null)
        {
            return services.BuildAspectCoreServiceContainer(configure).Build();
        }

        public static IServiceCollection AddDbContextFactory(this IServiceCollection services,
            Action<DbContextFactory> action)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            var factory = DbContextFactory.Instance;
            factory.ServiceCollection = services;
            action?.Invoke(factory);

            return factory.ServiceCollection;
        }

        public static IDbContextCore GetDbContext(this IServiceProvider provider, string dbContextTagName)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            return provider.GetServices<IDbContextCore>().FirstOrDefault(m => m.Option.TagName == dbContextTagName);
        }
    }
}
