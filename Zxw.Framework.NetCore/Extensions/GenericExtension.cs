using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

namespace Zxw.Framework.NetCore.Extensions
{
    /// <summary>
    /// 泛型扩展
    /// </summary>
    public static class GenericExtension
    {
        public static bool Equal<T>(this T x, T y)
        {
            return ((IComparable)(x)).CompareTo(y) == 0;
        }

        #region ToDictionary

        public static Dictionary<string, string> ToDictionary<T>(this T t, Dictionary<string, string> dic = null) where T : class
        {
            if (dic == null)
                dic = new Dictionary<string, string>();
            var properties = t.GetType().GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(t, null);
                dic.Add(property.Name, value?.ToString() ?? "");
            }
            return dic;
        }

        public static Dictionary<string, string> ToDictionary<TInterface, T>(this TInterface t, Dictionary<string, string> dic = null) where T : class, TInterface
        {
            if (dic == null)
                dic = new Dictionary<string, string>();
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var value = property.GetValue(t, null);
                if (value == null) continue;
                dic.Add(property.Name, value?.ToString() ?? "");
            }
            return dic;
        }

        #endregion

        /// <summary>
        ///     将字符串转换为指定的类型，如果转换不成功，返回默认值。
        /// </summary>
        /// <typeparam name="T">结构体类型或枚举类型</typeparam>
        /// <param name="str">需要转换的字符串</param>
        /// <returns>返回指定的类型。</returns>
        public static T ParseTo<T>(this string str) where T : struct
        {
            return str.ParseTo(default(T));
        }

        /// <summary>
        ///     将字符串转换为指定的类型，如果转换不成功，返回默认值。
        /// </summary>
        /// <typeparam name="T">结构体类型或枚举类型</typeparam>
        /// <param name="str">需要转换的字符串</param>
        /// <param name="defaultValue">如果转换失败，需要使用的默认值</param>
        /// <returns>返回指定的类型。</returns>
        public static T ParseTo<T>(this string str, T defaultValue) where T : struct
        {
            var t = str.ParseToNullable<T>();
            if (t.HasValue)
            {
                return t.Value;
            }
            return defaultValue;
        }

        /// <summary>
        ///     将字符串转换为指定的类型，如果转换不成功，返回null
        /// </summary>
        /// <typeparam name="T">结构体类型或枚举类型</typeparam>
        /// <param name="str">需要转换的字符串</param>
        /// <returns>返回指定的类型</returns>
        public static T? ParseToNullable<T>(this string str) where T : struct
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            var typeFromHandle = typeof(T);
            if (typeFromHandle.IsEnum)
            {
                return str.ToEnum<T>();
            }
            return (T?)str.ParseTo(typeFromHandle.FullName);
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> source)
        {
            DataTable dtReturn = new DataTable();


            if (source == null) return dtReturn;
            // column names 
            PropertyInfo[] oProps = null;

            foreach (var rec in source)
            {
                // Use reflection to get property names, to create table, Only first time, others will follow 
                if (oProps == null)
                {
                    oProps = rec.GetType().GetProperties();
                    foreach (var pi in oProps)
                    {
                        var colType = pi.PropertyType;

                        if (colType.IsNullable())
                        {
                            colType = colType.GetGenericArguments()[0];
                        }
                        if (colType == typeof(Boolean))
                        {
                            colType = typeof(int);
                        }

                        dtReturn.Columns.Add(new DataColumn(pi.Name, colType));
                    }
                }

                var dr = dtReturn.NewRow();

                foreach (var pi in oProps)
                {
                    var value = pi.GetValue(rec, null) ?? DBNull.Value;
                    if (value is bool)
                    {
                        dr[pi.Name] = (bool)value ? 1 : 0;
                    }
                    else
                    {
                        dr[pi.Name] = value;
                    }
                }

                dtReturn.Rows.Add(dr);
            }
            return dtReturn;
        }

        /// <summary>
        /// 快速复制
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TOut FastClone<TIn, TOut>(this TIn source)
        {
            return ObjectFastClone<TIn, TOut>.Clone(source);
        }

        /// <summary>
        /// 对IP地址列表实现排序
        /// </summary>
        /// <param name="ips"></param>
        /// <param name="asc">true为升序，false为降序</param>
        /// <returns></returns>
        public static ICollection<string> Order(this ICollection<string> ips, bool asc = true)
        {
            if (ips == null)
                throw new ArgumentNullException(nameof(ips));
            foreach (var ip in ips)
            {
                IPAddress tmp;
                if (!IPAddress.TryParse(ip, out tmp))
                {
                    throw new Exception("Illegal IPAdress data.");
                }
            }
            Func<string, int, int> func = (s, i) =>
            {
                var tmp = s.Split('.');
                return int.Parse(tmp[i]);
            };
            if (asc)
            {
                return ips.OrderBy(m => func(m, 0))
                    .OrderBy(m => func(m, 1))
                    .OrderBy(m => func(m, 2))
                    .OrderBy(m => func(m, 3))
                    .ToList();
            }
            return ips.OrderByDescending(m => func(m, 3))
                .OrderByDescending(m => func(m, 2))
                .OrderByDescending(m => func(m, 1))
                .OrderByDescending(m => func(m, 0))
                .ToList();
        }

        public static void SaveToCsv<T>(this IEnumerable<T> source, string csvFullName, string separator=",")
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrEmpty(separator))
                separator = ",";
            var csv = string.Join(separator, source);
            using (var sw = new StreamWriter(csvFullName, false))
            {
                sw.Write(csv);
                sw.Close();
            }
        }
    }

    /// <summary>
    /// 运用表达式树实现对象的快速复制
    /// </summary>
    /// <typeparam name="TIn">目标对象</typeparam>
    /// <typeparam name="TOut">输出对象</typeparam>
    public class ObjectFastClone<TIn, TOut>
    {
        private static readonly Func<TIn, TOut> cache = GetFunc();
        private static Func<TIn, TOut> GetFunc()
        {
            var parameterExpression = Expression.Parameter(typeof(TIn), "p");
            var memberBindingList = new List<MemberBinding>();

            foreach (var item in typeof(TOut).GetRuntimeProperties())
            {
                var property = Expression.Property(parameterExpression, typeof(TIn).GetRuntimeProperty(item.Name));
                var memberBinding = Expression.Bind(item, property);
                memberBindingList.Add(memberBinding);
            }

            var memberInitExpression = Expression.MemberInit(Expression.New(typeof(TOut)), memberBindingList.ToArray());
            var lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, parameterExpression);

            return lambda.Compile();
        }

        public static TOut Clone(TIn tIn)
        {
            return cache(tIn);
        }
    }
}
