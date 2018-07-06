using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.Extensions.Autofac;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Zxw.Framework.NetCore.IoC
{
    /// <summary>
    /// Autofac IOC 容器
    /// </summary>
    public class AutofacContainer
    {
        private static ContainerBuilder _builder = new ContainerBuilder();
        private static IContainer _container;
        private static string[] _otherAssembly;
        private static List<Type> _types = new List<Type>();
        private static Dictionary<Type, Type> _dicTypes = new Dictionary<Type, Type>();

        /// <summary>
        /// 注册程序集
        /// </summary>
        /// <param name="assemblies">程序集名称的集合</param>
        public static void Register(params string[] assemblies)
        {
            _otherAssembly = assemblies;
        }

        /// <summary>
        /// 注册类型
        /// </summary>
        /// <param name="types"></param>
        public static void Register(params Type[] types)
        {
            _types.AddRange(types.ToList());
        }
        /// <summary>
        /// 注册程序集。
        /// </summary>
        /// <param name="implementationAssemblyName"></param>
        /// <param name="interfaceAssemblyName"></param>
        public static void Register(string implementationAssemblyName, string interfaceAssemblyName)
        {
            var implementationAssembly = Assembly.Load(implementationAssemblyName);
            var interfaceAssembly = Assembly.Load(interfaceAssemblyName);
            var implementationTypes =
                implementationAssembly.DefinedTypes.Where(t =>
                    t.IsClass && !t.IsAbstract && !t.IsGenericType && !t.IsNested);
            foreach (var type in implementationTypes)
            {
                var interfaceTypeName = interfaceAssemblyName + ".I" + type.Name;
                var interfaceType = interfaceAssembly.GetType(interfaceTypeName);
                if (interfaceType.IsAssignableFrom(type))
                {
                    _dicTypes.Add(interfaceType, type);
                }
            }
        }
        /// <summary>
        /// 注册
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        public static void Register<TInterface, TImplementation>() where TImplementation : TInterface
        {
            _dicTypes.Add(typeof(TInterface), typeof(TImplementation));
        }

        /// <summary>
        /// 注册一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        public static void Register<T>(T instance) where T:class
        {
            _builder.RegisterInstance(instance).SingleInstance();
        }

        /// <summary>
        /// 构建IOC容器，需在各种Register后调用。
        /// </summary>
        public static IServiceProvider Build(IServiceCollection services, Action<IAspectConfiguration> config = null)
        {
            if (_otherAssembly != null)
            {
                foreach (var item in _otherAssembly)
                {
                    _builder.RegisterAssemblyTypes(Assembly.Load(item));
                }
            }

            if (_types != null)
            {
                foreach (var type in _types)
                {
                    _builder.RegisterType(type);
                }
            }

            if (_dicTypes != null)
            {
                foreach (var dicType in _dicTypes)
                {
                    _builder.RegisterType(dicType.Value).As(dicType.Key);
                }
            }

            _builder.Populate(services);
            _builder.RegisterDynamicProxy(config);
            _container = _builder.Build();
            return new AutofacServiceProvider(_container);
        }

        /// <summary>
        /// Resolve an instance of the default requested type from the container
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        public static T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public static T Resolve<T>(params Parameter[] parameters)
        {
            return _container.Resolve<T>(parameters);
        }

        public static object Resolve(Type targetType)
        {
            return _container.Resolve(targetType);
        }

        public static object Resolve(Type targetType, params Parameter[] parameters)
        {
            return _container.Resolve(targetType, parameters);
        }
    }
}