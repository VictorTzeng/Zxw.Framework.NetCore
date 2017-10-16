using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;

namespace Zxw.Framework.IoC
{
    /// <summary>
    /// Autofac IOC 容器
    /// </summary>
    public class IoCContainer
    {
        private static ContainerBuilder _builder = new ContainerBuilder();
        private static IContainer _container;
        private static string _controllerAssembly;
        private static string[] _otherAssembly;
        private static List<Type> _types = new List<Type>();
        private static Dictionary<Type, Type> _dicTypes = new Dictionary<Type, Type>();
        private static Dictionary<Type, object> _dicInstances = new Dictionary<Type, object>();

        /// <summary>
        /// 注册控制器
        /// </summary>
        /// <param name="controllerAssembly"></param>
        public static void RegisterControllers(string controllerAssembly)
        {
            _controllerAssembly = controllerAssembly;
        }

        /// <summary>
        /// 注册其他程序集
        /// </summary>
        /// <param name="assembly"></param>
        public static void Register(params string[] assembly)
        {
            _otherAssembly = assembly;
        }

        public static void Register<T>()
        {
            _types.Add(typeof(T));
        }
        public static void Register(params Type[] types)
        {
            _types.AddRange(types.ToList());
        }
        public static void RegisterImplementationAssemblyAsInterface(string implementationAssemblyName, string interfaceAssemblyName)
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

        public static void Register<T>(T instance) where T:class 
        {
            _builder.RegisterInstance(instance);
        }

        /// <summary>
        /// 构建IOC容器
        /// </summary>
        public static void Build()
        {
            //string path = System.Web.HttpContext.Current.Server.MapPath("");

            //_builder.RegisterModule(new ConfigurationSettingsReader("autofac", System.AppDomain.CurrentDomain.BaseDirectory + "\\Bin\\iocconfig.xml"));


            if (_controllerAssembly != null)
            {
                _builder.RegisterControllers(Assembly.Load(_controllerAssembly));
            }

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

            if (_dicInstances!=null)
            {
                foreach (var item in _dicInstances)
                {
                    //var instance = Convert.ChangeType(item.Value, item.Key);
                    _builder.RegisterInstance(item.Value);
                }
            }
            _container = _builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(_container));
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

    }
}