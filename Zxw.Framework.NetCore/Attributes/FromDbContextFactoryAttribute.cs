using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Zxw.Framework.NetCore.DbContextCore;
using Zxw.Framework.NetCore.Options;

namespace Zxw.Framework.NetCore.Attributes
{
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field|AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class FromDbContextFactoryAttribute: Attribute
    {
        public string DbContextTagName { get; set; }

        public FromDbContextFactoryAttribute(string tagName)
        {
            DbContextTagName = tagName;
        }
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class FromDbOptionAttribute : Attribute
    {
        public string TagName { get; set; }

        public FromDbOptionAttribute(string tagName)
        {
            TagName = tagName;
        }
    }

    public class FromDbContextFactoryInterceptor : AbstractInterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var impType = context.Implementation.GetType();
            var properties = impType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.IsDefined(typeof(FromDbContextFactoryAttribute))).ToList();
            if (properties.Any())
            {
                var dbContexts = context.ServiceProvider.GetServices<IDbContextCore>().ToList();
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<FromDbContextFactoryAttribute>();
                    var dbContext = dbContexts.FirstOrDefault(m=>m.Option.TagName == attribute.DbContextTagName);
                    property.SetValue(context.Implementation, dbContext);
                }
            }
            return context.Invoke(next);
        }
    }

    public class FromDbOptionInterceptor : AbstractInterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var impType = context.Implementation.GetType();
            var properties = impType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.IsDefined(typeof(FromDbOptionAttribute))).ToList();
            if (properties.Any())
            {
                var options = context.ServiceProvider.GetServices<IOptions<DbContextOption>>().ToList();
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<FromDbOptionAttribute>();
                    var option = options.FirstOrDefault(m => m.Value.TagName == attribute.TagName);
                    property.SetValue(context.Implementation, option);
                }
            }
            return context.Invoke(next);
        }
    }
}
