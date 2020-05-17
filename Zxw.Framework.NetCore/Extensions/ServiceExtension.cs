using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using CSRedis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Zxw.Framework.NetCore.Cache;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Helpers;
using Zxw.Framework.NetCore.IDbContext;
using Zxw.Framework.NetCore.IoC;
using Zxw.Framework.NetCore.Options;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Zxw.Framework.NetCore.Web;
using Zxw.Framework.NetCore.Attributes;
using AspectCore.Extensions.Reflection;

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
            services.AddSingleton<IDistributedCacheManager, DistributedCacheManager>();
            return services;
        }
        public static IServiceProvider BuildAutofacServiceProvider(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            return ServiceLocator.Current = AutofacContainer.Build(services);
        }
        public static IServiceProvider BuildAspectCoreWithAutofacServiceProvider(this IServiceCollection services, Action<IAspectConfiguration> configure = null)
        {
            if(services==null)throw new ArgumentNullException(nameof(services));
            if (configure == null)
            {
                configure = config =>
                {
                    config.Interceptors.AddTyped<FromDbContextFactoryInterceptor>();
                };
            }
            services.ConfigureDynamicProxy(configure);
            return ServiceLocator.Current = AutofacContainer.Build(services, configure);
        }

        public static IServiceContext BuildAspectCoreServiceContainer(this IServiceCollection services,
            Action<IAspectConfiguration> configure = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configure == null)
            {
                configure = config =>
                {
                    config.Interceptors.AddTyped<FromDbContextFactoryInterceptor>();
                };
            }
            services.AddAspectServiceContext();
            services.ConfigureDynamicProxy(configure);
            return services.ToServiceContext();
        }

        public static IServiceProvider BuildAspectCoreServiceProvider(this IServiceCollection services,
            Action<IAspectConfiguration> configure = null)
        {
            if (configure == null)
            {
                configure = config =>
                {
                    config.Interceptors.AddTyped<FromDbContextFactoryInterceptor>();
                };
            }
            return ServiceLocator.Current = AspectCoreContainer.BuildServiceProvider(services, configure);
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

        public static object GetDbContext(this IServiceProvider provider, string dbContextTagName, Type serviceType)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            var implService = provider.GetRequiredService(serviceType); 
            var option = provider.GetServices<DbContextOption>().FirstOrDefault(m => m.TagName == dbContextTagName);

            var context = Activator.CreateInstance(implService.GetType(), option);

            return context;
        }

        public static IServiceCollection AddDbContext<IT, T>(this IServiceCollection services, string tag,
            string connectionString) where IT:class,IDbContextCore where T:BaseDbContext,IT
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            return services.AddDbContext<IT, T>(new DbContextOption()
            {
                TagName = tag,
                ConnectionString = connectionString
            });
        }

        public static IServiceCollection AddDbContext<IT, T>(this IServiceCollection services, DbContextOption option) where IT:IDbContextCore where T:BaseDbContext,IT
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (option == null) throw new ArgumentNullException(nameof(option));
            //services.Configure<DbContextOption>(options =>
            //{
            //    options.IsOutputSql = option.IsOutputSql;
            //    options.ConnectionString = option.ConnectionString;
            //    options.ModelAssemblyName = option.ModelAssemblyName;
            //    options.TagName = option.TagName;
            //});
            services.AddSingleton(option);
            return services.AddDbContext<IT, T>();
        }
        /// <summary>
        /// 添加自定义Controller。自定义controller项目对应的dll必须复制到程序运行目录
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="controllerAssemblyName">自定义controller文件的名称，比如：xxx.Controllers.dll</param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IMvcBuilder AddCustomController(this IMvcBuilder builder, string controllerAssemblyName,
            Func<TypeInfo, bool> filter = null)
        {
            if (filter == null)
                filter = m => true;
            return builder.ConfigureApplicationPartManager(m =>
            {
                var feature = new ControllerFeature();
                m.ApplicationParts.Add(new AssemblyPart(Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory+controllerAssemblyName)));
                m.PopulateFeature(feature);
                builder.Services.AddSingleton(feature.Controllers.Where(filter).Select(t => t.AsType()).ToArray());
            });
        }
        /// <summary>
        /// 添加自定义Controller
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="controllerAssemblyDir">Controller文件所在路径</param>
        /// <param name="controllerAssemblyName">Controller文件名称，比如：xxx.Controllers.dll</param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IMvcBuilder AddCustomController(this IMvcBuilder builder, string controllerAssemblyDir, string controllerAssemblyName,
            Func<TypeInfo, bool> filter = null)
        {
            if (filter == null)
                filter = m => true;
            return builder.ConfigureApplicationPartManager(m =>
            {
                var feature = new ControllerFeature();
                m.ApplicationParts.Add(
                    new AssemblyPart(Assembly.LoadFile(Path.Combine(controllerAssemblyDir, controllerAssemblyName))));
                m.PopulateFeature(feature);
                builder.Services.AddSingleton(feature.Controllers.Where(filter).Select(t => t.AsType()).ToArray());
            });
        }

        public static IServiceCollection AddDefaultWebContext(this IServiceCollection services)
        {
            return services.AddSingleton<IWebContext, WebContext>();
        }

        public static IServiceCollection AddWebContext<T>(this IServiceCollection services) where T:WebContext
        {
            services.Remove(new ServiceDescriptor(typeof(IWebContext), typeof(WebContext), ServiceLifetime.Singleton));
            return services.AddSingleton<IWebContext, T>();
        }

        /// <summary>
        /// 框架入口。默认开启注入实现了ISingletonDependency、IScopedDependency、ITransientDependency三种不同生命周期的类，以及AddHttpContextAccessor和AddDataProtection。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="aspectConfig"></param>
        /// <returns></returns>
        public static IServiceProvider AddCoreX(this IServiceCollection services, Action<IServiceCollection> config = null, Action<IAspectConfiguration> aspectConfig = null)
        {
            config?.Invoke(services);
            services.RegisterServiceLifetimeDependencies();
            services.AddHttpContextAccessor();
            services.AddDataProtection();
            services.AddDefaultWebContext();
            if (aspectConfig == null)
            {
                aspectConfig = config =>
                {
                    config.Interceptors.AddTyped<FromDbContextFactoryInterceptor>();
                };
            }
            return services.BuildAspectCoreServiceProvider(aspectConfig);
        }
    }
}
