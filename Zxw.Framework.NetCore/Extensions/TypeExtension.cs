using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Zxw.Framework.NetCore.Extensions
{
    /// <summary>
    /// Type类的扩展方法
    /// </summary>
    public static class TypeExtension
    {
        /// <summary>
        /// 判断是否为基础类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsBulitinType(this Type type)
        {
            return (type == typeof(object) || Type.GetTypeCode(type) != TypeCode.Object);
        }

        public static bool IsNullable(this Type type){
            if(type==null) throw new ArgumentNullException(nameof(type));
            return type.IsGenericType && type.GetGenericTypeDefinition()==typeof(Nullable<>);
        }

        /// <summary>
        /// 判断给定的类型是否继承自<paramref name="genericType"/>泛型类型,
        /// <para>
        /// 例typeof(Child).IsChildTypeOfGenericType(typeof(IParent&lt;&gt;));
        /// </para>
        /// </summary>
        /// <param name="childType">子类型</param>
        /// <param name="genericType">泛型父级,例:typeof(IParent&lt;&gt;)</param>
        /// <returns></returns>
        public static bool IsChildTypeOfGenericType(this Type childType, Type genericType)
        {
            var interfaceTypes = childType.GetTypeInfo().ImplementedInterfaces;

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (childType.IsGenericType && childType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = childType.BaseType;
            if (baseType == null) return false;

            return IsChildTypeOfGenericType(baseType, genericType);
        }
        /// <summary>
        /// 获取实现的接口
        /// </summary>
        /// <param name="type"></param>
        /// <param name="includeInherited">是否包含继承父类的接口</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetInterfaces(this Type type, bool includeInherited)
        {
            if (includeInherited || type.BaseType == null)
                return type.GetInterfaces();
            else
                return type.GetInterfaces().Except(type.BaseType.GetInterfaces());
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            return list == null || !list.Any();
        }
        /// <summary>
        /// 是否为<paramref name="parentType"/>的子类
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parentType"></param>
        /// <returns></returns>
        public static bool IsChildTypeOf(this Type type, Type parentType)
        {
            return parentType.IsAssignableFrom(type);
        }

        /// <summary>
        /// 是否为<typeparamref name="T"/>的子类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsChildTypeOf<T>(this Type type)
        {
            return type.IsChildTypeOf(typeof(T));
        }
    }
}
