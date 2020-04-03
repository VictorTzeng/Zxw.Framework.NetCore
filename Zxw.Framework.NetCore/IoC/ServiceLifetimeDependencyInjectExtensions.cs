using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                a.FromAssembly().ForEach(services.Add);
                //var types = a.GetTypes().Where(t => t.IsClass && !t.IsAbstract);
                //types.Where(t => t.GetInterface(typeof(ISingletonDependency).Name) != null).ToList().ForEach(t =>
                //{

                //});
                //types.Where(t => t.GetInterface(typeof(IScopedDependency).Name) != null).ToList().ForEach(t =>
                //{

                //});
                //types.Where(t => t.GetInterface(typeof(ITransientDependency).Name) != null).ToList().ForEach(t =>
                //{

                //});
                //a.GetTypes().Where(t => t.IsClass).ToList().ForEach(t =>
                //{
                //    var serviceType = t.GetInterface($"I{t.Name}");
                //    if ((serviceType??t).GetInterface(typeof(ISingletonDependency).Name)!=null)
                //    {
                //        if (serviceType != null)
                //        {
                //            services.AddSingleton(serviceType, t);
                //        }
                //        else
                //        {
                //            services.AddSingleton(t);
                //        }
                //    }
                //    else if ((serviceType ?? t).GetInterface(typeof(IScopedDependency).Name)!=null)
                //    {
                //        if (serviceType != null)
                //        {
                //            services.AddScoped(serviceType, t);
                //        }
                //        else
                //        {
                //            services.AddScoped(t);
                //        }
                //    }
                //    else if ((serviceType ?? t).GetInterface(typeof(ITransientDependency).Name)!=null)
                //    {
                //        if (serviceType != null)
                //        {
                //            services.AddTransient(serviceType, t);
                //        }
                //        else
                //        {
                //            services.AddTransient(t);
                //        }
                //    }
                //});
            });
            return services;
        }

        public static List<ServiceDescriptor> FromAssembly(this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var types = assembly
                            .DefinedTypes
                            .Where(r => !r.IsAbstract && r.IsClass && r.BaseType != null)
                            .Where(r => !r.ImplementedInterfaces.IsNullOrEmpty()).ToList();

            List<ServiceDescriptor> result = new List<ServiceDescriptor>();
            types.ForEach(type =>
            {
                ValidInterfaces(type);

                ServiceLifetime serviceLifetime;
                if (type.IsChildTypeOf<ITransientDependency>())
                    serviceLifetime = ServiceLifetime.Transient;
                else if (type.IsChildTypeOf<ISingletonDependency>())
                    serviceLifetime = ServiceLifetime.Singleton;
                else if (type.IsChildTypeOf<IScopedDependency>())
                    serviceLifetime = ServiceLifetime.Scoped;
                else
                    return;

                var arg1 = type.GetGenericArguments();

                List<Type> services = new List<Type>();
                foreach (var interfaceType in type.ImplementedInterfaces)
                {
                    if (IsConvetionInterfaceType(interfaceType))
                        continue;

                    if (type.IsGenericTypeDefinition)
                    {
                        var arg2 = interfaceType.GetGenericArguments();

                        if (GenericArgumentsIsMatch(arg1, arg2))
                            services.Add(interfaceType.GetGenericTypeDefinition());
                    }
                    else
                        services.Add(interfaceType);
                }

                foreach (var baseType in type.GetAllBaseTypes())
                {
                    if (type.IsGenericTypeDefinition)
                    {
                        var arg2 = baseType.GetGenericArguments();

                        if (GenericArgumentsIsMatch(arg1, arg2))
                            services.Add(baseType.GetGenericTypeDefinition());
                    }
                    else
                        services.Add(baseType);
                }

                services.Add(type);
                services.ForEach(service =>
                {
                    result.Add(ServiceDescriptor.Describe(service, type, serviceLifetime));
                });
            });

            return result;
        }

        /// <summary>
        /// 泛型参数是否匹配
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        static bool GenericArgumentsIsMatch(Type[] arg1, Type[] arg2)
        {
            if (arg1 == null || arg1.Length == 0)
                return false;

            if (arg2 == null || arg2.Length == 0)
                return false;

            if (arg1.Length != arg2.Length)
                return false;

            for (int i = 0; i < arg1.Length; i++)
            {
                if (arg1[i] != arg2[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 是否为约定的接口类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static bool IsConvetionInterfaceType(Type type)
        {
            if (type == typeof(ITransientDependency) ||
                type == typeof(ISingletonDependency) ||
                type == typeof(IScopedDependency))
                return true;
            return false;
        }

        /// <summary>
        /// 验证接口继承
        /// </summary>
        /// <param name="type"></param>
        static void ValidInterfaces(TypeInfo type)
        {
            var convertionInterfaces = type.ImplementedInterfaces.Where(r => IsConvetionInterfaceType(r)).ToList();
            if (convertionInterfaces.Count > 1)
                throw new System.Exception($"convention type:{type} can't inherit from multiple interface:{Environment.NewLine}{convertionInterfaces.Select(r => r.FullName).Join(Environment.NewLine)}");
        }

        private static void FillBaseType(HashSet<Type> results, Type type)
        {
            if (type.BaseType != typeof(object))
            {
                results.Add(type.BaseType);
                FillBaseType(results, type.BaseType);
            }
        }

        /// <summary>
        /// 获取所有的父级类型,不包含接口和object
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static HashSet<Type> GetAllBaseTypes(this Type type)
        {
            HashSet<Type> types = new HashSet<Type>();
            FillBaseType(types, type);
            return types;
        }
    }
}
