using System;
using Microsoft.Extensions.DependencyInjection;

namespace Zxw.Framework.NetCore.Helpers
{
    /// <summary>
    /// .Net Core自带的DI辅助类
    /// </summary>
    public class ServiceLocator
    {
        private static IServiceCollection _container;

        static ServiceLocator()
        {
            _container = new ServiceCollection();
        }
        /// <summary>
        /// 创建容器
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection CreateServiceBuilder(IServiceCollection services = null)
        {
            var factory = new DefaultServiceProviderFactory();
            if (services == null) services = _container;
            _container = factory.CreateBuilder(services);
            return _container;
        }

        public static IServiceProvider GetServiceProvider(IServiceCollection container = null)
        {
            return CreateServiceBuilder(container).BuildServiceProvider();
        }

        public static T GetInstance<T>(IServiceProvider provider=null)
        {
            if (provider == null) provider = GetServiceProvider();
            return provider.GetService<T>();
        }
    }
}
