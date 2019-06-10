using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Zxw.Framework.NetCore.Helpers
{
    /// <summary>
    /// 实体类映射，源自SqlSugar(http://www.codeisbug.com/Doc/3/1113)
    /// </summary>
    public class EntityMapper
    {
        private delegate T MapEntity<T>(DbDataReader dr);

        private static readonly ConcurrentDictionary<Type, Delegate> CachedMappers = new ConcurrentDictionary<Type, Delegate>();

        public static IEnumerable<T> MapToEntities<T>(DbDataReader dr)
        {
            // If a mapping function from dr -> T does not exist, create and cache one
            if (!CachedMappers.ContainsKey(typeof(T)))
            {
                // Our method will take in a single parameter, a DbDataReader
                Type[] methodArgs = {typeof(DbDataReader)};

                // The MapDR method will map a DbDataReader row to an instance of type T
                DynamicMethod dm = new DynamicMethod("MapDR", typeof(T), methodArgs,
                    Assembly.GetExecutingAssembly().GetType().Module);
                ILGenerator il = dm.GetILGenerator();

                // We'll have a single local variable, the instance of T we're mapping
                il.DeclareLocal(typeof(T));

                // Create a new instance of T and save it as variable 0
                il.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Stloc_0);

                foreach (PropertyInfo pi in typeof(T).GetProperties())
                {
                    // Load the T instance, SqlDataReader parameter and the field name onto the stack
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, pi.Name);

                    // Push the column value onto the stack
                    il.Emit(OpCodes.Callvirt, typeof(DbDataReader).GetMethod("get_Item", new Type[] {typeof(string)}));

                    // Depending on the type of the property, convert the datareader column value to the type
                    switch (pi.PropertyType.Name)
                    {
                        case "Int16":
                            il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt16", new Type[] {typeof(object)}));
                            break;
                        case "Int32":
                            il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt32", new Type[] {typeof(object)}));
                            break;
                        case "Int64":
                            il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToInt64", new Type[] {typeof(object)}));
                            break;
                        case "Boolean":
                            il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToBoolean", new Type[] {typeof(object)}));
                            break;
                        case "String":
                            il.Emit(OpCodes.Callvirt, typeof(string).GetMethod("ToString", new Type[] { }));
                            break;
                        case "DateTime":
                            il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToDateTime", new Type[] {typeof(object)}));
                            break;
                        case "Decimal":
                            il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToDecimal", new Type[] {typeof(object)}));
                            break;
                        default:
                            // Don't set the field value as it's an unsupported type
                            continue;
                    }

                    // Set the T instances property value
                    il.Emit(OpCodes.Callvirt, typeof(T).GetMethod("set_" + pi.Name, new Type[] {pi.PropertyType}));
                }

                // Load the T instance onto the stack
                il.Emit(OpCodes.Ldloc_0);

                // Return
                il.Emit(OpCodes.Ret);

                // Cache the method so we won't have to create it again for the type T
                CachedMappers.TryAdd(typeof(T), dm.CreateDelegate(typeof(MapEntity<T>)));
            }

            // Get a delegate reference to the dynamic method
            MapEntity<T> invokeMapEntity = (MapEntity<T>) CachedMappers[typeof(T)];

            // For each row, map the row to an instance of T and yield return it
            while (dr.Read())
                yield return invokeMapEntity(dr);
        }

        public static void ClearCachedMapperMethods()
        {
            CachedMappers.Clear();
        }
    }
}
