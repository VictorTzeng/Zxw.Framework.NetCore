using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Zxw.Framework.NetCore.DbContextCore;

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

    public class FromDbContextFactoryService
    {
        IServiceProvider serviceProvider;

        public FromDbContextFactoryService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        Dictionary<Type, Action<object, IServiceProvider>> autowiredActions =
            new Dictionary<Type, Action<object, IServiceProvider>>();

        public void Autowired(object service)
        {
            Autowired(service, serviceProvider);
        }

        /// <summary>
        /// 装配属性和字段
        /// </summary>
        /// <param name="service"></param>
        /// <param name="serviceProvider"></param>
        public void Autowired(object service, IServiceProvider serviceProvider)
        {
            var serviceType = service.GetType();
            if (autowiredActions.TryGetValue(serviceType, out Action<object, IServiceProvider> act))
            {
                act(service, serviceProvider);
            }
            else
            {
                /*
             （obj,sp）=>{
                    ((TService)obj).aa=(TAAType)sp.GetService(aaFieldType);
                    ((TService)obj).bb=(TBBType)sp.GetService(aaFieldType);
                    ...
                }
             */
                //参数
                var objParam = Expression.Parameter(typeof(object), "obj");
                var spParam = Expression.Parameter(typeof(IServiceProvider), "sp");

                var obj = Expression.Convert(objParam, serviceType);
                var GetService =
                    typeof(FromDbContextFactoryService).GetMethod("GetService", BindingFlags.Static | BindingFlags.NonPublic);
                List<Expression> setList = new List<Expression>();

                //字段赋值
                foreach (FieldInfo field in serviceType.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var autowiredAttr = field.GetCustomAttribute<FromDbContextFactoryAttribute>();
                    if (autowiredAttr != null)
                    {
                        var fieldExp = Expression.Field(obj, field);
                        var createService = Expression.Call(null, GetService, spParam,
                            Expression.Constant(field.FieldType), Expression.Constant(autowiredAttr));
                        var setExp = Expression.Assign(fieldExp, Expression.Convert(createService, field.FieldType));
                        setList.Add(setExp);
                    }
                }

                //属性赋值
                foreach (PropertyInfo property in serviceType.GetProperties(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var autowiredAttr = property.GetCustomAttribute<FromDbContextFactoryAttribute>();
                    if (autowiredAttr != null)
                    {
                        var propExp = Expression.Property(obj, property);
                        var createService = Expression.Call(null, GetService, spParam,
                            Expression.Constant(property.PropertyType), Expression.Constant(autowiredAttr));

                        var setExp = Expression.Assign(propExp,
                            Expression.Convert(createService, property.PropertyType));
                        setList.Add(setExp);
                    }
                }

                //构造函数参数复制
                foreach (ConstructorInfo constructor in serviceType.GetConstructors(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var parameters = constructor.GetParameters().Where(m =>
                        m.GetCustomAttributes(typeof(FromDbContextFactoryAttribute)).Any());
                    foreach (var parameter in parameters)
                    {
                        var autowiredAttr = parameter.GetCustomAttribute<FromDbContextFactoryAttribute>();
                        if (autowiredAttr != null)
                        {
                            var propExp = Expression.Parameter(parameters.GetType());
                            var createService = Expression.Call(null, GetService, spParam,
                                Expression.Constant(parameters.GetType()), Expression.Constant(autowiredAttr));

                            var setExp = Expression.Assign(propExp,
                                Expression.Convert(createService, parameters.GetType()));
                            setList.Add(setExp);
                        }
                        
                    }
                }

                //构造函数参数复制
                foreach (MethodInfo method in serviceType.GetMethods(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var parameters = method.GetParameters().Where(m =>
                        m.GetCustomAttributes(typeof(FromDbContextFactoryAttribute)).Any());
                    foreach (var parameter in parameters)
                    {
                        var autowiredAttr = parameter.GetCustomAttribute<FromDbContextFactoryAttribute>();
                        if (autowiredAttr != null)
                        {
                            var propExp = Expression.Parameter(parameters.GetType());
                            var createService = Expression.Call(null, GetService, spParam,
                                Expression.Constant(parameters.GetType()), Expression.Constant(autowiredAttr));

                            var setExp = Expression.Assign(propExp,
                                Expression.Convert(createService, parameters.GetType()));
                            setList.Add(setExp);
                        }
                        
                    }
                }
                var bodyExp = Expression.Block(setList);
                var setAction = Expression.Lambda<Action<object, IServiceProvider>>(bodyExp, objParam, spParam)
                    .Compile();
                autowiredActions[serviceType] = setAction;
                setAction(service, serviceProvider);
            }
        }

        /// <summary>
        /// 根据不同的Identifier获取不同的服务实现
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="serviceType"></param>
        /// <param name="autowiredAttribute"></param>
        /// <returns></returns>
        private static object GetService(IServiceProvider serviceProvider, Type serviceType,
            FromDbContextFactoryAttribute fromDbContextFactoryAttribute)
        {
            if (string.IsNullOrEmpty(fromDbContextFactoryAttribute.DbContextTagName))
            {
                return null;
            }
            var dbContextFactory = (DbContextFactory)serviceProvider.GetService(typeof(DbContextFactory));

            return dbContextFactory.GetDbContext(fromDbContextFactoryAttribute.DbContextTagName);
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
                var factory = context.ServiceProvider.GetRequiredService<DbContextFactory>();
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<FromDbContextFactoryAttribute>();
                    var dbContext = factory.GetDbContext(attribute.DbContextTagName);
                    property.SetValue(context.Implementation, dbContext);
                }
            }
            return context.Invoke(next);
        }
    }
}
