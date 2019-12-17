using AspectCore.Extensions.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Zxw.Framework.NetCore.Extensions
{
    public static class ObjectExtension
    {
        public static bool HasProperty<T>(this T obj, string propertyName)
        {
            return obj != null && obj.GetType().GetProperties().Any(p => p.Name.Equals(propertyName));
        }
        /// <summary>
        ///     取得对象指定属性的值
        /// </summary>
        /// <param name="predicate">要取值的属性</param>
        /// <returns></returns>
        public static object GetPropertyValue<T, TProperty>(this T obj, Expression<Func<T, TProperty>> predicate)
        {
            var propertyName = predicate.GetPropertyName(); //属性名称

            return obj.GetPropertyValue(propertyName);
        }

        /// <summary>
        ///     取对象属性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName">支持“.”分隔的多级属性取值。</param>
        /// <returns></returns>
        public static object GetPropertyValue<T>(this T obj, string propertyName)
        {
            var strs = propertyName.Split('.');

            PropertyInfo property = null;
            object value = obj;

            for (var i = 0; i < strs.Length; i++)
            {
                property = value.GetType().GetProperty(strs[i]);
                value = property.GetValue(value, null);
            }
            return value;
        }

        /// <summary>
        ///     设置对象指定属性的值
        /// </summary>
        /// <param name="predicate">要设置值的属性</param>
        /// <param name="value">设置值</param>
        /// <returns>是否设置成功</returns>
        public static bool SetPropertyValue<T, TProperty>(this T obj, Expression<Func<T, TProperty>> predicate,
            object value)
        {
            var propertyName = predicate.GetPropertyName(); //属性名称

            return obj.SetPropertyValue(propertyName, value);
        }

        /// <summary>
        ///     设置对象属性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName">propertyName1.propertyName2.propertyName3</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetPropertyValue<T>(this T obj, string propertyName, object value)
        {
            var strs = propertyName.Split('.');

            PropertyInfo property = null;
            object target = obj;

            for (var i = 0; i < strs.Length; i++)
            {
                property = target.GetType().GetProperty(strs[i]);
                if (i < strs.Length - 1)
                    target = property.GetValue(target, null);
            }

            var flag = false; //设置成功标记
            if (property != null && property.CanWrite)
            {
                if (false == property.PropertyType.IsGenericType) //非泛型
                {
                    if (property.PropertyType.IsEnum)
                    {
                        property.SetValue(target, Convert.ChangeType(value, typeof(int)));
                        flag = true;
                    }
                    else if (value.ToString() != property.PropertyType.ToString())
                    {
                        //property.SetValue(target, string.IsNullOrEmpty(value) ? null : Convert.ChangeType(value, property.PropertyType), null);
                        property.SetValue(target,
                            value == null ? null : Convert.ChangeType(value, property.PropertyType),
                            null);
                        flag = true;
                    }
                }
                else //泛型Nullable<>
                {
                    var genericTypeDefinition = property.PropertyType.GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(Nullable<>))
                    {
                        //property.SetValue(target, string.IsNullOrEmpty(value) ? null : Convert.ChangeType(value, Nullable.GetUnderlyingType(property.PropertyType)), null);
                        property.SetValue(target,
                            value == null
                                ? null
                                : Convert.ChangeType(value, Nullable.GetUnderlyingType(property.PropertyType)),
                            null);
                        flag = true;
                    }
                }
            }

            return flag;
        }

        /// <summary>
        /// 将集合转换为数据集。
        /// </summary>
        /// <typeparam name="T">转换的元素类型。</typeparam>
        /// <param name="list">集合。</param>
        /// <param name="generic">是否生成泛型数据集。</param>
        /// <returns>数据集。</returns>
        public static DataSet ToDataSet<T>(this IEnumerable<T> list, bool generic = true)
        {
            return ListToDataSet(list, generic);
        }
        /// <summary>
        /// DataTable转List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static IList<T> ToList<T>(this DataTable dt) where T:class, new ()
        {    
            // 定义集合    
            IList<T> ts = new List<T>(); 

            if(dt == null || dt.Rows.Count == 0)return ts;     

            string tempName = "";      
      
            foreach (DataRow dr in dt.Rows)      
            {    
                T t = new T();     
                // 获得此模型的公共属性      
                PropertyInfo[] propertys = t.GetType().GetProperties(); 
                foreach (PropertyInfo pi in propertys)      
                {      
                    tempName = pi.Name;  // 检查DataTable是否包含此列    

                    if (!dt.Columns.Contains(tempName))
                    {
                        tempName = pi.GetCustomAttributes<ColumnAttribute>().FirstOrDefault()?.Name;
                    }
   
                    if (tempName!=null && dt.Columns.Contains(tempName))      
                    {      
                        // 判断此属性是否有Setter      
                        if (!pi.CanWrite) continue;         
   
                        object value = dr[tempName];
                        if (value != DBNull.Value)
                        {
                            pi.SetValue(t, ChangeType(value, pi.PropertyType), null);
                        }  
                    }
                }      
                ts.Add(t);      
            }     
            return ts;     
        }

        /// <summary>
        /// 类型转换（包含Nullable<>和非Nullable<>转换）
        /// </summary>
        /// <param name="value"></param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        private static object ChangeType(object value, Type conversionType)
        {
            // Note: This if block was taken from Convert.ChangeType as is, and is needed here since we're
            // checking properties on conversionType below.
            if (conversionType == null)
            {
                throw new ArgumentNullException("conversionType");
            } // end if

            // If it's not a nullable type, just pass through the parameters to Convert.ChangeType

            if (conversionType.IsGenericType &&
                conversionType.IsNullable())
            {
                if (value == null)
                {
                    return null;
                } // end if

                // It's a nullable type, and not null, so that means it can be converted to its underlying type,
                // so overwrite the passed-in conversion type with this underlying type
                System.ComponentModel.NullableConverter nullableConverter = new System.ComponentModel.NullableConverter(conversionType);

                conversionType = nullableConverter.UnderlyingType;
            } // end if

            // Now that we've guaranteed conversionType is something Convert.ChangeType can handle (i.e. not a
            // nullable type), pass the call on to Convert.ChangeType
            return Convert.ChangeType(value, conversionType);
        }
        /// <summary>
        /// 将集合转换为数据集。
        /// </summary>
        /// <param name="list">集合。</param>
        /// <param name="generic">是否生成泛型数据集。</param>
        /// <returns>数据集。</returns>
        public static DataSet ToDataSet(this IEnumerable list, bool generic = true)
        {
            return ListToDataSet(list, generic);
        }

        /// <summary>
        /// 将集合转换为数据集。
        /// </summary>
        /// <typeparam name="T">转换的元素类型。</typeparam>
        /// <param name="list">集合。</param>
        /// <param name="generic">是否生成泛型数据集。</param>
        /// <returns>数据集。</returns>
        public static DataSet ToDataSet<T>(this IEnumerable list, bool generic = true)
        {
            return ListToDataSet(list, typeof(T), generic);
        }

        /// <summary>
        /// 将实例转换为集合数据集。
        /// </summary>
        /// <typeparam name="T">实例类型。</typeparam>
        /// <param name="o">实例。</param>
        /// <param name="generic">是否生成泛型数据集。</param>
        /// <returns>数据集。</returns>
        public static DataSet ToListSet<T>(this T o, bool generic = true)
        {
            if (o is IEnumerable)
            {
                return ListToDataSet(o as IEnumerable, generic);
            }
            else
            {
                return ListToDataSet(new T[] { o }, generic);
            }
        }

        /// <summary>
        /// 将可序列化实例转换为XmlDocument。
        /// </summary>
        /// <typeparam name="T">实例类型。</typeparam>
        /// <param name="o">实例。</param>
        /// <returns>XmlDocument。</returns>
        public static XmlDocument ToXmlDocument<T>(this T o)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.InnerXml = o.ToListSet().GetXml();
            return xmlDocument;
        }

        /// <summary>
        /// 将集合转换为数据集。
        /// </summary>
        /// <param name="list">集合。</param>
        /// <param name="t">转换的元素类型。</param>
        /// <param name="generic">是否生成泛型数据集。</param>
        /// <returns>转换后的数据集。</returns>
        private static DataSet ListToDataSet(IEnumerable list, Type t, bool generic)
        {
            DataSet ds = new DataSet("Data");
            if (t == null)
            {
                if (list != null)
                {
                    foreach (var i in list)
                    {
                        if (i == null)
                        {
                            continue;
                        }
                        t = i.GetType();
                        break;
                    }
                }
                if (t == null)
                {
                    return ds;
                }
            }
            ds.Tables.Add(t.Name);
            //如果集合中元素为DataSet扩展涉及到的基本类型时，进行特殊转换。
            if (t.IsValueType || t == typeof(string))
            {
                ds.Tables[0].TableName = "Info";
                ds.Tables[0].Columns.Add(t.Name);
                if (list != null)
                {
                    foreach (var i in list)
                    {
                        DataRow addRow = ds.Tables[0].NewRow();
                        addRow[t.Name] = i;
                        ds.Tables[0].Rows.Add(addRow);
                    }
                }
                return ds;
            }
            //处理模型的字段和属性。
            var fields = t.GetFields();
            var properties = t.GetProperties();
            foreach (var j in fields)
            {
                if (!ds.Tables[0].Columns.Contains(j.Name))
                {
                    if (generic)
                    {
                        ds.Tables[0].Columns.Add(j.Name, j.FieldType);
                    }
                    else
                    {
                        ds.Tables[0].Columns.Add(j.Name);
                    }
                }
            }
            foreach (var j in properties)
            {
                if (!ds.Tables[0].Columns.Contains(j.Name))
                {
                    if (generic)
                    {
                        ds.Tables[0].Columns.Add(j.Name, j.PropertyType);
                    }
                    else
                    {
                        ds.Tables[0].Columns.Add(j.Name);
                    }
                }
            }
            if (list == null)
            {
                return ds;
            }
            //读取list中元素的值。
            foreach (var i in list)
            {
                if (i == null)
                {
                    continue;
                }
                DataRow addRow = ds.Tables[0].NewRow();
                foreach (var j in fields)
                {
                    MemberExpression field = Expression.Field(Expression.Constant(i), j.Name);
                    LambdaExpression lambda = Expression.Lambda(field, new ParameterExpression[] { });
                    Delegate func = lambda.Compile();
                    object value = func.DynamicInvoke();
                    addRow[j.Name] = value;
                }
                foreach (var j in properties)
                {
                    MemberExpression property = Expression.Property(Expression.Constant(i), j);
                    LambdaExpression lambda = Expression.Lambda(property, new ParameterExpression[] { });
                    Delegate func = lambda.Compile();
                    object value = func.DynamicInvoke();
                    addRow[j.Name] = value;
                }
                ds.Tables[0].Rows.Add(addRow);
            }
            return ds;
        }

        /// <summary>
        /// 将集合转换为数据集。
        /// </summary>
        /// <typeparam name="T">转换的元素类型。</typeparam>
        /// <param name="list">集合。</param>
        /// <param name="generic">是否生成泛型数据集。</param>
        /// <returns>数据集。</returns>
        private static DataSet ListToDataSet<T>(IEnumerable<T> list, bool generic)
        {
            return ListToDataSet(list, typeof(T), generic);
        }

        /// <summary>
        /// 将集合转换为数据集。
        /// </summary>
        /// <param name="list">集合。</param>
        /// <param name="generic">是否转换为字符串形式。</param>
        /// <returns>转换后的数据集。</returns>
        private static DataSet ListToDataSet(IEnumerable list, bool generic)
        {
            return ListToDataSet(list, null, generic);
        }

        /// <summary>
        /// 获取DataSet第一表，第一行，第一列的值。
        /// </summary>
        /// <param name="ds">DataSet数据集。</param>
        /// <returns>值。</returns>
        public static object GetData(this DataSet ds)
        {
            if (
                ds == null
                || ds.Tables.Count == 0
                )
            {
                return string.Empty;
            }
            else
            {
                return ds.Tables[0].GetData();
            }
        }

        /// <summary>
        /// 获取DataTable第一行，第一列的值。
        /// </summary>
        /// <param name="dt">DataTable数据集表。</param>
        /// <returns>值。</returns>
        public static object GetData(this DataTable dt)
        {
            if (
                dt.Columns.Count == 0
                || dt.Rows.Count == 0
                )
            {
                return string.Empty;
            }
            else
            {
                return dt.Rows[0][0];
            }
        }

        /// <summary>
        /// 获取DataSet第一个匹配columnName的值。
        /// </summary>
        /// <param name="ds">数据集。</param>
        /// <param name="columnName">列名。</param>
        /// <returns>值。</returns>
        public static object GetData(this DataSet ds, string columnName)
        {
            if (
                ds == null
                || ds.Tables.Count == 0
                )
            {
                return string.Empty;
            }
            foreach (DataTable dt in ds.Tables)
            {
                object o = dt.GetData(columnName);
                if (!string.IsNullOrEmpty(o.ToString()))
                {
                    return o;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取DataTable第一个匹配columnName的值。
        /// </summary>
        /// <param name="dt">数据表。</param>
        /// <param name="columnName">列名。</param>
        /// <returns>值。</returns>
        public static object GetData(this DataTable dt, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return GetData(dt);
            }
            if (
                dt.Columns.Count == 0
                || dt.Columns.IndexOf(columnName) == -1
                || dt.Rows.Count == 0
                )
            {
                return string.Empty;
            }
            return dt.Rows[0][columnName];
        }

        /// <summary>
        /// 将object转换为string类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>string。</returns>
        public static string ToString(this object o, string t)
        {
            string info = string.Empty;
            if (o == null)
            {
                info = t;
            }
            else
            {
                info = o.ToString();
            }
            return info;
        }

        /// <summary>
        /// 将DateTime?转换为string类型信息。
        /// </summary>
        /// <param name="o">DateTime?。</param>
        /// <param name="format">标准或自定义日期和时间格式的字符串。</param>
        /// <param name="t">默认值。</param>
        /// <returns>string。</returns>
        public static string ToString(this DateTime? o, string format, string t)
        {
            string info = string.Empty;
            if (o == null)
            {
                info = t;
            }
            else
            {
                info = o.Value.ToString(format);
            }
            return info;
        }

        /// <summary>
        /// 将TimeSpan?转换为string类型信息。
        /// </summary>
        /// <param name="o">TimeSpan?。</param>
        /// <param name="format">标准或自定义时间格式的字符串。</param>
        /// <param name="t">默认值。</param>
        /// <returns>string。</returns>
        public static string ToString(this TimeSpan? o, string format, string t)
        {
            string info = string.Empty;
            if (o == null)
            {
                info = t;
            }
            else
            {
                info = o.Value.ToString(format);
            }
            return info;
        }

        /// <summary>
        /// 将object转换为截取后的string类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="startIndex">此实例中子字符串的起始字符位置（从零开始）。</param>
        /// <param name="length">子字符串中的字符数。</param>
        /// <param name="suffix">后缀。如果没有截取则不添加。</param>
        /// <returns>截取后的string类型信息。</returns>
        public static string ToSubString(this object o, int startIndex, int length, string suffix = null)
        {
            string inputString = o.ToString(string.Empty);
            startIndex = Math.Max(startIndex, 0);
            startIndex = Math.Min(startIndex, (inputString.Length - 1));
            length = Math.Max(length, 1);
            if (startIndex + length > inputString.Length)
            {
                length = inputString.Length - startIndex;
            }
            if (inputString.Length == startIndex + length)
            {
                return inputString;
            }
            else
            {
                return inputString.Substring(startIndex, length) + suffix;
            }
        }

        /// <summary>
        /// 将object转换为byte类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>byte。</returns>
        public static byte ToByte(this object o, byte t = default(byte))
        {
            byte info;
            if (!byte.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        public static byte[] ToBytes(this object obj)
        {
            if (obj == null)
                return null;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static object ToObject(this byte[] source)
        {
            using (var memStream = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                memStream.Write(source, 0, source.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = bf.Deserialize(memStream);
                return obj;
            }
        }

        /// <summary>
        /// 将object转换为char类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>char。</returns>
        public static char ToChar(this object o, char t = default(char))
        {
            char info;
            if (!char.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为int类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>int。</returns>
        public static int ToInt(this object o, int t = default(int))
        {
            int info;
            if (!int.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为double类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>double。</returns>
        public static double ToDouble(this object o, double t = default(double))
        {
            double info;
            if (!double.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为decimal类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>decimal。</returns>
        public static decimal ToDecimal(this object o, decimal t = default(decimal))
        {
            decimal info;
            if (!decimal.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为float类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>float。</returns>
        public static float ToFloat(this object o, float t = default(float))
        {
            float info;
            if (!float.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为long类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>long。</returns>
        public static long ToLong(this object o, long t = default(long))
        {
            long info;
            if (!long.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为bool类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>bool。</returns>
        public static bool ToBool(this object o, bool t = default(bool))
        {
            bool info;
            if (!bool.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为sbyte类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>sbyte。</returns>
        public static sbyte ToSbyte(this object o, sbyte t = default(sbyte))
        {
            sbyte info;
            if (!sbyte.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为short类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>short。</returns>
        public static short ToShort(this object o, short t = default(short))
        {
            short info;
            if (!short.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为ushort类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>ushort。</returns>
        public static ushort ToUShort(this object o, ushort t = default(ushort))
        {
            ushort info;
            if (!ushort.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为ulong类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>ulong。</returns>
        public static ulong ToULong(this object o, ulong t = default(ulong))
        {
            ulong info;
            if (!ulong.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为Enum[T]类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>Enum[T]。</returns>
        public static T ToEnum<T>(this object o, T t = default(T))
            where T : struct
        {
            T info;
            if (!Enum.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为DateTime类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>DateTime。</returns>
        public static DateTime ToDateTime(this object o, DateTime t = default(DateTime))
        {
            if (t == default(DateTime))
            {
                t = new DateTime(1753, 1, 1);
            }
            DateTime info;
            if (!DateTime.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为TimeSpan类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>TimeSpan。</returns>
        public static TimeSpan ToTimeSpan(this object o, TimeSpan t = default(TimeSpan))
        {
            if (t == default(TimeSpan))
            {
                t = new TimeSpan(0, 0, 0);
            }
            TimeSpan info;
            if (!TimeSpan.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为Guid类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>Guid。</returns>
        public static Guid ToGuid(this object o, Guid t = default(Guid))
        {
            Guid info;
            if (!Guid.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            return info;
        }

        private static Regex BoolRegex = new Regex("(?<info>(true|false))", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// 从object中获取bool类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <returns>bool。</returns>
        public static bool? GetBool(this object o)
        {
            bool info;
            if (!bool.TryParse(BoolRegex.Match(o.ToString(string.Empty)).Groups["info"].Value, out info))
            {
                return null;
            }
            return info;
        }

        private static Regex IntRegex = new Regex("(?<info>-?\\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// 从object中获取int类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <returns>int。</returns>
        public static int? GetInt(this object o)
        {
            int info;
            if (!int.TryParse(IntRegex.Match(o.ToString(string.Empty)).Groups["info"].Value, out info))
            {
                return null;
            }
            return info;
        }

        private static Regex DecimalRegex = new Regex("(?<info>-?\\d+(\\.\\d+)?)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// 从object中获取decimal类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <returns>decimal。</returns>
        public static decimal? GetDecimal(this object o)
        {
            decimal info;
            if (!decimal.TryParse(DecimalRegex.Match(o.ToString(string.Empty)).Groups["info"].Value, out info))
            {
                return null;
            }
            return info;
        }

        /// <summary>
        /// 从object中获取double类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <returns>double。</returns>
        public static double? GetDouble(this object o)
        {
            double info;
            if (!double.TryParse(DecimalRegex.Match(o.ToString(string.Empty)).Groups["info"].Value, out info))
            {
                return null;
            }
            return info;
        }

        /// <summary>
        /// 从object中获取正数信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <returns>decimal。</returns>
        public static decimal? GetPositiveNumber(this object o)
        {
            decimal info;
            if (!decimal.TryParse(DecimalRegex.Match(o.ToString(string.Empty)).Groups["info"].Value, out info))
            {
                return null;
            }
            return Math.Abs(info);
        }

        private static Regex DateTimeRegex = new Regex("(?<info>(((\\d+)[/年-](0?[13578]|1[02])[/月-](3[01]|[12]\\d|0?\\d)[日]?)|((\\d+)[/年-](0?[469]|11)[/月-](30|[12]\\d|0?\\d)[日]?)|((\\d+)[/年-]0?2[/月-](2[0-8]|1\\d|0?\\d)[日]?))(\\s((2[0-3]|[0-1]\\d)):[0-5]\\d:[0-5]\\d)?)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// 从object中获取DateTime?类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <returns>DateTime?。</returns>
        public static DateTime? GetDateTime(this object o)
        {
            DateTime info;
            if (!DateTime.TryParse(DateTimeRegex.Match(o.ToString(string.Empty)).Groups["info"].Value.Replace("年", "-").Replace("月", "-").Replace("/", "-").Replace("日", ""), out info))
            {
                return null;
            }
            return info;
        }

        private static Regex TimeSpanRegex = new Regex("(?<info>-?(\\d+\\.(([0-1]\\d)|(2[0-3])):[0-5]\\d:[0-5]\\d)|((([0-1]\\d)|(2[0-3])):[0-5]\\d:[0-5]\\d)|(\\d+))", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// 从object中获取TimeSpan?类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <returns>TimeSpan?。</returns>
        public static TimeSpan? GetTimeSpan(this object o)
        {
            TimeSpan info;
            if (!TimeSpan.TryParse(TimeSpanRegex.Match(o.ToString(string.Empty)).Groups["info"].Value, out info))
            {
                return null;
            }
            return info;
        }

        private static Regex GuidRegex = new Regex("(?<info>\\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\\}{0,1})", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        /// <summary>
        /// 从object中获取Guid?类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <returns>Guid?。</returns>
        public static Guid? GetGuid(this object o)
        {
            Guid info;
            if (!Guid.TryParse(GuidRegex.Match(o.ToString(string.Empty)).Groups["info"].Value, out info))
            {
                return null;
            }
            return info;
        }

        /// <summary>
        /// 将object转换为SqlServer中的DateTime?类型信息。
        /// </summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>DateTime?。</returns>
        public static DateTime? GetSqlDateTime(this object o, DateTime t = default(DateTime))
        {
            DateTime info;
            if (!DateTime.TryParse(o.ToString(string.Empty), out info))
            {
                info = t;
            }
            if (info < new DateTime(1753, 1, 1) || info > new DateTime(9999, 12, 31))
            {
                return null;
            }
            return info;
        }

        /// <summary>
        /// 读取XElement节点的文本内容。
        /// </summary>
        /// <param name="xElement">XElement节点。</param>
        /// <param name="t">默认值。</param>
        /// <returns>文本内容。</returns>
        public static string Value(this XElement xElement, string t = default(string))
        {
            if (xElement == null)
            {
                return t;
            }
            else
            {
                return xElement.Value;
            }
        }

        /// <summary>
        /// 获取与指定键相关的值。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dictionary">表示键/值对象的泛型集合。</param>
        /// <param name="key">键。</param>
        /// <param name="t">默认值。</param>
        /// <returns>值。</returns>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue t = default(TValue))
        {
            TValue value = default(TValue);
            if (dictionary == null || key == null)
            {
                return t;
            }
            if (!dictionary.TryGetValue(key, out value))
            {
                value = t;
            }
            return value;
        }

        /// <summary>
        /// 获取与指定键相关或者第一个的值。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dictionary">表示键/值对象的泛型集合。</param>
        /// <param name="key">键。</param>
        /// <param name="t">默认值。</param>
        /// <returns>值。</returns>
        public static TValue GetFirstOrDefaultValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue t = default(TValue))
        {
            TValue value = default(TValue);
            if (dictionary == null || key == null)
            {
                return t;
            }
            if (!dictionary.TryGetValue(key, out value))
            {
                if (dictionary.Count() == 0)
                {
                    value = t;
                }
                else
                {
                    value = dictionary.FirstOrDefault().Value;
                }
            }
            return value;
        }

        /// <summary>
        /// 获取具有指定 System.Xml.Linq.XName 的第一个（按文档顺序）子元素。
        /// </summary>
        /// <param name="xContainer">XContainer。</param>
        /// <param name="xName">要匹配的 System.Xml.Linq.XName。</param>
        /// <param name="t">是否返回同名默认值。</param>
        /// <returns>与指定 System.Xml.Linq.XName 匹配的 System.Xml.Linq.XElement，或者为 null。</returns>
        public static XElement Element(this XContainer xContainer, XName xName, bool t)
        {
            XElement info;
            if (xContainer == null)
            {
                info = null;
            }
            else
            {
                info = xContainer.Element(xName);
            }
            if (t && info == null)
            {
                info = new XElement(xName);
            }
            return info;
        }

        /// <summary>
        /// 按文档顺序返回此元素或文档的子元素集合。
        /// </summary>
        /// <param name="xContainer">XContainer。</param>
        /// <param name="t">是否返回非空默认值。</param>
        /// <returns>System.Xml.Linq.XElement 的按文档顺序包含此System.Xml.Linq.XContainer 的子元素，或者非空默认值。</returns>
        public static IEnumerable<XElement> Elements(this XContainer xContainer, bool t)
        {
            IEnumerable<XElement> info;
            if (xContainer == null)
            {
                info = null;
            }
            else
            {
                info = xContainer.Elements();
            }
            if (t && info == null)
            {
                info = new List<XElement>();
            }
            return info;
        }

        /// <summary>
        /// 按文档顺序返回此元素或文档的经过筛选的子元素集合。集合中只包括具有匹配 System.Xml.Linq.XName 的元素。
        /// </summary>
        /// <param name="xContainer">XContainer。</param>
        /// <param name="xName">要匹配的 System.Xml.Linq.XName。</param>
        /// <param name="t">是否返回非空默认值。</param>
        /// <returns>System.Xml.Linq.XElement 的按文档顺序包含具有匹配System.Xml.Linq.XName 的 System.Xml.Linq.XContainer 的子级，或者非空默认值。</returns>
        public static IEnumerable<XElement> Elements(this XContainer xContainer, XName xName, bool t)
        {
            IEnumerable<XElement> info;
            if (xContainer == null)
            {
                info = null;
            }
            else
            {
                info = xContainer.Elements(xName);
            }
            if (t && info == null)
            {
                info = new List<XElement>();
            }
            return info;
        }

        /// <summary>
        /// 删除html标签。
        /// </summary>
        /// <param name="html">输入的字符串。</param>
        /// <returns>没有html标签的字符串。</returns>
        public static string RemoveHTMLTags(this string html)
        {
            return Regex.Replace(Regex.Replace(Regex.Replace((html ?? string.Empty).Replace("&nbsp;", " ").Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\t", " "), "<\\/?[^>]+>", "\r\n"), "(\r\n)+", "\r\n"), "(\\s)+", " ").Trim();
        }

        /// <summary>
        /// 字符串转换为文件名。
        /// </summary>
        /// <param name="s">字符串。</param>
        /// <returns>文件名。</returns>
        public static string ToFileName(this string s)
        {
            return Regex.Replace(s.ToString(string.Empty), @"[\\/:*?<>|]", "_").Replace("\t", " ").Replace("\r\n", " ").Replace("\"", " ");
        }

        /// <summary>
        /// 获取星期一的日期。
        /// </summary>
        /// <param name="dateTime">日期。</param>
        /// <returns>星期一的日期。</returns>
        public static DateTime? GetMonday(this DateTime dateTime)
        {
            return dateTime.AddDays(-1 * (int)dateTime.AddDays(-1).DayOfWeek).ToString("yyyy-MM-dd").GetDateTime();
        }

        /// <summary>
        /// 获取默认非空字符串。
        /// </summary>
        /// <param name="s">首选默认非空字符串。</param>
        /// <param name="args">依次非空字符串可选项。</param>
        /// <returns>默认非空字符串。若无可选项则返回string.Empty。</returns>
        public static string DefaultStringIfEmpty(this string s, params string[] args)
        {
            if (string.IsNullOrEmpty(s))
            {
                foreach (string i in args)
                {
                    if (!string.IsNullOrEmpty(i) && !string.IsNullOrEmpty(i.Trim()))
                    {
                        return i;
                    }
                }
            }
            return (s ?? string.Empty);
        }

        /// <summary>
        /// 对 URL 字符串进行编码。
        /// </summary>
        /// <param name="s">要编码的文本。</param>
        /// <param name="regex">匹配要编码的文本。</param>
        /// <param name="encoding">指定编码方案的 System.Text.Encoding 对象。</param>
        /// <returns>一个已编码的字符串。</returns>
        public static string ToUrlEncodeString(this string s, Regex regex = default(Regex), Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            if (regex == null)
            {
                return HttpUtility.UrlEncode(s, encoding);
            }
            List<string> l = new List<string>();
            foreach (char i in s)
            {
                string t = i.ToString();
                l.Add(regex.IsMatch(t) ? HttpUtility.UrlEncode(t, encoding) : t);
            }
            return string.Join(string.Empty, l);
        }

        /// <summary>
        /// 对 URL 字符串进行编码。
        /// </summary>
        /// <param name="s">要编码的文本。</param>
        /// <param name="regex">匹配要编码的文本。</param>
        /// <param name="encoding">指定编码方案的 System.Text.Encoding 对象。</param>
        /// <returns>一个已编码的字符串。</returns>
        public static string ToUrlEncodeString(this string s, string regex, Encoding encoding = null)
        {
            return ToUrlEncodeString(s, new Regex(regex), encoding);
        }

        /// <summary>
        /// 将日期转换为UNIX时间戳字符串
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToUnixTimeStamp(this DateTime date)
        {
            DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1));
            string timeStamp = date.Subtract(startTime).Ticks.ToString();
            return timeStamp.Substring(0, timeStamp.Length - 7);
        }

        private static Regex MobileRegex = new Regex("^1[3|4|5|7|8][0-9]\\d{4,8}$");
        private static Regex EmailRegex = new Regex("^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+((\\.[a-zA-Z0-9_-]{2,3}){1,2})$");

        /// <summary>
        /// 判断当前字符串是否是移动电话号码
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static bool IsMobile(this string mobile)
        {
            return MobileRegex.IsMatch(mobile);
        }

        /// <summary>
        /// 判断当前字符串是否为邮箱
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsEmail(this string email)
        {
            return EmailRegex.IsMatch(email);
        }

        public static bool IsTask(this Type source)
        {
            return source.BaseType == typeof(Task);
        }

        public static bool In<T>(this T source, ICollection<T> target)
        {
            if (source == null || target == null) return false;
            return target.Contains(source);
        }

        public static bool IsImplement(this Type entityType, Type interfaceType)
        {
            return /*entityType.IsClass && !entityType.IsAbstract &&*/ entityType.GetTypeInfo().GetInterfaces().Any(t =>
                t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == interfaceType);
        }

        public static bool IsSubClassOf(this Type entityType, Type superType)
        {
            return entityType.GetTypeInfo().IsSubclassOf(superType);
        }

                        #region 人民币大写转换
        public static string RMBD(this double num)
        {
            return ((decimal)num).RMBD();
        }

        /// <summary> 
        /// 转换人民币大小金额 
        /// </summary> 
        /// <param name="num">金额</param> 
        /// <returns>返回大写形式</returns> 
        public static string RMBD(this decimal num)
        {
            string str1 = "零壹贰叁肆伍陆柒捌玖";            //0-9所对应的汉字 
            string str2 = "万仟佰拾亿仟佰拾万仟佰拾元角分"; //数字位所对应的汉字 
            string str3 = "";    //从原num值中取出的值 
            string str4 = "";    //数字的字符串形式 
            string str5 = "";  //人民币大写金额形式 
            int i;    //循环变量 
            int j;    //num的值乘以100的字符串长度 
            string ch1 = "";    //数字的汉语读法 
            string ch2 = "";    //数字位的汉字读法 
            int nzero = 0;  //用来计算连续的零值是几个 
            int temp;            //从原num值中取出的值 

            num = Math.Round(Math.Abs(num), 2);    //将num取绝对值并四舍五入取2位小数 
            str4 = ((long)(num * 100)).ToString();        //将num乘100并转换成字符串形式 
            j = str4.Length;      //找出最高位 
            if (j > 15) { return "溢出"; }
            str2 = str2.Substring(15 - j);   //取出对应位数的str2的值。如：200.55,j为5所以str2=佰拾元角分 

            //循环取出每一位需要转换的值 
            for (i = 0; i < j; i++)
            {
                str3 = str4.Substring(i, 1);          //取出需转换的某一位的值 
                temp = Convert.ToInt32(str3);      //转换为数字 
                if (i != (j - 3) && i != (j - 7) && i != (j - 11) && i != (j - 15))
                {
                    //当所取位数不为元、万、亿、万亿上的数字时 
                    if (str3 == "0")
                    {
                        ch1 = "";
                        ch2 = "";
                        nzero = nzero + 1;
                    }
                    else
                    {
                        if (str3 != "0" && nzero != 0)
                        {
                            ch1 = "零" + str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            ch1 = str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                    }
                }
                else
                {
                    //该位是万亿，亿，万，元位等关键位 
                    if (str3 != "0" && nzero != 0)
                    {
                        ch1 = "零" + str1.Substring(temp * 1, 1);
                        ch2 = str2.Substring(i, 1);
                        nzero = 0;
                    }
                    else
                    {
                        if (str3 != "0" && nzero == 0)
                        {
                            ch1 = str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            if (str3 == "0" && nzero >= 3)
                            {
                                ch1 = "";
                                ch2 = "";
                                nzero = nzero + 1;
                            }
                            else
                            {
                                if (j >= 11)
                                {
                                    ch1 = "";
                                    nzero = nzero + 1;
                                }
                                else
                                {
                                    ch1 = "";
                                    ch2 = str2.Substring(i, 1);
                                    nzero = nzero + 1;
                                }
                            }
                        }
                    }
                }
                if (i == (j - 11) || i == (j - 3))
                {
                    //如果该位是亿位或元位，则必须写上 
                    ch2 = str2.Substring(i, 1);
                }
                str5 = str5 + ch1 + ch2;

                if (i == j - 1 && str3 == "0")
                {
                    //最后一位（分）为0时，加上“整” 
                    str5 = str5 + '整';
                }
            }
            if (num == 0)
            {
                str5 = "零元整";
            }
            return str5;
        }
        #endregion

        public static T ToObject<T>(this byte[] source)
        {
            return (T) source.ToObject();
        }

        public static string Join(this IEnumerable<object> source, string separator)
        {
            return string.Join(separator, source);
        }
    }
}