using System;

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
    }
}
